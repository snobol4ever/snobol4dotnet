namespace Snobol4.Common;

public sealed class ExpressionFormattingStrategy : IFormattingStrategy
{

    public string ToString(Var self)
    {
        return "expression";
    }


    public string DumpString(Var self)
    {
        return "<expression>";
    }

    public string DebugVar(Var self)
    {
        var expressionSelf = (ExpressionVar)self;
        var symbol = expressionSelf.Symbol.Length == 0 ? "<no name>" : expressionSelf.Symbol;
        var delegateName = expressionSelf.FunctionName.Method.Name;
        return $"EXPRESSION Symbol: {symbol}  Delegate: {delegateName}  Succeeded: {expressionSelf.Succeeded}";
    }
}