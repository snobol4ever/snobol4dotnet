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
                    SystemStack.Push(new StatementSeparator());
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
                    if (AmpStatementLimit >= 0) AmpStatementCount++;
                    TraceKeyword("STCOUNT", AmpStatementCount);
                    if (AmpStatementLimit > 0 && AmpStatementCount >= AmpStatementLimit)
                    {
                        LogRuntimeException(244);
                        Failure = true;
                        exitCode = -1;
                        goto Done;
                    }
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
                case OpCode.IndexCollection: IndexCollection(); break;

                case OpCode.ChoiceStart:
                    if (Failure) { SystemStack.Pop(); Failure = false; }
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

                case OpCode.CheckGotoFailure:
                    if (Failure)
                    {
                        LogRuntimeException(20);
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
                    else { LogRuntimeException(instr.IntOperand); InstructionPointer = -1; }
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
                    else { LogRuntimeException(instr.IntOperand); InstructionPointer = -1; }
                    break;
                }
            }
        }

        Done:
        // A SETEXIT handler's :(CONTINUE) resumes at the failure exit of the statement in error, as in
        // SPITBOL: its :F label, else its unconditional goto, else the next statement
        if (exitCode == -6 && _continueStatement >= 0 && overrideThread == null)
        {
            var resumeAt = StatementIndexToInstrIndex(_continueStatement);
            _continueStatement = -1;
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
        var errorStatement = AmpCurrentLineNumber;
        _continueStatement = Parent.FailureExits.TryGetValue(errorStatement, out var exitLabel) && exitLabel is not null
            ? LabelTable[Parent.FoldCase(exitLabel)]
            : errorStatement + 1;
        InstructionPointer = StatementIndexToInstrIndex(target);
        _trapTaken = true;
    }

    // Where :(CONTINUE) resumes after the trapped error, by statement index; -1 when no trap is pending
    private int _continueStatement = -1;

    // An error trapped by SETEXIT transfers to its label when the statement ends; a goto the statement's
    // MSIL delegate then returns (:F(OOPS) after a failed EVAL) must not override it, as in SPITBOL
    private bool _trapTaken;
}
