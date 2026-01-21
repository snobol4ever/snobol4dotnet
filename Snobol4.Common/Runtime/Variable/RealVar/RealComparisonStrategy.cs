using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Comparison strategy for real (floating-point) variables
/// </summary>
public sealed class RealComparisonStrategy : IComparisonStrategy
{
    private const double _epsilon = double.Epsilon;


    public int CompareTo(Var self, Var other)
    {
        var realSelf = (RealVar)self;

        return other switch
        {
            IntegerVar intOther => realSelf.Data.CompareTo((double)intOther.Data),
            RealVar realOther => realSelf.Data.CompareTo(realOther.Data),
            _ => string.CompareOrdinal(realSelf.DataType(), other.DataType())
        };
    }


    public bool Equals(Var self, Var other)
    {
        var realSelf = (RealVar)self;

        return other switch
        {
            IntegerVar intOther => double.Abs(realSelf.Data - intOther.Data) < _epsilon,
            RealVar realOther => double.Abs(realSelf.Data - realOther.Data) < _epsilon,
            _ => false
        };
    }


    public bool IsIdentical(Var self, Var other)
    {
        if (other is not RealVar realOther)
            return false;

        var realSelf = (RealVar)self;

        // For real numbers, use exact comparison (as in original code)
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return realSelf.Data == realOther.Data;
    }
}