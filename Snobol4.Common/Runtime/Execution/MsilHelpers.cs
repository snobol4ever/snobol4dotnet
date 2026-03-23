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
        TraceIdentifierAccess(varName);
        TraceIdentifierValue(varName);
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
        if (Failure) return;

        var slot = Parent.FunctionSlots[slotIdx];
        var entry = FunctionTable[slot.Symbol];
        if (entry == null)
        {
            LogRuntimeException(22);
            return;
        }

        _reusableArgList.Clear();
        if (SystemStack.ExtractArguments(argCount, _reusableArgList, this)) return;

        for (var i = _reusableArgList.Count; i < entry.ArgumentCount; i++)
            _reusableArgList.Add(StringVar.Null());

        InputArguments(_reusableArgList);

        // Add the function name as the last argument, matching Function()'s contract.
        _reusableArgList.Add(new StringVar(slot.Symbol));

        TraceFunctionCall(slot.Symbol);
        entry.Handler(_reusableArgList);
        TraceFunctionReturn(slot.Symbol);
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
        if (Failure) { SystemStack.Pop(); Failure = false; }
    }

    /// <summary>
    /// Statement-boundary initialisation called at the top of every MSIL delegate.
    /// Mirrors the <c>OpCode.Init</c> case in ThreadedExecuteLoop exactly,
    /// including the &amp;STLIMIT check.
    /// </summary>
    /// <returns><c>true</c> if the statement-limit was exceeded and the
    /// delegate should abort immediately.</returns>
    internal bool InitStatementMsil(int stmtIdx)
    {
        AmpCurrentLineNumber = stmtIdx;
        Failure = false;
        AlphaStack.Clear();
        BetaStack.Clear();
        SystemStack.Push(new StatementSeparator());

        if (AmpStatementLimit >= 0) AmpStatementCount++;
        if (AmpStatementLimit > 0 && AmpStatementCount >= AmpStatementLimit)
        {
            LogRuntimeException(244);
            Failure = true;
            return true;   // caller should abort
        }
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
        AmpLastLineNumber = AmpCurrentLineNumber;
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
    internal int ResolveLabel(string sym, int errorCode = 23)
    {
        TraceGoto(sym);
        var target = LabelTable[sym];
        if (target == -1)                 return -1;
        if (target <= -2 && target >= -7) return target;
        if (target >= 0)
        {
            var instrIdx = StatementIndexToInstrIndex(target);
            if (instrIdx >= 0) return instrIdx;
            return target;   // CODE'd statement — caller treats as exitCode
        }
        // GotoNotFound or unknown label
        LogRuntimeException(errorCode);
        return -1;
    }

    /// <summary>
    /// Pop the top of <see cref="SystemStack"/>, look up its Symbol in
    /// <see cref="LabelTable"/>, and return the target instruction pointer.
    /// Mirrors <c>OpCode.GotoIndirect</c> exactly.
    /// </summary>
    internal int ResolveLabelFromStack(int errorCode = 23)
    {
        var sym    = SystemStack.Peek().Symbol;
        var target = LabelTable[sym];
        SystemStack.Pop();
        TraceGoto(sym);
        if (target == -1)                 return -1;
        if (target <= -2 && target >= -7) return target;
        if (target >= 0)
        {
            var instrIdx = StatementIndexToInstrIndex(target);
            if (instrIdx >= 0) return instrIdx;
            return target;
        }
        LogRuntimeException(errorCode);
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
        var sym = SystemStack.Peek().Symbol;
        SystemStack.Pop();
        TraceGoto(sym);
        if (IdentifierTable.TryGetValue(sym, out var v) && v is CodeVar cv)
        {
            var target = cv.StatementNumber;
            if (target == -1)                 return -1;
            if (target <= -2 && target >= -7) return target;
            var instrIdx = StatementIndexToInstrIndex(target);
            if (instrIdx >= 0) return instrIdx;
            return target;
        }
        LogRuntimeException(errorCode);
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
        LogRuntimeException(20);
        SystemStack.Pop();   // pop the StringVar(false) pushed by LogRuntimeException
        return true;
    }
}
