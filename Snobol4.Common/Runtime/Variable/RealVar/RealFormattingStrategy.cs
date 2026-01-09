using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Formatting strategy for real (floating-point) variables
/// </summary>
public sealed class RealFormattingStrategy : IFormattingStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(Var self)
    {
        var realSelf = (RealVar)self;
        return realSelf.Data.ToString(CultureInfo.CurrentCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string DumpString(Var self)
    {
        var realSelf = (RealVar)self;
        return $"{realSelf.Data}";
    }

    public string DebugString(Var self)
    {
        var realSelf = (RealVar)self;
        var symbol = realSelf.Symbol.Length == 0 ? "<no name>" : realSelf.Symbol;
        return $"REAL Symbol: {symbol}  Data: {realSelf.Data}  Succeeded: {realSelf.Succeeded}";
    }
}