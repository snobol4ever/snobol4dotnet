namespace Snobol4.Common;

/// <summary>
/// Threaded execution engine for SNOBOL4.NET.
/// Accepts a starting instruction index and returns an exit code matching
/// ExecuteLoop's contract: -1 = normal end, -2 = RETURN, -3 = FRETURN, -4 = NRETURN.
/// Safe for recursive calls: all mutable state that must survive a nested
/// call is saved on the C# call stack and restored on return.
/// </summary>
public partial class Executive
{
    internal int ThreadedExecuteLoop(int startAt = 0, bool useFastPath = true, Instruction[]? overrideThread = null)
    {
        var thread    = overrideThread ?? Thread!;
        var varSlots  = Parent.VariableSlots;
        var constPool = Parent.Constants.Pool;
        var funcSlots = Parent.FunctionSlots;

        // Save all fields that recursive calls may overwrite
        var savedIP         = InstructionPointer;
        var savedFailure    = Failure;
        var savedErrorJump  = OnErrorGoto;
        OnErrorGoto = 0;

        // Set the instruction pointer for this call.
        // When running a star sub-expression (overrideThread != null) the entry-label
        // override must be suppressed: the sub-thread starts at index 0 of its own
        // Instruction[], and redirecting IP into StatementInstructionStarts would run
        // the wrong instructions and recurse infinitely via ExecuteProgramDefinedFunction.
        InstructionPointer = startAt;
        if (startAt == 0 && overrideThread == null)
        {
            var entryKey = Parent.FoldCase(Parent.EntryLabel);
            if (!string.IsNullOrEmpty(entryKey))
            {
                var entryStmt = LabelTable[entryKey];
                if (entryStmt >= 0)
                {
                    var starts = Parent.StatementInstructionStarts;
                    if (starts != null && entryStmt < starts.Length)
                        InstructionPointer = starts[entryStmt];
                }
            }
        }

        int  exitCode        = -1;
        bool localSavedFailure = false; // local SaveFailure/RestoreFailure state

        Restart:

        // ── Fast path: all instructions are CallMsil or Halt ────────────────
        // When every opcode in the thread is either CallMsil or Halt, skip the
        // full switch dispatch and spin in a tight loop.  Sub-expression threads
        // compiled by CompileSubExpression still use regular threaded opcodes,
        // so callers that know they are on a sub-thread pass useFastPath:false.
        if (useFastPath && Parent.ThreadIsMsilOnly)
        {
            var msilDelegates = Parent.MsilDelegates;
            while (InstructionPointer >= 0 && InstructionPointer < thread.Length
                   && Parent.ThreadIsMsilOnly)
            {
                var instr = thread[InstructionPointer++];
                if (instr.Op == OpCode.Halt)
                {
                    exitCode = -1;
                    goto Done;
                }
                // instr.Op == OpCode.CallMsil (guaranteed by ThreadIsMsilOnly)
                var next = msilDelegates[instr.IntOperand](this);
                if (_trapTaken) { _trapTaken = false; thread = Thread!; continue; }   // the SETEXIT trap wins over the statement's goto
                if (next == int.MinValue) continue;       // fall through
                if (next < 0) { exitCode = next; goto Done; }
                thread = Thread!;                         // AppendCompile may have extended Thread
                InstructionPointer = next;
            }
            // If ThreadIsMsilOnly went false mid-loop (CODE/AppendCompile at runtime),
            // fall through into the full switch to handle the remaining opcodes.
            if (Parent.ThreadIsMsilOnly) goto Done;  // loop exited normally (IP out of range)
        }
        // ── End fast path ────────────────────────────────────────────────────

#pragma warning disable CS0164  // label present for readability; fast path falls through here
        FullSwitch:
#pragma warning restore CS0164

        while (InstructionPointer >= 0 && InstructionPointer < thread.Length)
        {
            var instr = thread[InstructionPointer++];

            switch (instr.Op)
            {
                case OpCode.Init:
                    BeginStatement(instr.IntOperand);
                    Failure = false;
                    AlphaStack.Clear();
                    BetaStack.Clear();
                    SystemStack.Push(StatementSeparator.Instance);
                    // Monitor bridge — LABEL event (SN-26-bridge-coverage-f).
                    // Use precomputed Execute.SourceStno — Parent.Code is
                    // replaced by EVAL/CODE at runtime; a BlankLineCount
                    // lookup against the swapped SourceLines silently
                    // returns 0 and emits a wrong stno.  See
                    // GOAL-NET-BEAUTY-SELF S-2-bridge-7-fullscan, session
                    // #58.
                    {
                        int idx = instr.IntOperand;
                        long stno = (idx >= 0 && idx < SourceStno.Count)
                            ? SourceStno[idx]
                            : (idx + 1);
                        MonitorIpc.EmitLabel(stno);
                    }
                    var limitPassed = StatementLimitReached();
                    if (limitPassed == StatementLimit.PassedForGood)
                    {
                        LogRuntimeException(244, fatal: true);
                        Failure = true;
                        exitCode = -1;
                        goto Done;
                    }
                    if (limitPassed == StatementLimit.Passed)
                    {
                        // an error like any other: the statement fails and is not counted
                        LogRuntimeException(244);
                        break;
                    }
                    // with &STLIMIT negative statements are not counted, so &STCOUNT neither changes nor traces, as in SPITBOL
                    if (AmpStatementLimit >= 0)
                    {
                        AmpStatementCount++;
                        TraceKeyword("STCOUNT", AmpStatementCount);
                    }
                    // a statement that did not compile is error 7 once it is counted as executed, as in SPITBOL
                    CheckCompiledStatement(instr.IntOperand);
                    break;

                case OpCode.Finalize:
                    while (SystemStack.Peek() is not StatementSeparator)
                        SystemStack.Pop();
                    SystemStack.Pop();
                    if (OnErrorGoto > 0) ProcessTrappedErrorThreaded();
                    break;

                case OpCode.PushVar:
                {
                    // Direct array access — no string hash, no dict lookup.
                    // Expand if BuildEval/BuildCode added slots after init.
                    var slotIdx = instr.IntOperand;
                    if (slotIdx >= VarSlotArray.Length) ExpandVarSlotArray();
                    SystemStack.Push(VarSlotArray[slotIdx]);
                    break;
                }

                case OpCode.PushConst:
                    // Clone to prevent runtime mutation of the shared pool object
                    SystemStack.Push(constPool[instr.IntOperand].Clone());
                    break;

                case OpCode.PushExpr:
                    Constant(StarFunctionList[instr.IntOperand]);
                    break;

                case OpCode.CallFunc:
                    // Function name StringVar was pushed before the args by PushConst.
                    // Function() pops args then the name. On FRETURN, the function
                    // handler sets Failure=true and pushes a sentinel; downstream
                    // operators propagate this correctly via OperatorFast (drain on
                    // failure for arithmetic/concat, ExtractArguments propagation
                    // elsewhere) and ChoiceStart (clears between alternatives).
                    // No skip-to-Finalize: it bypasses unary predicates and choice.
                    // See GOAL-NET-BEAUTY-SELF S-2-bridge-7.
                    Function(instr.IntOperand2);
                    break;

                case OpCode.CallFuncIndirect:
                    // $VAR(args): the indirection result is the function name string.
                    // The stack has: [fn-name-string] [arg0] ... [argN-1]
                    // OpIndirection already ran and pushed the resolved name value.
                    // FunctionIndirect() pops args, then pops the name value and
                    // converts it to string for lookup.  FRETURN handling: same as
                    // CallFunc — operator-level propagation, no skip-to-Finalize.
                    FunctionIndirect(instr.IntOperand2);
                    break;

                case OpCode.OpAdd:       OperatorFast(OpCode.OpAdd,       2); break;
                case OpCode.OpSubtract:  OperatorFast(OpCode.OpSubtract,  2); break;
                case OpCode.OpMultiply:  OperatorFast(OpCode.OpMultiply,  2); break;
                case OpCode.OpDivide:    OperatorFast(OpCode.OpDivide,    2); break;
                case OpCode.OpPower:     OperatorFast(OpCode.OpPower,     2); break;
                case OpCode.OpConcat:    OperatorFast(OpCode.OpConcat,    2); break;
                case OpCode.OpAlt:       OperatorFast(OpCode.OpAlt,       2); break;
                case OpCode.OpPeriod:    OperatorFast(OpCode.OpPeriod,    2); break;
                case OpCode.OpDollar:    OperatorFast(OpCode.OpDollar,    2); break;
                case OpCode.OpQuestion:  OperatorFast(OpCode.OpQuestion,  2); break;
                case OpCode.OpAt:        OperatorFast(OpCode.OpAt,        2); break;
                case OpCode.OpAmpersand: OperatorFast(OpCode.OpAmpersand, 2); break;
                case OpCode.OpPercent:   OperatorFast(OpCode.OpPercent,   2); break;
                case OpCode.OpHash:      OperatorFast(OpCode.OpHash,      2); break;
                case OpCode.OpTilde:     OperatorFast(OpCode.OpTilde,     2); break;

                case OpCode.OpUnaryMinus:    OperatorFast(OpCode.OpUnaryMinus,    1); break;
                case OpCode.OpUnaryPlus:     OperatorFast(OpCode.OpUnaryPlus,     1); break;
                case OpCode.OpIndirection:   OperatorFast(OpCode.OpIndirection,   1); break;
                case OpCode.OpKeyword:       OperatorFast(OpCode.OpKeyword,       1); break;
                case OpCode.OpName:          OperatorFast(OpCode.OpName,          1); break;
                case OpCode.OpNegation:      OperatorFast(OpCode.OpNegation,      0); break;
                case OpCode.OpInterrogation: OperatorFast(OpCode.OpInterrogation, 0); break;
                case OpCode.OpUnaryAt:       OperatorFast(OpCode.OpUnaryAt,       1); break;
                case OpCode.OpUnaryPercent:  OperatorFast(OpCode.OpUnaryPercent,  1); break;
                case OpCode.OpUnaryHash:     OperatorFast(OpCode.OpUnaryHash,     1); break;
                case OpCode.OpUnarySlash:    OperatorFast(OpCode.OpUnarySlash,    1); break;
                case OpCode.OpUnaryOpsyn:    Operator(((StringVar)constPool[instr.IntOperand]).Data, 1); break;

                case OpCode.BinaryEquals:    _BinaryEquals();   break;
                case OpCode.IndexCollection: IndexCollection(instr.IntOperand); break;

                case OpCode.ChoiceStart:
                    // an error that a SETEXIT handler waits for goes no further (ChoiceStart in MsilHelpers)
                    if (Failure && !(FailureIsError && OnErrorGoto > 0)) { SystemStack.Pop(); Failure = false; }
                    break;

                case OpCode.Jump:          InstructionPointer = instr.IntOperand;                     break;
                case OpCode.JumpOnSuccess: if (!Failure) InstructionPointer = instr.IntOperand;       break;
                case OpCode.JumpOnFailure: if  (Failure) InstructionPointer = instr.IntOperand;       break;

                case OpCode.Halt:
                    exitCode = -1;
                    goto Done;

                case OpCode.CallMsil:
                {
                    var next = Parent.MsilDelegates[instr.IntOperand](this);
                    if (_trapTaken) { _trapTaken = false; thread = Thread!; break; }   // the SETEXIT trap wins over the statement's goto
                    if (next == int.MinValue) break;          // fall through — IP already advanced
                    if (next < 0) { exitCode = next; goto Done; } // halt / RETURN / FRETURN etc.
                    thread = Thread!;                         // AppendCompile may have extended Thread
                    InstructionPointer = next;                // explicit jump
                    break;
                }

                case OpCode.SaveFailure:
                    localSavedFailure = Failure;
                    Failure = false;
                    break;

                case OpCode.RestoreFailure:
                    Failure = localSavedFailure;
                    break;

                case OpCode.SetFailure:
                    Failure = true;
                    break;

                case OpCode.ClearFailure:
                    Failure = false;
                    break;

                case OpCode.ReadSubject:
                    ReadInputOperand();
                    break;

                case OpCode.CheckGotoFailure:
                    if (Failure)
                    {
                        ComputedGotoError(20);
                        // LogRuntimeException pushed a StringVar(false) on top of the
                        // label var that GotoIndirect needs. Pop it so dispatch works.
                        SystemStack.Pop();
                    }
                    break;

                case OpCode.GotoIndirect:
                {
                    var sym    = SystemStack.Peek().Symbol;
                    var target = LabelTable[sym];
                    SystemStack.Pop();
                    if (target == -1)                 { exitCode = -1; goto Done; }
                    if (target <= -2 && target >= -7) { exitCode = target; goto Done; }
                    if (target >= 0)
                    {
                        var instrIdx = StatementIndexToInstrIndex(target);
                        if (instrIdx >= 0)
                        {
                            thread = Thread!;  // AppendCompile may have extended Thread
                            InstructionPointer = instrIdx;
                        }
                        else { exitCode = target; goto Done; }
                    }
                    else { ComputedGotoError(instr.IntOperand); if (!_trapTaken) InstructionPointer = -1; }
                    break;
                }

                case OpCode.GotoIndirectCode:
                {
                    if (GotoCode(SystemStack.Pop()) is { } cv)
                    {
                        var target = cv.StatementNumber;
                        if (target == -1)                 { exitCode = -1; goto Done; }
                        if (target <= -2 && target >= -7) { exitCode = target; goto Done; }
                        var instrIdx = StatementIndexToInstrIndex(target);
                        if (instrIdx >= 0)
                        {
                            thread = Thread!;  // AppendCompile may have extended Thread
                            InstructionPointer = instrIdx;
                        }
                        else { exitCode = target; goto Done; } // CODE'd stmt in Statements[]
                    }
                    else { ComputedGotoError(instr.IntOperand); if (!_trapTaken) InstructionPointer = -1; }
                    break;
                }
            }
        }

        Done:
        // A SETEXIT handler's :(CONTINUE) resumes at the failure exit of the statement in error, as in
        // SPITBOL: its :F label, else its unconditional goto, else the next statement
        // :(ABORT) ends the run with the error that is pending, which is then fatal; with none it is error 36, which a handler takes once
        if (exitCode == -5 && overrideThread == null)
        {
            if (_trapPending && Parent.ErrorCodeHistory.Count > 0)
            {
                _trapPending = false;
                LogRuntimeException(Parent.ErrorCodeHistory[^1], fatal: true, AmpErrorType);
            }

            TrappedOrFatalError(36);
            if (_trapTaken)
            {
                _trapTaken = false;
                thread = Thread!;
                exitCode = -1;
                goto Restart;
            }
        }

        // :(CONTINUE) with no trapped error pending is itself error 37, which a SETEXIT handler traps
        if (exitCode == -6 && _continueStatement == -1 && overrideThread == null)
        {
            // struck like SCONTINUE: with no handler armed the statement fails and takes its failure exit, the goto again, and the third
            // strike is fatal
            LogRuntimeException(37, fatal: ++_fatalErrors >= 3);
            SystemStack.Pop();
            if (OnErrorGoto > 0)
            {
                ProcessTrappedErrorThreaded();
                _trapTaken = false;
                thread = Thread!;
                exitCode = -1;
                goto Restart;
            }

            goto Done;
        }

        // :(SCONTINUE) resumes only after a user interrupt (error 320), which this run has none of: with an error pending it is
        // error 331, with none error 321. Both are fatal, as in SPITBOL (l_scn counts them in errft): a SETEXIT handler still
        // takes them, but the third fatal error ends the run whatever the handler and &ERRLIMIT say
        if (exitCode == -7 && overrideThread == null)
        {
            var code = _trapPending ? 331 : 321;
            _trapPending = false;
            _continueStatement = -1;
            LogRuntimeException(code, fatal: ++_fatalErrors >= 3);
            SystemStack.Pop();
            if (OnErrorGoto > 0)
            {
                ProcessTrappedErrorThreaded();
                _trapTaken = false;
                thread = Thread!;
                exitCode = -1;
                goto Restart;
            }

            // no handler is armed: the statement fails and takes its failure exit, which for a goto is the goto again
            goto Done;
        }

        // a failure exit that is an expression (:F($'RETURN')) is evaluated now, as its goto would be, and names the label resumed at
        if (exitCode == -6 && _continueStatement == ContinueAtExpression && overrideThread == null)
            _continueStatement = ContinueExpressionTarget(_trapStatement);

        // one that fails or names no label is a computed goto that cannot be made: its error, and then the same goto as its failure
        // exit, error 332, as in SPITBOL
        if (exitCode == -6 && _continueStatement == ContinueExpressionUnresolved && overrideThread == null)
        {
            _continueStatement = -1;
            _trapPending = false;
            ReportLineIndex = _trapStatement;
            ComputedGotoError(38);
            if (_trapTaken)
            {
                _trapTaken = false;
                thread = Thread!;
                exitCode = -1;
                goto Restart;
            }
        }

        // :(CONTINUE) resumes at the failure exit of the statement in error, which can be a special label (SCONTINUE, RETURN):
        // that label is then taken as the goto it is
        if (exitCode == -6 && _continueStatement is <= -2 and >= -7 && overrideThread == null)
        {
            ReportLineIndex = _trapStatement;
            exitCode = _continueStatement;
            _continueStatement = -1;
            _trapPending = false;
            goto Done;
        }

        // the failure exit that :(CONTINUE) resumes at can be a label that is not there: the goto errs again, and a handler takes it again
        if (exitCode == -6 && _continueStatement == GotoNotFound && overrideThread == null)
        {
            _continueStatement = -1;
            _trapPending = false;
            ReportLineIndex = _trapStatement;
            UndefinedLabelError();
            if (_trapTaken)
            {
                _trapTaken = false;
                thread = Thread!;
                exitCode = -1;
                goto Restart;
            }
        }

        if (exitCode == -6 && _continueStatement >= 0 && overrideThread == null)
        {
            var resumeAt = StatementIndexToInstrIndex(_continueStatement);
            _continueStatement = -1;
            _trapPending = false;
            if (resumeAt >= 0)
            {
                thread = Thread!;
                InstructionPointer = resumeAt;
                exitCode = -1;
                goto Restart;
            }
        }

        // Restore all fields saved at entry so the caller's context is intact.
        // Save the expression's final Failure state first so RunExpressionThread
        // can re-apply it after the restore.
        LastExpressionFailure = Failure;
        InstructionPointer    = savedIP;
        // A sub-expression (EVAL, a deferred *X) has no statement end of its own: a trap its error armed
        // stays pending for the calling statement, as SPITBOL takes it (SETEXIT around EVAL)
        if (overrideThread == null || OnErrorGoto == 0)
            OnErrorGoto = savedErrorJump;
        Failure               = savedFailure;
        return exitCode;
    }

    // :(CONTINUE) is to evaluate the failure exit of the statement in error (Builder.FailureExitExpressions), or found that it fails
    // or names no label
    private const int ContinueAtExpression = -11;
    private const int ContinueExpressionUnresolved = -12;

    // The label the failure exit expression of a statement names, as GotoIndirect resolves it: a statement or a special label
    // (-2 to -7), or ContinueExpressionUnresolved when the expression fails or names no label
    private int ContinueExpressionTarget(int statement)
    {
        if (!Parent.FailureExitExpressions.TryGetValue(statement, out var tokens))
            return statement + 1;
        var depth = SystemStack.Count;
        RunExpressionThread(new ThreadedCodeCompiler(Parent).CompileSubExpression(tokens));
        if (Failure || SystemStack.Count <= depth)
        {
            while (SystemStack.Count > depth)
                SystemStack.Pop();
            Failure = false;
            return ContinueExpressionUnresolved;
        }

        var target = LabelTable[SystemStack.Pop().Symbol];
        while (SystemStack.Count > depth)
            SystemStack.Pop();
        return target == GotoNotFound ? ContinueExpressionUnresolved : target;
    }

    private int StatementIndexToInstrIndex(int stmtIdx)
    {
        if (stmtIdx < 0) return stmtIdx;
        var starts = Parent.StatementInstructionStarts;
        return (starts != null && stmtIdx < starts.Length) ? starts[stmtIdx] : -1;
    }

    private void ProcessTrappedErrorThreaded()
    {
        var target = OnErrorGoto;
        OnErrorGoto  = 0;
        // firing the trap disarms SETEXIT: its stored label is reset to null, so the handler is taken once
        // and SETEXIT then answers the null string, as in SPITBOL
        SetExitLabel = "";
        SetExitNumber = 0;
        // an error of the goto that :(CONTINUE) resumed at is the error of the statement that was in error
        var errorStatement = ReportLineIndex >= 0 ? ReportLineIndex : AmpCurrentLineNumber;
        _trapStatement = errorStatement;
        _continueStatement = Parent.FailureExits.TryGetValue(errorStatement, out var exitLabel) && exitLabel is not null
            ? LabelTable[Parent.FoldCase(exitLabel)]
            : Parent.FailureExitExpressions.ContainsKey(errorStatement) ? ContinueAtExpression : errorStatement + 1;
        InstructionPointer = StatementIndexToInstrIndex(target);
        _trapTaken = true;
        _trapPending = true;
    }

    // An error is waiting for :(CONTINUE) or :(SCONTINUE) (SPITBOL's r_cnt); the failure exit it resumes at can itself be a
    // special label, so the statement index above cannot say
    private bool _trapPending;

    // The fatal errors so far (SPITBOL's errft): a bad SCONTINUE counts as one, and the third ends the run
    private int _fatalErrors;

    // The statement the pending trapped error was in
    private int _trapStatement;

    // Where :(CONTINUE) resumes after the trapped error, by statement index; -1 when no trap is pending
    private int _continueStatement = -1;

    // An error trapped by SETEXIT transfers to its label when the statement ends; a goto the statement's
    // MSIL delegate then returns (:F(OOPS) after a failed EVAL) must not override it, as in SPITBOL
    private bool _trapTaken;
}
