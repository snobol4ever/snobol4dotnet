using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

public abstract class Var : IEquatable<Var>
{
    public delegate bool ValidationDelegate(Var v);

    #region Properties

    private static int _creationOrder; 
    public static long CreationOrder => Interlocked.Increment(ref _creationOrder); 
    public bool IsKeyword { get; internal set; }
    public bool IsReadOnly { get; internal set; }
    public bool Succeeded { get; internal set; } = true;
    public string InputChannel { get; internal set; } = string.Empty;
    public string OutputChannel { get; internal set; } = string.Empty;
    public ValidationDelegate? Validation { get; internal set; }
    public string Symbol { get; internal set; } = string.Empty;
    public long SequenceId { get; } = ++_creationOrder;
    public object? Key { get; internal set; }
    public Var? Collection { get; internal set; }
    [MemberNotNullWhen(true, nameof(Key), nameof(Collection))]
    public bool IsCollectionElement => Key is not null && Collection is not null;

    #endregion

    #region Strategy Properties

    protected abstract IArithmeticStrategy ArithmeticStrategy { get; }

    protected abstract IComparisonStrategy ComparisonStrategy { get; }

    protected abstract IConversionStrategy ConversionStrategy { get; }

    protected abstract ICloningStrategy CloningStrategy { get; }

    protected abstract IFormattingStrategy FormattingStrategy { get; }

    #endregion

    #region Arithmetic Operations (Strategy Pattern)


    public virtual Var Add(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Add(this, other, executive);
    }


    public virtual Var Subtract(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Subtract(this, other, executive);
    }


    public virtual Var Multiply(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Multiply(this, other, executive);
    }


    public virtual Var Divide(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Divide(this, other, executive);
    }


    public virtual Var Power(Var other, Executive executive)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Power(this, other, executive);
    }


    public virtual Var Negate(Executive executive)
    {
        ArgumentNullException.ThrowIfNull(executive);
        return ArithmeticStrategy.Negate(this, executive);
    }

    #endregion

    #region Double Dispatch Methods for Type-Safe Operations

    // These methods enable double dispatch for type-safe arithmetic
    // Subclasses override these to provide type-specific behavior

    protected internal virtual Var AddInteger(IntegerVar left, Executive executive)
=> ThrowNotSupportedException($"Cannot add {left.DataType()} to {DataType()}");

    protected internal virtual Var AddReal(RealVar left, Executive executive)
=> ThrowNotSupportedException($"Cannot add {left.DataType()} to {DataType()}");

    protected internal virtual Var AddString(StringVar left, Executive executive)
=> ThrowNotSupportedException($"Cannot add {left.DataType()} to {DataType()}");

    protected internal virtual Var SubtractInteger(IntegerVar left, Executive executive)
=> ThrowNotSupportedException($"Cannot subtract {DataType()} from {left.DataType()}");

    protected internal virtual Var SubtractReal(RealVar left, Executive executive)
=> ThrowNotSupportedException($"Cannot subtract {DataType()} from {left.DataType()}");

    protected internal virtual Var MultiplyInteger(IntegerVar left, Executive executive)
=> ThrowNotSupportedException($"Cannot multiply {left.DataType()} by {DataType()}");

    protected internal virtual Var MultiplyReal(RealVar left, Executive executive)
=> ThrowNotSupportedException($"Cannot multiply {left.DataType()} by {DataType()}");

    protected internal virtual Var DivideInteger(IntegerVar left, Executive executive)
=> ThrowNotSupportedException($"Cannot divide {left.DataType()} by {DataType()}");

    protected internal virtual Var DivideReal(RealVar left, Executive executive)
=> ThrowNotSupportedException($"Cannot divide {left.DataType()} by {DataType()}");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Var ThrowNotSupportedException(string message)
        => throw new NotSupportedException(message);

    #endregion

    #region Base Double Dispatch Error Handlers


    protected static Var LogArithmeticTypeError(Executive executive, int errorCode)
    {
        executive.LogRuntimeException(errorCode);
        return StringVar.Null();
    }

    #endregion

    #region Comparison Operations (Strategy Pattern)


    internal virtual int Compare(Var other)
    {
        return ComparisonStrategy.CompareTo(this, other);
    }

    public virtual bool Equals(Var? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ComparisonStrategy.Equals(this, other);
    }

    public override bool Equals(object? obj) => obj is Var other && Equals(other);

    public override int GetHashCode() => SequenceId.GetHashCode();


    internal virtual bool IsIdentical(Var other)
    {
        return ComparisonStrategy.IsIdentical(this, other);
    }

    #endregion

    #region Conversion Operations (Strategy Pattern)


    public virtual bool Convert(Executive.VarType varType, out Var varOut, out object valueOut, Executive exec)
    {
        return ConversionStrategy.TryConvert(this, varType, out varOut, out valueOut, exec);
    }


    internal virtual string DataType() => ConversionStrategy.GetDataType(this);


    internal virtual object GetTableKey() => ConversionStrategy.GetTableKey(this);

    #endregion

    #region Cloning Operations (Strategy Pattern)


    internal virtual Var Clone() => CloningStrategy.Clone(this);

    #endregion

    #region Formatting Operations (Strategy Pattern)

    public override string ToString() => FormattingStrategy.ToString(this);

    internal virtual string DumpString() => FormattingStrategy.DumpString(this);

    #endregion

    #region Static Helper Methods


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