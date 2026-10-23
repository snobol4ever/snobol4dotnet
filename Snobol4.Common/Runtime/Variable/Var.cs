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
    // The statement (Executive.StatementSerial) whose index read last set Key and Collection
    internal long LvalueMark;
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

    // The same value with no variable's name on it, as a call hands back a value it was given (CONVERT to the type it has, COPY of a
    // string): a name operator takes it for a value, not for the variable it came from (.CONVERT(X, 'STRING') and .COPY(X) are error 21),
    // and it keeps its identity and its contents (IDENT(A, CONVERT(A, 'ARRAY')), and an element set through either is set in both)
    internal Var Unnamed()
    {
        if (Symbol.Length == 0 && Key is null && Collection is null)
            return this;
        var alias = (Var)MemberwiseClone();
        alias.Symbol = string.Empty;
        alias.Key = null;
        alias.Collection = null;
        alias.LvalueMark = 0;
        return alias;
    }

    #endregion

    #region Formatting Operations (Strategy Pattern)

    public override string ToString() => FormattingStrategy.ToString(this);

    internal virtual string DumpString() => FormattingStrategy.DumpString(this);

    #endregion

    #region Static Helper Methods


    internal static bool ToInteger(ReadOnlySpan<char> inString, out long integerOut)
    {
        // The null string, or one of blanks and tabs only, is 0, as in SPITBOL
        if (inString.Trim(" \t").IsEmpty)
        {
            integerOut = 0;
            return true;
        }

        return long.TryParse(inString, NumberStyles.Integer, CultureInfo.InvariantCulture, out integerOut);
    }


    /// <summary>
    /// A string read as a real, as SPITBOL reads it (sbl.min gtnum, the real circuit): blanks or tabs
    /// around it, an optional sign, digits with at most one point, an optional exponent (e, E, d or D). The
    /// digits before any point accumulate as an integer; from there each digit is acc * 10.0 + digit in
    /// doubles; the scale (digits after the point less the exponent) is applied in steps of 10**10 and
    /// then once from the powers-of-ten table. The double arithmetic is SPITBOL's own, step for step, so
    /// the value read is SPITBOL's, not a correctly rounded one; results below the smallest normal double
    /// flush to zero, as SPITBOL runs with flush-to-zero set. The null string or blanks alone read 0.0.
    /// </summary>
    internal static bool ToReal(ReadOnlySpan<char> inString, out double realOut)
    {
        realOut = 0.0;
        var i = 0;
        var n = inString.Length;
        while (i < n && inString[i] is ' ' or '\t') i++;
        if (i == n)
            return true;

        var negative = false;
        if (inString[i] is '+' or '-')
        {
            negative = inString[i] == '-';
            if (++i == n)
                return false;
        }

        // the integer circuit: a negative accumulator, as SPITBOL's cvm keeps it
        long integer = 0;
        var digits = false;
        double acc;
        while (true)
        {
            if (i == n || inString[i] is ' ' or '\t')
            {
                for (; i < n; i++)
                    if (inString[i] is not (' ' or '\t'))
                        return false;
                if (!digits)
                    return false;
                // 9223372036854775808 is no integer, and a string of digits with no point or exponent is no real: it is not a number
                if (!negative && integer == long.MinValue)
                    return false;
                realOut = negative ? integer : -(double)integer;
                return true;
            }

            var c = inString[i];
            if (c is < '0' or > '9')
            {
                acc = -(double)integer;
                break;
            }

            if (integer < (long.MinValue + (c - '0')) / 10)
            {
                acc = -(double)integer;   // overflow: the digits so far, as a real, and this digit in the real circuit
                break;
            }

            integer = integer * 10 - (c - '0');
            digits = true;
            i++;
        }

        // the real circuit
        var scale = 0;
        var point = false;
        long exponent = 0;
        while (i < n)
        {
            var c = inString[i];
            if (c is >= '0' and <= '9')
            {
                acc = FlushToZero(FlushToZero(acc * 10.0) + (c - '0'));
                if (double.IsInfinity(acc))
                    return false;
                if (point)
                    scale++;
                digits = true;
                i++;
                continue;
            }

            if (c == '.')
            {
                if (point)
                    return false;
                point = true;
                i++;
                continue;
            }

            if (c is 'e' or 'E' or 'd' or 'D')
            {
                point = true;
                if (++i == n)
                    return false;
                var exponentNegative = false;
                if (inString[i] is '+' or '-')
                {
                    exponentNegative = inString[i] == '-';
                    if (++i == n)
                        return false;
                }

                for (; i < n && inString[i] is >= '0' and <= '9'; i++)
                {
                    exponent = exponent * 10 + (inString[i] - '0');
                    if (exponent > int.MaxValue)
                        return false;
                }

                if (exponentNegative)
                    exponent = -exponent;
            }

            for (; i < n; i++)
                if (inString[i] is not (' ' or '\t'))
                    return false;
        }

        if (!digits || !point)
            return false;

        var net = scale - exponent;
        if (net >= 0)
        {
            for (; net > 10; net -= 10)
                acc = FlushToZero(acc / 1e10);
            if (net > 0)
                acc = FlushToZero(acc / _powersOfTen[net]);
        }
        else
        {
            for (net = -net; net > 10; net -= 10)
                acc *= 1e10;
            if (net > 0)
                acc *= _powersOfTen[net];
            if (double.IsInfinity(acc))
                return false;
        }

        realOut = negative ? -acc : acc;
        return true;
    }

    private static readonly double[] _powersOfTen = [1.0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10];

    private static double FlushToZero(double x) => Math.Abs(x) < 2.2250738585072014E-308 ? 0.0 : x;

    internal static bool ToNumeric(Var varIn, out bool isInteger, out long l, out double d, Executive exec)
    {
        // A name is a number by its text, as SPITBOL converts it: .X + 1 is error 1, CONVERT('12', 'NAME') * 2 is 24, and the name of an
        // element, which has no text, is no number (a call that returns a name by NRETURN gives the variable's value, read before this)
        if (varIn is NameVar nameVar)
        {
            if (nameVar.Pointer.Length == 0)
                return InitializeFailureValues(out isInteger, out l, out d);
            varIn = new StringVar(nameVar.Pointer);
        }

        // An expression (*(...), or a variable that holds one) is not a number, whatever it would evaluate to: only EVAL, a pattern
        // match and a pattern primitive's argument evaluate one. E = *5 then E + 1, -E, LT(E, 6) and REMDR(E, 3) are the errors of
        // a non-numeric operand, as in SPITBOL.

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