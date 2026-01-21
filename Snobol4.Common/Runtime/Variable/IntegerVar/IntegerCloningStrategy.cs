using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Cloning strategy for integer variables
/// Creates independent copies of integer values
/// </summary>
public sealed class IntegerCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var intSelf = (IntegerVar)self;
        return new IntegerVar(intSelf.Data);
    }
}