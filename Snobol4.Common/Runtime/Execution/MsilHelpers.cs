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

        entry.Handler(_reusableArgList);
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
}
