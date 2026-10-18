using System.Globalization;

namespace Snobol4.Common;

public sealed class RealFormattingStrategy : IFormattingStrategy
{
    public string ToString(Var self)
    {
        return RealConversionStrategy.SpitbolRealString(((RealVar)self).Data);
    }

    public string DumpString(Var self)
    {
        return $"{ToString(self)}";
    }

    public string DebugVar(Var self)
    {
        var realSelf = (RealVar)self;
        var symbol = realSelf.Symbol.Length == 0 ? "<no name>" : realSelf.Symbol;
        return $"REAL Symbol: {symbol}  Data: {ToString(realSelf)}  Succeeded: {realSelf.Succeeded}";
    }
}