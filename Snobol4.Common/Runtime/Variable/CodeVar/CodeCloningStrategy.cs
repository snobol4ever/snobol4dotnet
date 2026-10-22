namespace Snobol4.Common;

public sealed class CodeCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        // a code is a value that is shared, not copied: its copy is the same code
        return ((CodeVar)self).Alias();
    }
}