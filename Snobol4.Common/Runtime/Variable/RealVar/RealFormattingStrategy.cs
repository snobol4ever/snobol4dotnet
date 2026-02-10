using System.Globalization;

namespace Snobol4.Common;

public sealed class RealFormattingStrategy : IFormattingStrategy
{

    public string ToString(Var self)
    {
        var realSelf = (RealVar)self;
        return realSelf.Data.ToString(CultureInfo.CurrentCulture);
    }


    public string DumpString(Var self)
    {
        var realSelf = (RealVar)self;
        return $"{realSelf.Data}";
    }

    public string DebugVar(Var self)
    {
        var realSelf = (RealVar)self;
        var symbol = realSelf.Symbol.Length == 0 ? "<no name>" : realSelf.Symbol;
        return $"REAL Symbol: {symbol}  Data: {realSelf.Data}  Succeeded: {realSelf.Succeeded}";
    }
}