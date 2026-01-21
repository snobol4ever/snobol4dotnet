using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Cloning strategy for expression variables
/// Creates a copy of the expression variable
/// </summary>
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