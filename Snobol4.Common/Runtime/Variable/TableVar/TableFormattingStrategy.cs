using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Formatting strategy for table variables
/// Provides string representations for display and debugging purposes
/// </summary>
public sealed class TableFormattingStrategy : IFormattingStrategy
{
    /// <summary>
    /// Returns the type name for standard output
    /// </summary>

    public string ToString(Var self)
    {
        return "table";
    }

    /// <summary>
    /// Returns a concise representation including entry count
    /// Used for dump operations and diagnostics
    /// </summary>

    public string DumpString(Var self)
    {
        var tableSelf = (TableVar)self;
        return $"table({tableSelf.Count})";
    }

    /// <summary>
    /// Returns detailed debug information including:
    /// - Symbol name
    /// - Entry count
    /// - Success state
    /// </summary>
    public string DebugVar(Var self)
    {
        var tableSelf = (TableVar)self;
        var symbol = string.IsNullOrEmpty(tableSelf.Symbol) ? "<no name>" : tableSelf.Symbol;
        return $"TABLE Symbol: {symbol}, Count: {tableSelf.Count}, Succeeded: {tableSelf.Succeeded}, Fill: {tableSelf.Fill.DataType()}";
    }
}