using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Cloning strategy for name variables
/// Creates a copy that points to the same target
/// </summary>
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