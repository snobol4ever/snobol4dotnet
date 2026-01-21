using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Formatting strategy for expression variables
/// </summary>
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