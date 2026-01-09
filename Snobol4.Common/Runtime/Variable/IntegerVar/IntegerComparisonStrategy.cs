    using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Comparison strategy for integer variables
/// Supports comparison with integers, reals, and type-based fallback
/// </summary>
public sealed class IntegerComparisonStrategy : IComparisonStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Var self, Var other)
    {
        var intSelf = (IntegerVar)self;

        return other switch
        {
            IntegerVar intOther => intSelf.Data.CompareTo(intOther.Data),
            RealVar realOther => ((double)intSelf.Data).CompareTo(realOther.Data),
            _ => string.Compare(intSelf.DataType(), other.DataType(), StringComparison.InvariantCulture)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Var self, Var other)
    {
        if (other is not IntegerVar intOther)
        {
            return false;
        }

        var intSelf = (IntegerVar)self;
        return intSelf.Data == intOther.Data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsIdentical(Var self, Var other)
    {
        return Equals(self, other);
    }
}