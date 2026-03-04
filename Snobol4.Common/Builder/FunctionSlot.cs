namespace Snobol4.Common;

/// <summary>
/// A pre-resolved reference to a SNOBOL4 built-in or user-defined function,
/// created at compile time so that threaded execution can dispatch function
/// calls by array index rather than by string dictionary lookup.
///
/// The slot holds the canonical (case-folded) function name and a direct
/// reference to the FunctionTableEntry. The entry reference is filled in
/// during the resolution pass (after Executive.FunctionTable is initialised).
/// For user-defined functions created at runtime via DEFINE(), the entry
/// reference is updated in-place when the function is defined.
///
/// OPSYN safety: the Entry reference is not stored directly. Instead the
/// threaded executor looks up FunctionTable[Symbol] once on first call and
/// caches it. If OPSYN reassigns the slot, the cache is invalidated.
/// </summary>
internal sealed class FunctionSlot
{
    /// <summary>
    /// Index of this slot in Builder.FunctionSlots.
    /// </summary>
    internal int SlotIndex { get; }

    /// <summary>
    /// The canonical (case-folded) function or operator name,
    /// e.g. "ROMAN", "REPLACE", "__+", "_&".
    /// </summary>
    internal string Symbol { get; }

    /// <summary>
    /// Argument count as seen at the call site in source code.
    /// Stored here so the threaded executor doesn't need to pop the
    /// function name off the stack and look it up again.
    /// </summary>
    internal int ArgumentCount { get; }

    internal FunctionSlot(int slotIndex, string symbol, int argumentCount)
    {
        SlotIndex = slotIndex;
        Symbol = symbol;
        ArgumentCount = argumentCount;
    }

    public override string ToString() => $"FuncSlot[{SlotIndex}] \"{Symbol}\"({ArgumentCount})";
}
