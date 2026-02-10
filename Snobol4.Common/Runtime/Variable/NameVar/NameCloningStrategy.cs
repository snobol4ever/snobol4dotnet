namespace Snobol4.Common;

public class NameCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var nameSelf = (NameVar)self;

        return new NameVar(nameSelf.Pointer, nameSelf.Key, nameSelf.Collection)
        {
            Symbol = nameSelf.Symbol,
            InputChannel = nameSelf.InputChannel,
            OutputChannel = nameSelf.OutputChannel
        };
    }
}