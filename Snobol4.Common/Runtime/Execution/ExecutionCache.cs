namespace Snobol4.Common;

/// <summary>
/// Pre-resolved runtime caches that eliminate hot-path dictionary lookups:
///
///   VarSlotArray    — Var[] indexed by VariableSlot index.
///                     PushVar reads vars[slot] directly, no string hash.
///                     IdentifierTable.SetSlotEntry() keeps this in sync.
///
///   SymbolToSlotIndex — reverse map symbol→slot used by IdentifierTable
///                       setter to decide which VarSlotArray cell to update.
///
///   OperatorHandlers  — FunctionTableEntry.FunctionHandler[] indexed by
///                       OpCode (cast to int).  Operator opcodes call
///                       handlers[op] instead of FunctionTable[string].
///
/// Both caches are initialised once, lazily, on the first call to
/// InitExecutionCache(), which is invoked at the top of ExecuteLoop
/// (i.e. before the very first statement executes).
/// </summary>
public partial class Executive
{
    // -------------------------------------------------------------------------
    // Public fields (read by ThreadedExecuteLoop directly for speed)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runtime variable array.  Index = VariableSlot.SlotIndex.
    /// Each element is the live Var for that slot, identical to the
    /// Var held in IdentifierTable for the same symbol.
    /// Grows via ExpandVarSlotArray() when BuildEval/BuildCode add new slots.
    /// </summary>
    internal Var[] VarSlotArray = [];

    /// <summary>
    /// symbol → slot-index reverse map, copied from Builder.VariableSlotIndex
    /// at init time.  Used by the IdentifierTable setter to propagate writes
    /// back into VarSlotArray in O(1).
    /// </summary>
    internal Dictionary<string, int>? SymbolToSlotIndex;

    /// <summary>
    /// Pre-resolved operator handlers indexed by (int)OpCode.
    /// Slots for non-operator opcodes remain null.
    /// Populated once in InitExecutionCache().
    /// </summary>
    internal FunctionTableEntry.FunctionHandler?[]? OperatorHandlers;

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    private bool _executionCacheReady;

    /// <summary>
    /// Populate VarSlotArray and OperatorHandlers from the current
    /// FunctionTable and VariableSlots.  Called once before the first
    /// threaded execution begins.
    /// </summary>
    internal void InitExecutionCache()
    {
        if (_executionCacheReady) return;
        _executionCacheReady = true;

        // --- Variable slot array -------------------------------------------
        SymbolToSlotIndex = new Dictionary<string, int>(64, StringComparer.Ordinal);
        VarSlotArray      = [];
        ExpandVarSlotArray();

        // --- Operator handler array ----------------------------------------
        // Size to the highest opcode value we use for operators.
        const int size = (int)OpCode.OpUnaryOpsyn + 1;
        OperatorHandlers = new FunctionTableEntry.FunctionHandler?[size];

        void Map(OpCode op, string name)
        {
            if (FunctionTable.TryGetValue(name, out var entry))
                OperatorHandlers[(int)op] = entry.Handler;
        }

        Map(OpCode.OpAdd,       "__+");
        Map(OpCode.OpSubtract,  "__-");
        Map(OpCode.OpMultiply,  "__*");
        Map(OpCode.OpDivide,    "__/");
        Map(OpCode.OpPower,     "__^");
        Map(OpCode.OpConcat,    "___");
        Map(OpCode.OpAlt,       "__|");
        Map(OpCode.OpPeriod,    "__.");
        Map(OpCode.OpDollar,    "__$");
        Map(OpCode.OpQuestion,  "__?");
        Map(OpCode.OpAt,        "__@");
        Map(OpCode.OpAmpersand, "__&");
        Map(OpCode.OpPercent,   "__%");
        Map(OpCode.OpHash,      "__#");
        Map(OpCode.OpTilde,     "__~");

        Map(OpCode.OpUnaryMinus,    "_-");
        Map(OpCode.OpUnaryPlus,     "_+");
        Map(OpCode.OpIndirection,   "_$");
        Map(OpCode.OpKeyword,       "_&");
        Map(OpCode.OpName,          "_.");
        Map(OpCode.OpNegation,      "_~");
        Map(OpCode.OpInterrogation, "_?");
        Map(OpCode.OpUnaryAt,       "_@");
        Map(OpCode.OpUnaryPercent,  "_%");
        Map(OpCode.OpUnaryHash,     "_#");
        Map(OpCode.OpUnarySlash,    "_/");
        // OpUnaryOpsyn uses a dynamic name stored in constPool — handled inline.
    }

    /// <summary>
    /// Expand VarSlotArray to cover any newly-added variable slots.
    /// Called after BuildEval/BuildCode add slots at runtime.
    /// Safe to call multiple times — only extends, never shrinks.
    /// </summary>
    internal void ExpandVarSlotArray()
    {
        var slots = Parent.VariableSlots;
        var start = VarSlotArray!.Length;
        if (slots.Count <= start) return;

        // Grow array
        var newArr = new Var[slots.Count];
        Array.Copy(VarSlotArray, newArr, start);
        VarSlotArray = newArr;

        for (var i = start; i < slots.Count; i++)
        {
            var sym = slots[i].Symbol;
            SymbolToSlotIndex![sym] = i;
            newArr[i] = IdentifierTable[sym];
        }
    }

    // -------------------------------------------------------------------------
    // Fast operator dispatch (replaces Operator(string, int) on hot path)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Dispatch an operator whose handler is already resolved in
    /// OperatorHandlers[op].  Identical logic to Operator() but avoids the
    /// dictionary lookup and the Profiler allocation in Release builds.
    /// </summary>
    internal void OperatorFast(OpCode op, int argumentCount)
    {
        // Reuse the pre-allocated list to avoid per-call heap allocation.
        _reusableArgList.Clear();
        if (SystemStack.ExtractArguments(argumentCount, _reusableArgList, this))
            return;
        InputArguments(_reusableArgList);
        OperatorHandlers![(int)op]!(_reusableArgList);
    }

    /// <summary>
    /// Keep VarSlotArray in sync when a symbol is written into IdentifierTable.
    /// Called by IdentifierTable setter immediately after the base setter.
    /// </summary>
    internal void SyncVarSlot(string symbol, Var value)
    {
        if (SymbolToSlotIndex != null &&
            SymbolToSlotIndex.TryGetValue(symbol, out var idx))
        {
            VarSlotArray[idx] = value;
        }
    }

    // -------------------------------------------------------------------------
    // OPSYN invalidation support
    // -------------------------------------------------------------------------

    /// <summary>
    /// Map from operator function name to OpCode, for OPSYN invalidation.
    /// </summary>
    private static readonly Dictionary<string, OpCode> _operatorNameToOpCode = new(StringComparer.Ordinal)
    {
        ["__+"] = OpCode.OpAdd,
        ["__-"] = OpCode.OpSubtract,
        ["__*"] = OpCode.OpMultiply,
        ["__/"] = OpCode.OpDivide,
        ["__^"] = OpCode.OpPower,
        ["___"] = OpCode.OpConcat,
        ["__|"] = OpCode.OpAlt,
        ["__."] = OpCode.OpPeriod,
        ["__$"] = OpCode.OpDollar,
        ["__?"] = OpCode.OpQuestion,
        ["__@"] = OpCode.OpAt,
        ["__&"] = OpCode.OpAmpersand,
        ["__%"] = OpCode.OpPercent,
        ["__#"] = OpCode.OpHash,
        ["__~"] = OpCode.OpTilde,
        ["_-"]  = OpCode.OpUnaryMinus,
        ["_+"]  = OpCode.OpUnaryPlus,
        ["_$"]  = OpCode.OpIndirection,
        ["_&"]  = OpCode.OpKeyword,
        ["_."]  = OpCode.OpName,
        ["_~"]  = OpCode.OpNegation,
        ["_?"]  = OpCode.OpInterrogation,
        ["_@"]  = OpCode.OpUnaryAt,
        ["_%"]  = OpCode.OpUnaryPercent,
        ["_#"]  = OpCode.OpUnaryHash,
        ["_/"]  = OpCode.OpUnarySlash,
    };

    /// <summary>
    /// Refresh the OperatorHandlers cache entry for <paramref name="operatorName"/>
    /// after OPSYN has modified FunctionTable.  No-op if the name is not
    /// a known operator or the cache hasn't been initialised yet.
    /// </summary>
    internal void InvalidateOperatorHandler(string operatorName)
    {
        if (OperatorHandlers == null) return;
        if (!_operatorNameToOpCode.TryGetValue(operatorName, out var op)) return;
        if (FunctionTable.TryGetValue(operatorName, out var entry))
            OperatorHandlers[(int)op] = entry.Handler;
        else
            OperatorHandlers[(int)op] = null;
    }
}
