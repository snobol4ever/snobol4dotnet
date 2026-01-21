using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Cloning strategy for real (floating-point) variables
/// </summary>
public sealed class RealCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var realSelf = (RealVar)self;
        return new RealVar(realSelf.Data);
    }
}