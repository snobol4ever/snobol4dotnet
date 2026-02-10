namespace Snobol4.Common;

public class PatternFormattingStrategy : IFormattingStrategy
{
    public string ToString(Var self)
    {
        return "pattern";
    }

    public string DumpString(Var self)
    {
        var patternSelf = (PatternVar)self;

        // Try to provide more detail about the pattern type
        var patternType = patternSelf.Data switch
        {
            AlternatePattern => "|",
            AnyPattern => "any",
            ArbPattern => "arb",
            ArbNoPattern => "arbno",
            BalPattern => "bal",
            BreakPattern => "break",
            ConcatenatePattern => "&",
            AtSign => "@",
            FailPattern => "fail",
            LenPattern => "len",
            LiteralPattern lit => $"'{lit.Literal}'",
            NotAnyPattern => "notany",
            PosPattern => "pos",
            RemPattern => "rem",
            RPosPattern => "rpos",
            RTabPattern => "rtab",
            SpanPattern => "span",
            SucceedPattern => "succeed",
            TabPattern => "tab",
            UnevaluatedPattern => "unevaluated",
            _ => "pattern"
        };

        return $"<{patternType}>";
    }

    public string DebugVar(Var self)
    {
        var patternSelf = (PatternVar)self;
        var symbol = patternSelf.Symbol == "" ? "<no name>" : patternSelf.Symbol;
        var patternType = patternSelf.Data.GetType().Name;
        return $"PATTERN Symbol: {symbol}  Type: {patternType}  Succeeded: {patternSelf.Succeeded}";
    }

}