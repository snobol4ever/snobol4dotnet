using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Formatting strategy for statement separator
/// </summary>
public sealed class StatementSeparatorFormattingStrategy : IFormattingStrategy
{
    private const string _toStringValue = "<statement-separator>";
    private const string _dumpStringValue = "───";
    private const string _debugStringValue = "STATEMENT SEPARATOR";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(Var self)
    {
        return _toStringValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string DumpString(Var self)
    {
        return _dumpStringValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string DebugString(Var self)
    {
        return _debugStringValue;
    }
}