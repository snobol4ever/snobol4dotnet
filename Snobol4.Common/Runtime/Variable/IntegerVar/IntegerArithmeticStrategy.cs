using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Arithmetic strategy for integer variables
/// Handles binary operations with overflow checking and type promotion
/// </summary>
public sealed class IntegerArithmeticStrategy : IArithmeticStrategy
{
    public Var Add(Var self, Var other, Executive executive)
    {
        var intSelf = (IntegerVar)self;
        // Use double dispatch to get type-specific behavior
        return other.AddInteger(intSelf, executive);
    }

    public Var Subtract(Var self, Var other, Executive executive)
    {
        var intSelf = (IntegerVar)self;
        return other.SubtractInteger(intSelf, executive);
    }

    public Var Multiply(Var self, Var other, Executive executive)
    {
        var intSelf = (IntegerVar)self;
        return other.MultiplyInteger(intSelf, executive);
    }

    public Var Divide(Var self, Var other, Executive executive)
    {
        var intSelf = (IntegerVar)self;
        return other.DivideInteger(intSelf, executive);
    }

    public Var Power(Var self, Var other, Executive executive)
    {
        var intSelf = (IntegerVar)self;

        if (other is not IntegerVar intOther)
        {
            // Convert to real for non-integer exponents
            return new RealVar(intSelf.Data).Power(other, executive);
        }

        long baseValue = intSelf.Data;
        long exponent = intOther.Data;

        // Handle special cases
        if (exponent < 0)
        {
            // Negative exponents require real arithmetic
            return new RealVar(Math.Pow(baseValue, exponent));
        }

        if (baseValue == 0)
        {
            if (exponent == 0)
            {
                executive.LogRuntimeException(18);
                return StringVar.Null();
            }
            return IntegerVar.Zero;
        }

        if (exponent == 0)
        {
            return IntegerVar.One;
        }

        if (exponent == 1)
        {
            return IntegerVar.Create(baseValue);
        }

        if (baseValue == 1)
        {
            return IntegerVar.One;
        }

        if (baseValue == -1)
        {
            return (exponent & 1) == 0 ? IntegerVar.One : IntegerVar.MinusOne;
        }

        // Use exponentiation by squaring for better performance
        return PowerBySquaring(baseValue, exponent, executive);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Var PowerBySquaring(long baseValue, long exponent, Executive executive)
    {
        long result = 1;
        long currentBase = baseValue;
        long currentExponent = exponent;

        while (currentExponent > 0)
        {
            // If exponent is odd, multiply result by current base
            if ((currentExponent & 1) == 1)
            {
                // Check for overflow
                if (!TryMultiplySafe(result, currentBase, out long newResult))
                {
                    // Fall back to real arithmetic on overflow
                    return new RealVar(Math.Pow(baseValue, exponent));
                }
                result = newResult;
            }

            // Square the base for next iteration
            currentExponent >>= 1;
            if (currentExponent > 0)
            {
                if (!TryMultiplySafe(currentBase, currentBase, out long newBase))
                {
                    // Fall back to real arithmetic on overflow
                    return new RealVar(Math.Pow(baseValue, exponent));
                }
                currentBase = newBase;
            }
        }

        return IntegerVar.Create(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryMultiplySafe(long left, long right, out long result)
    {
        // Fast path for small values
        if (left == 0 || right == 0)
        {
            result = 0;
            return true;
        }

        if (left == 1)
        {
            result = right;
            return true;
        }

        if (right == 1)
        {
            result = left;
            return true;
        }

        // Manual overflow check for multiplication
        try
        {
            checked
            {
                result = left * right;
            }
            return true;
        }
        catch (OverflowException)
        {
            result = 0;
            return false;
        }
    }

    public Var Negate(Var self, Executive executive)
    {
        var intSelf = (IntegerVar)self;
        long value = intSelf.Data;

        // Check for overflow (only MinValue causes overflow when negated)
        if (value == long.MinValue)
        {
            executive.LogRuntimeException(11);
            return StringVar.Null();
        }

        return IntegerVar.Create(-value);
    }
}