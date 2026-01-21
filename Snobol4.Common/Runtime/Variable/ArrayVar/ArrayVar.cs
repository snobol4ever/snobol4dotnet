#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace Snobol4.Common;
#pragma warning restore IDE0130

/// <summary>
/// Represents a multidimensional array variable in SNOBOL4.
/// Supports arbitrary lower and upper bounds per dimension.
/// </summary>
[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class ArrayVar : Var
{
    #region Data

    internal List<long> Sizes { get; } = [];
    internal List<Var> Data { get; private set; } = [];
    internal long Dimensions { get; set; }
    internal long TotalSize { get; set; } = 1;
    internal string Prototype { get; set; } = string.Empty;
    internal Var Fill { get; set; } = StringVar.Null();
    internal List<long> LowerBounds { get; } = [];
    internal List<long> Multipliers { get; } = [];
    internal List<long> UpperBounds { get; } = [];

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

    /// <summary>
    /// Configure array dimensions and bounds from prototype string
    /// </summary>
    /// <param name="prototype">Prototype string (e.g., "1:10,1:20" or "10,20")</param>
    /// <param name="fill">Fill value for array elements</param>
    /// <returns>0 on success, error code otherwise</returns>
    internal int ConfigurePrototype(string prototype, Var fill)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prototype);
        ArgumentNullException.ThrowIfNull(fill);

        Fill = fill;
        var prototypeSpan = prototype.AsSpan();

        while (prototypeSpan.Length > 0)
        {
            var match = CompiledRegex.ArrayPrototypePattern().Match(prototypeSpan.ToString());
            if (!match.Success)
                return 65; // Invalid prototype syntax

            prototypeSpan = prototypeSpan[match.Length..];

            if (!TryParseDimensionBounds(match, out var lower, out var upper, out var errorCode))
                return errorCode;

            // Validate bounds
            if (lower > upper)
                return 67; // Lower bound exceeds upper bound

            var dimensionSize = upper - lower + 1;
            if (dimensionSize <= 0)
                return 67; // Dimension size must be positive

            LowerBounds.Insert(0, lower);
            UpperBounds.Insert(0, upper);
            Dimensions++;
            Sizes.Insert(0, dimensionSize);
        }

        // Validate we have at least one dimension
        if (Dimensions == 0)
            return 67; // Array must have at least one dimension

        return InitializeArrayData();
    }

    /// <summary>
    /// Parse dimension bounds from regex match
    /// </summary>

    private static bool TryParseDimensionBounds(System.Text.RegularExpressions.Match match, out long lower, out long upper, out int errorCode)
    {
        lower = 1;
        upper = 0;
        errorCode = 0;

        if (match.Groups[3].Success)
        {
            // Format: "lower:upper"
            if (!ToInteger(match.Groups[1].ValueSpan, out lower))
            {
                errorCode = 65; // Invalid lower bound
                return false;
            }

            if (!ToInteger(match.Groups[3].ValueSpan, out upper))
            {
                errorCode = 66; // Invalid upper bound
                return false;
            }
        }
        else
        {
            // Format: "upper" (assumes lower = 1)
            if (!ToInteger(match.Groups[1].ValueSpan, out upper))
            {
                errorCode = 67; // Invalid dimension size
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Initialize array multipliers, total size, and data
    /// </summary>
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

        // Validate total size to prevent memory issues
        if (TotalSize > int.MaxValue)
            return 67; // Array too large

        var totalSize = (int)TotalSize;

        // Pre-allocate array data with exact capacity
        Data = new List<Var>(totalSize);

        // Fill array efficiently - avoid repeated property access
        for (var i = 0; i < totalSize; i++)
            Data.Add(Fill);

        // Build human-readable prototype string
        BuildPrototypeString();

        return 0;
    }

    /// <summary>
    /// Build human-readable prototype string from bounds
    /// </summary>
    private void BuildPrototypeString()
    {
        var dimensions = (int)Dimensions;

        // Use Span-based string concatenation for better performance
        if (dimensions == 1)
        {
            Prototype = $"{LowerBounds[0]}:{UpperBounds[0]}";
            return;
        }

        var parts = new string[dimensions];
        for (var d = dimensions - 1; d >= 0; --d)
        {
            parts[dimensions - 1 - d] = $"{LowerBounds[d]}:{UpperBounds[d]}";
        }
        Prototype = string.Join(',', parts);
    }

    /// <summary>
    /// Convert multi-dimensional indices to linear index
    /// </summary>
    /// <param name="indices">List of dimension indices</param>
    /// <returns>Linear index into Data array</returns>

    internal long Index(List<long> indices)
    {
        ArgumentNullException.ThrowIfNull(indices);

        var dimensions = (int)Dimensions;
        if (indices.Count != dimensions)
            throw new ArgumentException($"Expected {dimensions} indices but got {indices.Count}", nameof(indices));

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

    /// <summary>
    /// Get the element at the specified indices
    /// </summary>

    internal Var GetElement(List<long> indices)
    {
        var index = Index(indices);
        if (index < 0 || index >= Data.Count)
            throw new ArgumentOutOfRangeException(nameof(indices), "Computed index is out of bounds");

        return Data[(int)index];
    }

    /// <summary>
    /// Set the element at the specified indices
    /// </summary>

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