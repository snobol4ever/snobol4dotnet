namespace Snobol4.Common;

public sealed class RealCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var realSelf = (RealVar)self;
        return new RealVar(realSelf.Data);
    }
}