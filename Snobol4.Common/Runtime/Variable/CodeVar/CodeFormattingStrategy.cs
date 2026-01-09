using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Formatting strategy for code variables
/// </summary>
public sealed class CodeFormattingStrategy : IFormattingStrategy
{
    private const string _codeTypeString = "code";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(Var self)
    {
        return _codeTypeString;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string DumpString(Var self)
    {
        var codeSelf = (CodeVar)self;
        return $"<code@{codeSelf.StatementNumber}>";
    }

    public string DebugString(Var self)
    {
        var codeSelf = (CodeVar)self;
        var symbol = codeSelf.Symbol.Length == 0 ? "<no name>" : codeSelf.Symbol;
        var preview = codeSelf.Data.Length > 40
            ? string.Concat(codeSelf.Data.AsSpan(0, 40), "...")
            : codeSelf.Data;
        return $"CODE Symbol: {symbol}  StatementNumber: {codeSelf.StatementNumber}  Source: '{preview}'  Succeeded: {codeSelf.Succeeded}";
    }
}