#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Represents a SNOBOL4 table variable - a hash table with dynamic key-value pairs
/// Tables can use any data type as keys and values, with a fill value for missing keys
/// </summary>
[DebuggerDisplay("{DebugString()}")]
public sealed class TableVar : Var
{
    #region Data

    internal Dictionary<object, Var> Data;
    internal readonly Var Fill;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the number of elements in the table
    /// </summary>
    public int Count => Data.Count;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly TableArithmeticStrategy _arithmeticStrategy = new();
    private static readonly TableComparisonStrategy _comparisonStrategy = new();
    private static readonly TableConversionStrategy _conversionStrategy = new();
    private static readonly TableCloningStrategy _cloningStrategy = new();
    private static readonly TableFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new table with the specified fill value
    /// </summary>
    /// <param name="fill">The default value returned for non-existent keys</param>
    /// <exception cref="ArgumentNullException">Thrown when fill is null</exception>
    internal TableVar(Var fill)
    {
        ArgumentNullException.ThrowIfNull(fill);
        Data = [];
        Fill = fill;
    }

    /// <summary>
    /// Creates a new table with the specified fill value and initial capacity
    /// </summary>
    /// <param name="fill">The default value returned for non-existent keys</param>
    /// <param name="capacity">Initial capacity for the dictionary to avoid resizing</param>
    /// <exception cref="ArgumentNullException">Thrown when fill is null</exception>
    internal TableVar(Var fill, int capacity)
    {
        ArgumentNullException.ThrowIfNull(fill);
        Data = new Dictionary<object, Var>(capacity);
        Fill = fill;
    }

    #endregion

    #region Table-Specific Methods

    /// <summary>
    /// Gets value by key, returning a clone of the fill value if key doesn't exist
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <returns>The value associated with the key, or a clone of the fill value</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var GetOrDefault(object key)
    {
        ArgumentNullException.ThrowIfNull(key);

        // Use CollectionsMarshal.GetValueRefOrNullRef for faster lookups in .NET 9
        ref var value = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrNullRef(Data, key);
        if (!System.Runtime.CompilerServices.Unsafe.IsNullRef(ref value))
        {
            return value;
        }

        // Return a clone to prevent shared reference issues
        var fillClone = Fill.Clone();
        fillClone.Key = key;
        fillClone.Collection = this;
        return fillClone;
    }

    /// <summary>
    /// Sets value by key, creating a new entry or updating an existing one
    /// </summary>
    /// <param name="key">The key to set</param>
    /// <param name="value">The value to associate with the key</param>
    /// <exception cref="ArgumentNullException">Thrown when key or value is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Set(object key, Var value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        value.Key = key;
        value.Collection = this;
        Data[key] = value;
    }

    /// <summary>
    /// Checks if table contains a specific key
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if the key exists, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool ContainsKey(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Data.ContainsKey(key);
    }

    /// <summary>
    /// Removes a key-value pair from the table
    /// </summary>
    /// <param name="key">The key to remove</param>
    /// <returns>True if the key was found and removed, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool Remove(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Data.Remove(key);
    }

    /// <summary>
    /// Clears all entries from the table
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Clear()
    {
        Data.Clear();
    }

    /// <summary>
    /// Attempts to get a value from the table
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <param name="value">The value if found</param>
    /// <returns>True if the key exists, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetValue(object key, out Var value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Data.TryGetValue(key, out value!);
    }

    /// <summary>
    /// Gets all keys in the table
    /// </summary>
    /// <returns>Collection of all keys</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IEnumerable<object> GetKeys()
    {
        return Data.Keys;
    }

    /// <summary>
    /// Gets all values in the table
    /// </summary>
    /// <returns>Collection of all values</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IEnumerable<Var> GetValues()
    {
        return Data.Values;
    }

    #endregion

    #region Double Dispatch Methods

    // Tables don't support arithmetic operations with other types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var AddInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 2); // Right operand of + is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var AddReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 2); // Right operand of + is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 33); // Right operand of - is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var SubtractReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 33); // Right operand of - is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 27); // Right operand of * is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var MultiplyReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 27); // Right operand of * is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 13); // Right operand of / is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var DivideReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 13); // Right operand of / is not numeric

    #endregion
}