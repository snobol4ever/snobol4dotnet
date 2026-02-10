namespace Snobol4.Common;

public sealed class StatementSeparatorCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        // Return the singleton instance - statement separators are immutable markers
        return StatementSeparator.Instance;
    }
}