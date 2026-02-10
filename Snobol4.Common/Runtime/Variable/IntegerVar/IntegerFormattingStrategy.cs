    using System.Globalization;

    namespace Snobol4.Common;

public sealed class IntegerFormattingStrategy : IFormattingStrategy
{
            
    public string ToString(Var self)
    {
        var intSelf = (IntegerVar)self;
        return intSelf.Data.ToString(CultureInfo.CurrentCulture);
    }

                
    public string DumpString(Var self)
    {
        var intSelf = (IntegerVar)self;
        return $"{intSelf.Data}";
    }

                            public string DebugVar(Var self)
    {
        var intSelf = (IntegerVar)self;
        var symbol = string.IsNullOrEmpty(intSelf.Symbol) ? "<no name>" : intSelf.Symbol;
        return $"INTEGER Symbol: {symbol}, Data: {intSelf.Data}, Succeeded: {intSelf.Succeeded}";
    }
}