namespace Snobol4.Common;

public sealed class ExpressionCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var expressionSelf = (ExpressionVar)self;

        return new ExpressionVar(expressionSelf.FunctionName)
        {
            Symbol = expressionSelf.Symbol,
            InputChannel = expressionSelf.InputChannel,
            OutputChannel = expressionSelf.OutputChannel
        };
    }
}