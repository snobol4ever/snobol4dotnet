namespace Snobol4.Common;

/// <summary>
/// Pre-resolution pass for the threaded execution engine (Phase 2).
///
/// After Parse() has populated SourceLine.ParseBody and goto token lists,
/// ResolveSlots() walks every token in every parsed statement and builds
/// three tables:
///
///   VariableSlots    — one entry per distinct identifier, holds the
///                      canonical symbol name for fast IdentifierTable access
///
///   FunctionSlots    — one entry per distinct (name, argCount) call site,
///                      holds the canonical name for fast FunctionTable access
///
///   Constants        — one interned Var per distinct literal value, so
///                      threaded execution never allocates per-execution
///
/// The existing C# code generation path is completely unaffected — these
/// tables are built alongside it and used only by the threaded executor.
/// </summary>
public partial class Builder
{
    /// <summary>
    /// Walk all parsed token lists and populate VariableSlots, FunctionSlots,
    /// and Constants.  Call this after Parse() and before code generation.
    /// </summary>
    internal void ResolveSlots()
    {
        foreach (var line in Code.SourceLines)
        {
            ResolveTokenList(line.ParseBody);
            ResolveTokenList(line.ParseSuccessGoto);
            ResolveTokenList(line.ParseFailureGoto);
            ResolveTokenList(line.ParseUnconditionalGoto);
        }

        // Also resolve deferred expressions (star functions used in patterns)
        foreach (var expr in ParseExpression)
        {
            ResolveTokenList(expr);
        }
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private void ResolveTokenList(List<Token> tokens)
    {
        // We need to pair IDENTIFIER_FUNCTION tokens with their following
        // R_PAREN_FUNCTION token to know the argument count at the call site.
        // Walk the list tracking pending function names.
        var pendingFunctions = new Stack<string>();

        for (var i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];
            switch (t.TokenType)
            {
                case Token.Type.IDENTIFIER:
                case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
                    ResolveVariable(t.MatchedString);
                    break;

                case Token.Type.IDENTIFIER_FUNCTION:
                    // Push the function name; we resolve it when we see the
                    // matching R_PAREN_FUNCTION (which carries the arg count).
                    pendingFunctions.Push(t.MatchedString);
                    break;

                case Token.Type.R_PAREN_FUNCTION:
                    if (pendingFunctions.Count > 0)
                    {
                        var funcName = pendingFunctions.Pop();
                        ResolveFunction(funcName, (int)t.IntegerValue);
                    }
                    break;

                case Token.Type.STRING:
                case Token.Type.NULL:
                    Constants.GetOrAddString(t.MatchedString);
                    break;

                case Token.Type.INTEGER:
                    Constants.GetOrAddInteger(t.IntegerValue);
                    break;

                case Token.Type.REAL:
                    Constants.GetOrAddReal(t.DoubleValue);
                    break;

                // Operators are resolved directly from their token type at
                // threaded-compile time, so no slot needed here.
            }
        }
    }

    private void ResolveVariable(string name)
    {
        var canonical = FoldCase(name);
        if (!VariableSlotIndex.TryGetValue(canonical, out _))
        {
            var slot = new VariableSlot(VariableSlots.Count, canonical);
            VariableSlotIndex[canonical] = slot.SlotIndex;
            VariableSlots.Add(slot);
        }
    }

    private void ResolveFunction(string name, int argCount)
    {
        var canonical = FoldCase(name);
        // Key includes arg count: same function name with different arg counts
        // (e.g. variadic functions) gets separate slots.
        var key = $"{canonical}/{argCount}";
        if (!FunctionSlotIndex.TryGetValue(key, out _))
        {
            var slot = new FunctionSlot(FunctionSlots.Count, canonical, argCount);
            FunctionSlotIndex[key] = slot.SlotIndex;
            FunctionSlots.Add(slot);
        }
    }
}
