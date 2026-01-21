    using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Formatting strategy for integer variables
/// Provides string representations for display and debugging purposes
/// </summary>
public sealed class IntegerFormattingStrategy : IFormattingStrategy
{
    /// <summary>
    /// Returns the integer value as a string using current culture formatting
    /// </summary>

    public string ToString(Var self)
    {
        var intSelf = (IntegerVar)self;
        return intSelf.Data.ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Returns a concise representation of the integer value
    /// Used for dump operations and diagnostics
    /// </summary>

    public string DumpString(Var self)
    {
        var intSelf = (IntegerVar)self;
        return $"{intSelf.Data}";
    }

    /// <summary>
    /// Returns detailed debug information including:
    /// - Symbol name
    /// - Data value
    /// - Success state
    /// </summary>
    public string DebugVar(Var self)
    {
        var intSelf = (IntegerVar)self;
        var symbol = string.IsNullOrEmpty(intSelf.Symbol) ? "<no name>" : intSelf.Symbol;
        return $"INTEGER Symbol: {symbol}, Data: {intSelf.Data}, Succeeded: {intSelf.Succeeded}";
    }
}