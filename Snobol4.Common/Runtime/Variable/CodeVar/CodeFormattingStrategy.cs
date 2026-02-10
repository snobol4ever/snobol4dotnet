namespace Snobol4.Common;

public sealed class CodeFormattingStrategy : IFormattingStrategy
{
    private const string _codeTypeString = "code";


    public string ToString(Var self)
    {
        return _codeTypeString;
    }


    public string DumpString(Var self)
    {
        var codeSelf = (CodeVar)self;
        return $"<code@{codeSelf.StatementNumber}>";
    }

    public string DebugVar(Var self)
    {
        var codeSelf = (CodeVar)self;
        var symbol = codeSelf.Symbol.Length == 0 ? "<no name>" : codeSelf.Symbol;
        var preview = codeSelf.Data.Length > 40
            ? string.Concat(codeSelf.Data.AsSpan(0, 40), "...")
            : codeSelf.Data;
        return $"CODE Symbol: {symbol}  StatementNumber: {codeSelf.StatementNumber}  Source: '{preview}'  Succeeded: {codeSelf.Succeeded}";
    }
}