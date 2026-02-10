namespace Snobol4.Common;

public sealed class IntegerCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var intSelf = (IntegerVar)self;
        return new IntegerVar(intSelf.Data);
    }
}