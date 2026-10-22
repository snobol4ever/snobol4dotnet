namespace Snobol4.Common;

public sealed class ExpressionCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        // an expression is a value that is shared, not copied: its copy is the same expression
        return ((ExpressionVar)self).Alias();
    }
}