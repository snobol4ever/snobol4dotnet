namespace Snobol4.Common;

/// <summary>
/// Helper methods called by MSIL-emitted delegates (DynamicMethod instances
/// created by BuilderEmitMsil).  Each method mirrors the corresponding
/// case in ThreadedExecuteLoop so the JIT-compiled path produces identical
/// results.
/// </summary>
public partial class Executive
{
    /// <summary>
    /// Push the variable at <paramref name="slotIdx"/> onto the system stack.
    /// Mirrors the <c>OpCode.PushVar</c> case in ThreadedExecuteLoop.
    /// </summary>
    internal void PushVarBySlot(int slotIdx)
    {
        if (slotIdx >= VarSlotArray.Length) ExpandVarSlotArray();
        // Fire trace hooks for identifier access and value — mirrors the
        // IdentifierTable indexer path that the threaded PushVar opcode takes.
        var varName = Parent.VariableSlots[slotIdx].Symbol;
        TraceIdentifierAccess(varName, VarSlotArray[slotIdx]);
        SystemStack.Push(VarSlotArray[slotIdx]);
    }

    // The target of a plain assignment, pushed as PushVarBySlot does but as a name: it fires no ACCESS trace
    internal void PushTargetBySlot(int slotIdx)
    {
        if (slotIdx >= VarSlotArray.Length) ExpandVarSlotArray();
        SystemStack.Push(VarSlotArray[slotIdx]);
    }

    /// <summary>
    /// Push a clone of the constant at <paramref name="poolIdx"/> onto the
    /// system stack.  Mirrors the <c>OpCode.PushConst</c> case.
    /// </summary>
    internal void PushConstByIndex(int poolIdx)
    {
        SystemStack.Push(Parent.Constants.Pool[poolIdx].Clone());
    }

    /// <summary>
    /// Call the function whose compile-time slot is <paramref name="slotIdx"/>,
    /// passing <paramref name="argCount"/> arguments already on the stack.
    /// Unlike the threaded <c>CallFunc</c> path, the function-name StringVar is
    /// NOT on the stack — the name is resolved directly from the slot.
    /// Mirrors the logic of <see cref="Function"/> without the name-pop step.
    /// </summary>
    internal void CallFuncBySlot(int slotIdx, int argCount)
    {
        // After a failure in this statement the call is not made, but its arguments still leave the stack
        // and a failure value takes their place, as Function() does; otherwise an outer operator reads a
        // stray argument (-G(REPLACE(F(), 'a', 'b')) negated 'b' when F failed)
        if (Failure)
        {
            for (var i = 0; i < argCount; ++i) SystemStack.Pop();
            PushFailedValue();
            return;
        }

        var slot = Parent.FunctionSlots[slotIdx];
        var entry = FunctionTable[slot.Symbol];

        // the arguments are evaluated before the call is found undefined: an argument that failed fails the statement, an input
        // association is read, and at its end the statement fails with no error 22, as in SPITBOL
        _reusableArgList.Clear();
        if (SystemStack.ExtractArguments(argCount, _reusableArgList, this)) return;
        if (entry == null)
        {
            if (InputArguments(_reusableArgList))
                return;
            LogRuntimeException(22);
            return;
        }

        for (var i = _reusableArgList.Count; i < entry.ArgumentCount; i++)
            _reusableArgList.Add(StringVar.Null());

        if (InputArguments(_reusableArgList))
            return;

        // Add the function name as the last argument, matching Function()'s contract.
        _reusableArgList.Add(new StringVar(slot.Symbol));

        entry.Handler(_reusableArgList);

        // FRETURN propagation: the handler signals failure by pushing a failure
        // sentinel (NonExceptionFailure) and setting Failure=true. The downstream
        // operators in the body propagate this correctly:
        //   * OperatorFast drains arithmetic/concat operands on Failure (BUG-4).
        //   * ExtractArguments returns true for any op whose operand has
        //     Succeeded=false, halting that op before it touches the failed value.
        //   * BinaryEquals / Assign bail at ExtractArguments rather than running.
        //   * Unary predicates `~` and `?` (OpNegation/OpInterrogation) consume
        //     the sentinel themselves (Peek+flip / Pop+rewrap).
        // The MSIL body's `Brtrue earlyExit` after this call jumps past everything
        // to FinalizeStatementMsil, which drains stack to StatementSeparator.
        // The earlier explicit drain here was redundant on the earlyExit path and
        // *destructive* on the predicate path (it removed the sentinel that `~` /
        // `?` need to consume). Removed. See GOAL-NET-BEAUTY-SELF S-2-bridge-7.
    }

    /// <summary>
    /// Evaluate the star-function (deferred expression) at
    /// <paramref name="exprIdx"/> in <see cref="StarFunctionList"/> and push
    /// the result.  Mirrors the <c>OpCode.PushExpr</c> case.
    /// </summary>
    internal void PushExprByIndex(int exprIdx)
    {
        Constant(StarFunctionList[exprIdx]);
    }

    /// <summary>
    /// Handle the start of a choice alternative: if the current execution is
    /// in a failure state, pop the failed value and clear the flag so the
    /// next alternative can be tried.
    /// Mirrors the <c>OpCode.ChoiceStart</c> case in ThreadedExecuteLoop.
    /// </summary>
    internal void ChoiceStart()
    {
        // an error that ends the evaluation (a SETEXIT handler waits for it, or it is in an evaluated expression) goes no further: the
        // next alternative is not tried, as in SPITBOL
        if (Failure && !ErrorEndsEvaluation) { SystemStack.Pop(); Failure = false; }
    }

    /// <summary>
    /// The end of a statement that is only its subject (SUBJECT_VALUE): an input-associated subject reads a record.
    /// Mirrors the <c>OpCode.ReadSubject</c> case in ThreadedExecuteLoop.
    /// </summary>
    internal void ReadSubject() => ReadInputOperand();

    /// <summary>
    /// Statement-boundary initialisation called at the top of every MSIL delegate.
    /// Mirrors the <c>OpCode.Init</c> case in ThreadedExecuteLoop exactly,
    /// including the &amp;STLIMIT check.
    /// </summary>
    /// <returns><c>true</c> if the statement-limit was exceeded and the
    /// delegate should abort immediately.</returns>
    internal bool InitStatementMsil(int stmtIdx)
    {
        BeginStatement(stmtIdx);
        Failure = false;
        AlphaStack.Clear();
        BetaStack.Clear();
        SystemStack.Push(StatementSeparator.Instance);

        // Monitor bridge — LABEL event (SN-26-bridge-coverage-f).
        // Wire payload: 1-based statement number aligned with SPITBOL's
        // &STNO convention.  Use the precomputed Execute.SourceStno —
        // Parent.Code is mutable (EVAL/CODE replace it with a fresh
        // SourceCode), so a runtime BlankLineCount lookup against
        // Parent.Code.SourceLines silently fails after the first EVAL,
        // emitting a wrong stno on the wire.  See GOAL-NET-BEAUTY-SELF
        // S-2-bridge-7-fullscan, session #58.
        long stno = (stmtIdx >= 0 && stmtIdx < SourceStno.Count)
            ? SourceStno[stmtIdx]
            : (stmtIdx + 1);  // fall-back if metadata absent (only on tests)
        MonitorIpc.EmitLabel(stno);

        var limitPassed = StatementLimitReached();
        if (limitPassed == StatementLimit.PassedForGood)
        {
            LogRuntimeException(244, fatal: true);
            Failure = true;
            return true;   // caller should abort
        }

        if (limitPassed == StatementLimit.Passed)
        {
            // an error like any other: the statement fails and is not counted
            LogRuntimeException(244);
            return false;
        }
        // with &STLIMIT negative statements are not counted, so &STCOUNT neither changes nor traces, as in SPITBOL
        if (AmpStatementLimit >= 0)
        {
            AmpStatementCount++;
            TraceKeyword("STCOUNT", AmpStatementCount);
        }
        // a statement that did not compile is error 7 once it is counted as executed, as in SPITBOL
        CheckCompiledStatement(stmtIdx);
        return false;
    }

    /// <summary>
    /// Statement-boundary finalisation called at the bottom of every MSIL delegate.
    /// Mirrors the <c>OpCode.Finalize</c> case in ThreadedExecuteLoop exactly,
    /// including the ErrorJump trap.
    /// </summary>
    internal void FinalizeStatementMsil()
    {
        while (SystemStack.Peek() is not StatementSeparator)
            SystemStack.Pop();
        SystemStack.Pop();
        if (OnErrorGoto > 0) ProcessTrappedErrorThreaded();
    }

    /// <summary>
    /// Resolve a label string to the next instruction pointer the execute loop
    /// should jump to.  Mirrors <c>OpCode.GotoIndirect</c> exactly.
    /// Return value follows delegate conventions:
    ///   &gt;= 0        = jump to this IP
    ///   -1          = end of program (Halt)
    ///   -2 .. -7    = RETURN / FRETURN / NRETURN / etc. exit codes
    ///   int.MinValue = label not found (error already logged)
    /// </summary>
    internal int ResolveLabel(string sym, int errorCode = 38)
    {
        TraceGoto(sym);
        var target = LabelTable[sym];
        if (target == -1)                 return -1;
        if (target <= -2 && target >= -4 && AmpFunctionLevel == 0) return ReturnFromLevelZero();
        if (target <= -2 && target >= -7) return target;
        if (target >= 0)
        {
            var instrIdx = StatementIndexToInstrIndex(target);
            if (instrIdx >= 0) return instrIdx;
            return target;   // CODE'd statement — caller treats as exitCode
        }
        // an error the statement had, which SETEXIT has trapped, takes the statement's place: the goto is not made
        if (_trapTaken)
            return int.MinValue;

        UndefinedLabelError(errorCode);
        return _trapTaken ? int.MinValue : -1;
    }

    // GotoNotFound or unknown label: a goto to a label that is not defined. A SETEXIT handler takes the error as it takes any other, and
    // counts &ERRLIMIT down; with none armed it ends the run, whatever &ERRLIMIT allows, and leaves &ERRLIMIT at 0, as SPITBOL does
    internal void UndefinedLabelError(int errorCode = 38) => TrappedOrFatalError(errorCode);

    // An error that is the run's end unless a SETEXIT handler is armed to take it (the handler is armed once)
    internal void TrappedOrFatalError(int errorCode)
    {
        if (SetExitNumber > 0)
        {
            LogRuntimeException(errorCode);
            if (OnErrorGoto > 0)
                ProcessTrappedErrorThreaded();
            return;
        }

        AmpErrorLimit = 0;
        LogRuntimeException(errorCode, fatal: true);
    }

    // A computed goto that cannot be performed (to no label, to no code, or whose expression fails) is the error it is; with &ERRLIMIT above 0
    // the goto counts as failed and the failure exit is tried, which is the same goto: error 332 ends the run, as sbl's does. A SETEXIT
    // intercept takes the first error instead.
    internal void ComputedGotoError(int code)
    {
        LogRuntimeException(code);
        if (OnErrorGoto > 0)
        {
            // a SETEXIT handler takes the error as it takes any other
            ProcessTrappedErrorThreaded();
            return;
        }

        // no handler: the failure exit of a goto is the goto again, which errs as error 332, a fatal-class error, until the third strike
        // (SPITBOL's errft, shared with SCONTINUE); the first two are trapped while &ERRLIMIT lasts
        while (true)
            LogRuntimeException(332, fatal: ++_fatalErrors >= 3);
    }

    // RETURN, FRETURN and NRETURN outside every function (&FNCLEVEL 0) end the run with error 242
    private int ReturnFromLevelZero()
    {
        AmpErrorLimit = 0;
        LogRuntimeException(242, fatal: true);
        return -1;
    }

    /// <summary>
    /// Pop the top of <see cref="SystemStack"/>, look up its Symbol in
    /// <see cref="LabelTable"/>, and return the target instruction pointer.
    /// Mirrors <c>OpCode.GotoIndirect</c> exactly.
    /// </summary>
    internal int ResolveLabelFromStack(int errorCode = 38)
    {
        var sym    = SystemStack.Peek().Symbol;
        var target = LabelTable[sym];
        SystemStack.Pop();
        TraceGoto(sym);
        if (target == -1)                 return -1;
        if (target <= -2 && target >= -4 && AmpFunctionLevel == 0) return ReturnFromLevelZero();
        if (target <= -2 && target >= -7) return target;
        if (target >= 0)
        {
            var instrIdx = StatementIndexToInstrIndex(target);
            if (instrIdx >= 0) return instrIdx;
            return target;
        }

        ComputedGotoError(errorCode);
        return -1;
    }

    /// <summary>
    /// Pop the top of <see cref="SystemStack"/>, look up its Symbol in
    /// <see cref="IdentifierTable"/> as a <see cref="CodeVar"/>, and return
    /// the target instruction pointer.
    /// Mirrors <c>OpCode.GotoIndirectCode</c> exactly.
    /// </summary>
    internal int ResolveCodeLabelFromStack(int errorCode = 24)
    {
        var operand = SystemStack.Pop();
        var sym = operand.Symbol;
        TraceGoto(sym);
        if (GotoCode(operand) is { } cv)
        {
            var target = cv.StatementNumber;
            if (target == -1)                 return -1;
            if (target <= -2 && target >= -7) return target;
            var instrIdx = StatementIndexToInstrIndex(target);
            if (instrIdx >= 0) return instrIdx;
            return target;
        }
        ComputedGotoError(errorCode);
        return -1;
    }

    /// <summary>
    /// Called after evaluating a goto expression.  If <see cref="Failure"/>
    /// is set the expression failed: log error 20 and pop the extra var the
    /// logger pushed.  Returns <c>true</c> when the goto should be skipped.
    /// </summary>
    internal bool CheckGotoExprFailure()
    {
        if (!Failure) return false;
        ComputedGotoError(20);
        SystemStack.Pop();   // pop the StringVar(false) pushed by LogRuntimeException
        return true;
    }

    // The CODE a direct goto (:<X>) transfers to: the operand's value when it is CODE (:<CODE('...')>
    // has no variable behind it), else the CODE held by the variable it names
    private CodeVar? GotoCode(Var operand) =>
        operand as CodeVar ?? (IdentifierTable.TryGetValue(operand.Symbol, out var v) ? v as CodeVar : null);
}
