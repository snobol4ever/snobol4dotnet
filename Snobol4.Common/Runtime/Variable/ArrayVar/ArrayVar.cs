#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;
using System.Globalization;

#pragma warning disable IDE0130
namespace Snobol4.Common;
#pragma warning restore IDE0130

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class ArrayVar : Var
{
    #region Data

    // SPITBOL's idval: the creation number DUMP prints (ARRAY(3) #2); 0 until the array is given one
    internal long BlockId;
    // the DATA object whose fields this array holds, so DUMP can name a field (VAL(N))
    internal ProgramDefinedDataVar? DataOwner;
    internal List<long> Sizes { get; } = [];
    internal List<Var> Data { get; private set; } = [];
    internal long Dimensions { get; set; }
    internal long TotalSize { get; set; } = 1;
    internal string Prototype { get; set; } = string.Empty;
    internal Var Fill { get; set; } = StringVar.Null();
    internal List<long> LowerBounds { get; } = [];
    internal List<long> Multipliers { get; } = [];
    internal List<long> UpperBounds { get; } = [];
    internal List<bool> ExplicitLower { get; } = [];

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly ArrayArithmeticStrategy _arithmeticStrategy = new();
    private static readonly ArrayComparisonStrategy _comparisonStrategy = new();
    private static readonly ArrayConversionStrategy _conversionStrategy = new();
    private static readonly ArrayCloningStrategy _cloningStrategy = new();
    private static readonly ArrayFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Array-Specific Methods

    // SPITBOL's array prototype, scanned as sbl does: a prototype that is one integer in any form ('3', ' 3', '1e1') is a vector;
    // otherwise it is bounds separated by commas, each "high" or "low:high" with the bound taken as an integer (a null bound is 0,
    // so ':3' is 0:3 and '3:' is the negative dimension 3:0). The error is 65 for a low bound, 66 for a high bound that is not an
    // integer, 67 for a dimension that is zero, negative or out of range, and 68 when the elements do not fit in `maxObjectBytes`.
    // A zero-length array does not exist: 1:0 is 67.
    internal int ConfigurePrototype(string prototype, Var fill, long maxObjectBytes = long.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(prototype);
        ArgumentNullException.ThrowIfNull(fill);

        Fill = fill;

        if (BoundToInteger(prototype, out var length))
        {
            if (length <= 0)
                return 67;
            if (!Fits(length, 3, maxObjectBytes))
                return 68;

            LowerBounds.Add(1);
            UpperBounds.Add(length);
            ExplicitLower.Add(false);
            Dimensions = 1;
            Sizes.Add(length);
            Prototype = length.ToString(CultureInfo.InvariantCulture);
            return InitializeArrayData();
        }

        long elements = 1;
        var position = 0;
        while (true)
        {
            long lower = 1;
            var hasExplicitLower = false;
            var delimiter = prototype.AsSpan(position).IndexOfAny(':', ',');
            if (delimiter >= 0)
                delimiter += position;
            if (delimiter >= 0 && prototype[delimiter] == ':')
            {
                if (!BoundToInteger(prototype.AsSpan(position, delimiter - position), out lower))
                    return 65;
                hasExplicitLower = true;
                position = delimiter + 1;
                var comma = prototype.AsSpan(position).IndexOf(',');
                delimiter = comma < 0 ? -1 : comma + position;
            }

            var end = delimiter < 0 ? prototype.Length : delimiter;
            if (!BoundToInteger(prototype.AsSpan(position, end - position), out var upper))
                return 66;

            long dimensionSize;
            try
            {
                dimensionSize = checked(upper - lower);
            }
            catch (OverflowException)
            {
                return 67;
            }

            if (dimensionSize < 0 || dimensionSize == long.MaxValue)
                return 67;
            dimensionSize++;

            try
            {
                elements = checked(elements * dimensionSize);
            }
            catch (OverflowException)
            {
                return 68;
            }

            LowerBounds.Insert(0, lower);
            UpperBounds.Insert(0, upper);
            ExplicitLower.Insert(0, hasExplicitLower);
            Dimensions++;
            Sizes.Insert(0, dimensionSize);

            if (delimiter < 0)
                break;
            position = delimiter + 1;
        }

        if (!Fits(elements, 6 + 2 * Dimensions, maxObjectBytes))
            return 68;

        Prototype = prototype;
        return InitializeArrayData();
    }

    // A bound as SPITBOL's gtint reads it: an integer, or a string that reads as a real, truncated; the null string is 0
    private static bool BoundToInteger(ReadOnlySpan<char> text, out long bound)
    {
        if (ToInteger(text, out bound))
            return true;
        if (!ToReal(text, out var real) || double.IsNaN(real) || real >= 9223372036854775808.0 || real < -9223372036854775808.0)
            return false;
        bound = (long)real;
        return true;
    }

    // The block is its elements and `overheadWords` of its own (a vector 3; an array 6 and two for each dimension), each word 8
    // bytes, and may not be larger than `maxObjectBytes`
    private static bool Fits(long elements, long overheadWords, long maxObjectBytes)
    {
        var words = (UInt128)(ulong)elements + (ulong)overheadWords;
        return words * 8 <= (ulong)maxObjectBytes;
    }

                private int InitializeArrayData()
    {
        var dimensions = (int)Dimensions;

        // Pre-allocate lists with known capacity
        Multipliers.Capacity = dimensions;

        // Calculate multipliers for index conversion
        Multipliers.Add(1);
        for (var j = 0; j < dimensions - 1; ++j)
        {
            var nextMultiplier = Multipliers[j] * Sizes[j];
            Multipliers.Add(nextMultiplier);
        }

        TotalSize = Multipliers[dimensions - 1] * Sizes[dimensions - 1];

        var totalSize = (int)TotalSize;

        // Pre-allocate array data with exact capacity
        Data = new List<Var>(totalSize);

        // Fill array efficiently - avoid repeated property access
        for (var i = 0; i < totalSize; i++)
            Data.Add(Fill);

        return 0;
    }


    internal long Index(List<long> indices)
    {
        ArgumentNullException.ThrowIfNull(indices);

        var dimensions = (int)Dimensions;
        if (indices.Count != dimensions)
            throw new ArgumentException(@$"Expected {dimensions} indices but got {indices.Count}", nameof(indices));

        long key = 0;

        // Unroll small dimension counts for performance
        switch (dimensions)
        {
            case 1:
                return (indices[0] - LowerBounds[0]) * Multipliers[0];
            case 2:
                return (indices[0] - LowerBounds[0]) * Multipliers[0] +
                       (indices[1] - LowerBounds[1]) * Multipliers[1];
            case 3:
                return (indices[0] - LowerBounds[0]) * Multipliers[0] +
                       (indices[1] - LowerBounds[1]) * Multipliers[1] +
                       (indices[2] - LowerBounds[2]) * Multipliers[2];
            default:
                // General case for N dimensions
                for (var i = 0; i < dimensions; ++i)
                {
                    key += (indices[i] - LowerBounds[i]) * Multipliers[i];
                }
                return key;
        }
    }

            
    internal Var GetElement(List<long> indices)
    {
        var index = Index(indices);
        if (index < 0 || index >= Data.Count)
            throw new ArgumentOutOfRangeException(nameof(indices), "Computed index is out of bounds");

        return Data[(int)index];
    }

            
    internal void SetElement(List<long> indices, Var value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var index = Index(indices);
        if (index < 0 || index >= Data.Count)
            throw new ArgumentOutOfRangeException(nameof(indices), "Computed index is out of bounds");

        Data[(int)index] = value;
    }

    #endregion

    #region Double Dispatch Methods

    // Arrays don't support arithmetic operations with other types


    protected internal override Var AddInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 2); // RightPattern operand of + is not numeric


    protected internal override Var AddReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 2); // RightPattern operand of + is not numeric


    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 33); // RightPattern operand of - is not numeric


    protected internal override Var SubtractReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 33); // RightPattern operand of - is not numeric


    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 27); // RightPattern operand of * is not numeric


    protected internal override Var MultiplyReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 27); // RightPattern operand of * is not numeric


    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 13); // RightPattern operand of / is not numeric


    protected internal override Var DivideReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 13); // RightPattern operand of / is not numeric

    #endregion
}