using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Represents the base abstract class for all variable types in the SNOBOL4 runtime.
/// Uses Strategy Pattern for extensible operations.
/// </summary>
public abstract class Var : IEquatable<Var>
{
    public delegate bool ValidationDelegate(Var v);

    #region Properties

    /// <summary>
    /// Gets or sets whether this variable is a keyword variable.
    /// </summary>
    public bool IsKeyword { get; internal set; }

    /// <summary>
    /// Gets or sets whether this variable is read-only.
    /// </summary>
    public bool IsReadOnly { get; internal set; }

    /// <summary>
    /// Gets or sets whether the last operation on this variable succeeded.
    /// </summary>
    public bool Succeeded { get; internal set; } = true;

    /// <summary>
    /// Gets the unique identifier for this variable instance.
    /// </summary>
    ///public long CreationOrder  = Builder.CreationOrder++;

    /// <summary>
    /// Gets or sets the input channel associated with this variable.
    /// </summary>
    public string InputChannel { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets or sets the output channel associated with this variable.
    /// </summary>
    public string OutputChannel { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation delegate for this variable.
    /// </summary>
    public ValidationDelegate? Validation { get; internal set; }

    /// <summary>
    /// Gets or sets the symbol name associated with this variable.
    /// </summary>
    public string Symbol { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the date and time when this variable was created (in UTC).
    /// </summary>
    public long CreationOrder { get; } = ++Builder.CreationOrder;

    /// <summary>
    /// Gets or sets the key if this variable is an element of a collection.
    /// </summary>
    public object? Key { get; internal set; }

    /// <summary>
    /// Gets or sets the parent collection if this variable is an element of a collection.
    /// </summary>
    public Var? Collection { get; internal set; }

    /// <summary>
    /// Gets whether this variable is part of a collection.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Key), nameof(Collection))]
    public bool IsCollectionElement => Key is not null && Collection is not null;

    #endregion

    #region Strategy Properties

    /// <summary>
    /// Strategy for arithmetic operations
    /// </summary>
    protected abstract IArithmeticStrategy ArithmeticStrategy { get; }

    /// <summary>
    /// Strategy for comparison operations
    /// </summary>
    protected abstract IComparisonStrategy ComparisonStrategy { get; }

    /// <summary>
    /// Strategy for type conversion operations
    /// </summary>
    protected abstract IConversionStrategy ConversionStrategy { get; }

    /// <summary>
    /// Strategy for cloning operations
    /// </summary>
    protected abstract ICloningStrategy CloningStrategy { get; }

    /// <summary>
    /// Strategy for formatting operations
    /// </summary>
    protected abstract IFormattingStrategy FormattingStrategy { get; }

    #endregion

    #region Arithmetic Operations (Strategy Pattern)

    /// <summary>
    /// Add another variable to this one using double dispatch
    /// </summary>
    /// <param name="other">The variable to add</param>
    /// <param name="executive">The execution context</param>
    /// <returns>The result of the addition</returns>
    /// <exception cref="ArgumentNullException">Thrown when other or executive is null</exception>

    public virtual Var Add(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Add(this, other, executive);
    }

    /// <summary>
    /// Subtract another variable from this one
    /// </summary>
    /// <param name="other">The variable to subtract</param>
    /// <param name="executive">The execution context</param>
    /// <returns>The result of the subtraction</returns>
    /// <exception cref="ArgumentNullException">Thrown when other or executive is null</exception>

    public virtual Var Subtract(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Subtract(this, other, executive);
    }

    /// <summary>
    /// Multiply this variable by another
    /// </summary>
    /// <param name="other">The variable to multiply by</param>
    /// <param name="executive">The execution context</param>
    /// <returns>The result of the multiplication</returns>
    /// <exception cref="ArgumentNullException">Thrown when other or executive is null</exception>

    public virtual Var Multiply(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Multiply(this, other, executive);
    }

    /// <summary>
    /// Divide this variable by another
    /// </summary>
    /// <param name="other">The variable to divide by</param>
    /// <param name="executive">The execution context</param>
    /// <returns>The result of the division</returns>
    /// <exception cref="ArgumentNullException">Thrown when other or executive is null</exception>

    public virtual Var Divide(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Divide(this, other, executive);
    }

    /// <summary>
    /// Raise this variable to the power of another
    /// </summary>
    /// <param name="other">The exponent</param>
    /// <param name="executive">The execution context</param>
    /// <returns>The result of the exponentiation</returns>
    /// <exception cref="ArgumentNullException">Thrown when other or executive is null</exception>

    public virtual Var Power(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Power(this, other, executive);
    }

    /// <summary>
    /// Negate this variable (unary minus)
    /// </summary>
    /// <param name="executive">The execution context</param>
    /// <returns>The negated value</returns>
    /// <exception cref="ArgumentNullException">Thrown when executive is null</exception>

    public virtual Var Negate(Executive executive)
    {
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Negate(this, executive);
    }

    #endregion

    #region Double Dispatch Methods for Type-Safe Operations

    // These methods enable double dispatch for type-safe arithmetic
    // Subclasses override these to provide type-specific behavior

    /// <summary>
    /// Handles addition when the left operand is an integer
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported by default</exception>
    [DoesNotReturn]
    protected internal virtual Var AddInteger(IntegerVar left, Executive executive)
        => ThrowNotSupportedException($"Cannot add {left.DataType()} to {DataType()}");

    /// <summary>
    /// Handles addition when the left operand is a real number
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported by default</exception>
    [DoesNotReturn]
    protected internal virtual Var AddReal(RealVar left, Executive executive)
        => ThrowNotSupportedException($"Cannot add {left.DataType()} to {DataType()}");

    /// <summary>
    /// Handles addition when the left operand is a string
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported by default</exception>
    [DoesNotReturn]
    protected internal virtual Var AddString(StringVar left, Executive executive)
        => ThrowNotSupportedException($"Cannot add {left.DataType()} to {DataType()}");

    /// <summary>
    /// Handles subtraction when the left operand is an integer
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported by default</exception>
    [DoesNotReturn]
    protected internal virtual Var SubtractInteger(IntegerVar left, Executive executive)
        => ThrowNotSupportedException($"Cannot subtract {DataType()} from {left.DataType()}");

    /// <summary>
    /// Handles subtraction when the left operand is a real number
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported by default</exception>
    [DoesNotReturn]
    protected internal virtual Var SubtractReal(RealVar left, Executive executive)
        => ThrowNotSupportedException($"Cannot subtract {DataType()} from {left.DataType()}");

    /// <summary>
    /// Handles multiplication when the left operand is an integer
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported by default</exception>
    [DoesNotReturn]
    protected internal virtual Var MultiplyInteger(IntegerVar left, Executive executive)
        => ThrowNotSupportedException($"Cannot multiply {left.DataType()} by {DataType()}");

    /// <summary>
    /// Handles multiplication when the left operand is a real number
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported by default</exception>
    [DoesNotReturn]
    protected internal virtual Var MultiplyReal(RealVar left, Executive executive)
        => ThrowNotSupportedException($"Cannot multiply {left.DataType()} by {DataType()}");

    /// <summary>
    /// Handles division when the left operand is an integer
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported by default</exception>
    [DoesNotReturn]
    protected internal virtual Var DivideInteger(IntegerVar left, Executive executive)
        => ThrowNotSupportedException($"Cannot divide {left.DataType()} by {DataType()}");

    /// <summary>
    /// Handles division when the left operand is a real number
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported by default</exception>
    [DoesNotReturn]
    protected internal virtual Var DivideReal(RealVar left, Executive executive)
        => ThrowNotSupportedException($"Cannot divide {left.DataType()} by {DataType()}");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Var ThrowNotSupportedException(string message)
        => throw new NotSupportedException(message);

    #endregion

    #region Base Double Dispatch Error Handlers

    /// <summary>
    /// Standard error handler for arithmetic operations on non-numeric types.
    /// Logs the error and returns a null StringVar.
    /// </summary>
    /// <param name="executive">The execution context</param>
    /// <param name="errorCode">The runtime error code to log</param>
    /// <returns>A null StringVar indicating failure</returns>

    protected static Var LogArithmeticTypeError(Executive executive, int errorCode)
    {
        executive.LogRuntimeException(errorCode);
        return StringVar.Null();
    }

    #endregion

    #region Comparison Operations (Strategy Pattern)

    /// <summary>
    /// Compares this variable to another
    /// </summary>
    /// <param name="other">The variable to compare to</param>
    /// <returns>A value indicating the relative order</returns>
    /// <exception cref="ArgumentNullException">Thrown when other is null</exception>

    internal virtual int Compare(Var other)
    {
        return ComparisonStrategy.CompareTo(this, other);
    }

    /// <summary>
    /// Determines whether this variable is equal to another
    /// </summary>
    /// <param name="other">The variable to compare to</param>
    /// <returns>True if equal, false otherwise</returns>
    public virtual bool Equals(Var? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ComparisonStrategy.Equals(this, other);
    }

    /// <summary>
    /// Determines whether the specified object is equal to this variable
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if equal, false otherwise</returns>
    public override bool Equals(object? obj) => obj is Var other && Equals(other);

    /// <summary>
    /// Determines whether two variables are identical (reference equality)
    /// </summary>
    /// <param name="other">The variable to compare to</param>
    /// <returns>True if identical, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when other is null</exception>

    internal virtual bool IsIdentical(Var other)
    {
        return ComparisonStrategy.IsIdentical(this, other);
    }

    #endregion

    #region Conversion Operations (Strategy Pattern)

    /// <summary>
    /// Converts this variable to the specified type
    /// </summary>
    /// <param name="varType">The target type</param>
    /// <param name="varOut">The converted variable</param>
    /// <param name="valueOut">The converted value</param>
    /// <param name="exec">The execution context</param>
    /// <returns>True if conversion succeeded, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when exec is null</exception>

    public virtual bool Convert(Executive.VarType varType, out Var varOut, out object valueOut, Executive exec)
    {
        return ConversionStrategy.TryConvert(this, varType, out varOut, out valueOut, exec);
    }

    /// <summary>
    /// Gets the data type name of this variable
    /// </summary>
    /// <returns>The data type name</returns>

    internal virtual string DataType() => ConversionStrategy.GetDataType(this);

    /// <summary>
    /// Gets the key used for table lookups
    /// </summary>
    /// <returns>The table key</returns>

    internal virtual object GetTableKey() => ConversionStrategy.GetTableKey(this);

    #endregion

    #region Cloning Operations (Strategy Pattern)

    /// <summary>
    /// Creates a clone of this variable
    /// </summary>
    /// <returns>A new variable instance with copied values</returns>

    internal virtual Var Clone() => CloningStrategy.Clone(this);

    #endregion

    #region Formatting Operations (Strategy Pattern)

    /// <summary>
    /// Returns a string representation of this variable
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString() => FormattingStrategy.ToString(this);

    /// <summary>
    /// Returns a detailed string representation for debugging dumps
    /// </summary>
    /// <returns>Detailed string representation</returns>
    internal virtual string DumpString() => FormattingStrategy.DumpString(this);

    #endregion

    #region Static Helper Methods

    /// <summary>
    /// Attempts to convert a string to an integer
    /// </summary>
    /// <param name="inString">The input string</param>
    /// <param name="integerOut">The parsed integer value</param>
    /// <returns>True if conversion succeeded, false otherwise</returns>

    internal static bool ToInteger(ReadOnlySpan<char> inString, out long integerOut)
    {
        // Convert empty string to 0 (fast path)
        if (inString.IsEmpty)
        {
            integerOut = 0;
            return true;
        }

        return long.TryParse(inString, NumberStyles.Integer, CultureInfo.InvariantCulture, out integerOut);
    }

    /// <summary>
    /// Attempts to convert a string to a real number
    /// </summary>
    /// <param name="inString">The input string</param>
    /// <param name="realOut">The parsed real value</param>
    /// <returns>True if conversion succeeded, false otherwise</returns>

    internal static bool ToReal(ReadOnlySpan<char> inString, out double realOut)
    {
        // Convert empty string to 0.0 (fast path)
        if (inString.IsEmpty)
        {
            realOut = 0.0;
            return true;
        }

        if (!double.TryParse(inString, NumberStyles.Float | NumberStyles.AllowThousands, 
            CultureInfo.InvariantCulture, out realOut))
        {
            return false;
        }

        // Simplified: IsFinite checks both NaN and Infinity in one call
        return double.IsFinite(realOut);
    }

    /// <summary>
    /// Attempts to convert a variable to a numeric value (integer or real)
    /// </summary>
    internal static bool ToNumeric(Var varIn, out bool isInteger, out long l, out double d, Executive exec)
    {
        // Resolve NameVar references
        if (varIn is NameVar nameVar)
        {
            varIn = exec.IdentifierTable[nameVar.Pointer];
        }

        // Use pattern matching with type checks - optimized for common cases
        return varIn switch
        {
            IntegerVar intVar => HandleIntegerVar(intVar, out isInteger, out l, out d),
            RealVar realVar => HandleRealVar(realVar, out isInteger, out l, out d),
            StringVar strVar => TryParseStringAsNumeric(strVar, out isInteger, out l, out d),
            _ => InitializeFailureValues(out isInteger, out l, out d)
        };
    }


    private static bool HandleIntegerVar(IntegerVar integerVar, out bool isInteger, out long l, out double d)
    {
        isInteger = true;
        l = integerVar.Data;
        d = 0.0; // Initialize with zero instead of default
        return true;
    }


    private static bool HandleRealVar(RealVar realVar, out bool isInteger, out long l, out double d)
    {
        isInteger = false;
        l = 0L; // Initialize with zero instead of default
        d = realVar.Data;
        return true;
    }

    private static bool TryParseStringAsNumeric(StringVar stringVar, out bool isInteger, out long l, out double d)
    {
        // Try integer first (more common and faster)
        if (ToInteger(stringVar.Data, out l))
        {
            isInteger = true;
            d = 0.0;
            return true;
        }

        // Try real
        if (ToReal(stringVar.Data, out d))
        {
            isInteger = false;
            l = 0L;
            return true;
        }

        // Failed both conversions
        isInteger = false;
        l = 0L;
        d = double.NaN;
        return false;
    }


    private static bool InitializeFailureValues(out bool isInteger, out long l, out double d)
    {
        isInteger = false;
        l = 0L;
        d = double.NaN;
        return false;
    }

    #endregion
}