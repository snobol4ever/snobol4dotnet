namespace Snobol4.Common;

/// <summary>
/// A pre-resolved reference to a SNOBOL4 variable, created at compile time
/// so that threaded execution can look up variables by array index rather
/// than by string dictionary lookup.
///
/// The slot holds the canonical (case-folded) name of the variable. The
/// actual Var object is not stored here — it lives in Executive.IdentifierTable
/// as always — but the slot index into VariableSlotTable lets the threaded
/// executor call IdentifierTable.GetValueSafe() with a pre-resolved key,
/// bypassing the hash computation and equality comparison that happen on
/// every call to IdentifierTable[string].
/// </summary>
internal sealed class VariableSlot
{
    /// <summary>
    /// Index of this slot in Builder.VariableSlots.
    /// Assigned when the slot is created during the parse pass.
    /// </summary>
    internal int SlotIndex { get; }

    /// <summary>
    /// The canonical (case-folded) symbol name, e.g. "R1", "N", "OUTPUT".
    /// This is the key used in Executive.IdentifierTable.
    /// </summary>
    internal string Symbol { get; }

    internal VariableSlot(int slotIndex, string symbol)
    {
        SlotIndex = slotIndex;
        Symbol = symbol;
    }

    public override string ToString() => $"Slot[{SlotIndex}] \"{Symbol}\"";
}
