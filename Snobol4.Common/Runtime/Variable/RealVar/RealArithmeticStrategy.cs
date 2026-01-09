using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Arithmetic strategy for real (floating-point) variables
/// </summary>
public sealed class RealArithmeticStrategy : IArithmeticStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Var Add(Var self, Var other, Executive executive)
    {
        RealVar realSelf = (RealVar)self;

        // Use double dispatch to get type-specific behavior
        return other.AddReal(realSelf, executive);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Var Subtract(Var self, Var other, Executive executive)
    {
        RealVar realSelf = (RealVar)self;
        return other.SubtractReal(realSelf, executive);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Var Multiply(Var self, Var other, Executive executive)
    {
        RealVar realSelf = (RealVar)self;
        return other.MultiplyReal(realSelf, executive);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Var Divide(Var self, Var other, Executive executive)
    {
        RealVar realSelf = (RealVar)self;
        return other.DivideReal(realSelf, executive);
    }

    public Var Power(Var self, Var other, Executive executive)
    {
        RealVar realSelf = (RealVar)self;

        // Convert other to real if needed using pattern matching
        double exponent = other switch
        {
            IntegerVar intOther => intOther.Data,
            RealVar realOther => realOther.Data,
            _ => HandleInvalidExponent(executive)
        };

        // Check for 0^0 special case
        if (realSelf.Data == 0.0 && exponent == 0.0)
        {
            executive.LogRuntimeException(18);
            return StringVar.Null();
        }

        double result = Math.Pow(realSelf.Data, exponent);

        // Use IsFinite for combined NaN/Infinity check
        if (!double.IsFinite(result))
        {
            int errorCode = double.IsNaN(result) ? 311 : (realSelf.Data == 0.0 ? 18 : 266);
            executive.LogRuntimeException(errorCode);
            return StringVar.Null();
        }

        return new RealVar(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Var Negate(Var self, Executive executive)
    {
        RealVar realSelf = (RealVar)self;
        return new RealVar(-realSelf.Data);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double HandleInvalidExponent(Executive executive)
    {
        executive.LogRuntimeException(17);
        return double.NaN;
    }
}