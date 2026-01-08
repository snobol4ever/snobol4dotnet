using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents the base abstract class for all variable types in the SNOBOL4 runtime.
/// Uses Strategy Pattern for extensible operations.
/// </summary>
[DebuggerDisplay("{DebugString()}")]
public abstract class Var
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
    public Guid UniqueId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the input channel associated with this variable.
    /// </summary>
    public string InputChannel { get; internal set; } = "";

    /// <summary>
    /// Gets or sets the output channel associated with this variable.
    /// </summary>
    public string OutputChannel { get; internal set; } = "";

    /// <summary>
    /// Gets or sets the validation delegate for this variable.
    /// </summary>
    public ValidationDelegate? Validation { get; internal set; }

    /// <summary>
    /// Gets or sets the symbol name associated with this variable.
    /// </summary>
    public string Symbol { get; internal set; } = "";

    /// <summary>
    /// Gets the date and time when this variable was created.
    /// </summary>
    public DateTime CreationDateTime { get; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the key if this variable is an element of a collection.
    /// </summary>
    public object? Key { get; internal set; }

    /// <summary>
    /// Gets or sets the parent collection if this variable is an element of a collection.
    /// </summary>
    public Var? Collection { get; internal set; }

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
    public virtual Var Add(Var other, Executive executive)
    {
        return ArithmeticStrategy.Add(this, other, executive);
    }

    /// <summary>
    /// Subtract another variable from this one
    /// </summary>
    public virtual Var Subtract(Var other, Executive executive)
    {
        return ArithmeticStrategy.Subtract(this, other, executive);
    }

    /// <summary>
    /// Multiply this variable by another
    /// </summary>
    public virtual Var Multiply(Var other, Executive executive)
    {
        return ArithmeticStrategy.Multiply(this, other, executive);
    }

    /// <summary>
    /// Divide this variable by another
    /// </summary>
    public virtual Var Divide(Var other, Executive executive)
    {
        return ArithmeticStrategy.Divide(this, other, executive);
    }

    /// <summary>
    /// Raise this variable to the power of another
    /// </summary>
    public virtual Var Power(Var other, Executive executive)
    {
        return ArithmeticStrategy.Power(this, other, executive);
    }

    /// <summary>
    /// Negate this variable (unary minus)
    /// </summary>
    public virtual Var Negate(Executive executive)
    {
        return ArithmeticStrategy.Negate(this, executive);
    }

    #endregion

    #region Double Dispatch Methods for Type-Safe Operations

    // These methods enable double dispatch for type-safe arithmetic
    // Subclasses override these to provide type-specific behavior

    protected internal virtual Var AddInteger(IntegerVar left, Executive executive)
        => throw new NotSupportedException($"Cannot add {left.DataType()} to {DataType()}");

    protected internal virtual Var AddReal(RealVar left, Executive executive)
        => throw new NotSupportedException($"Cannot add {left.DataType()} to {DataType()}");

    protected internal virtual Var AddString(StringVar left, Executive executive)
        => throw new NotSupportedException($"Cannot add {left.DataType()} to {DataType()}");

    protected internal virtual Var SubtractInteger(IntegerVar left, Executive executive)
        => throw new NotSupportedException($"Cannot subtract {DataType()} from {left.DataType()}");

    protected internal virtual Var SubtractReal(RealVar left, Executive executive)
        => throw new NotSupportedException($"Cannot subtract {DataType()} from {left.DataType()}");

    protected internal virtual Var MultiplyInteger(IntegerVar left, Executive executive)
        => throw new NotSupportedException($"Cannot multiply {left.DataType()} by {DataType()}");

    protected internal virtual Var MultiplyReal(RealVar left, Executive executive)
        => throw new NotSupportedException($"Cannot multiply {left.DataType()} by {DataType()}");

    protected internal virtual Var DivideInteger(IntegerVar left, Executive executive)
        => throw new NotSupportedException($"Cannot divide {left.DataType()} by {DataType()}");

    protected internal virtual Var DivideReal(RealVar left, Executive executive)
        => throw new NotSupportedException($"Cannot divide {left.DataType()} by {DataType()}");

    #endregion

    #region Comparison Operations (Strategy Pattern)

    internal virtual int Compare(Var other)
    {
        return ComparisonStrategy.CompareTo(this, other);
    }

    public virtual bool Equals(Var other)
    {
        return ComparisonStrategy.Equals(this, other);
    }

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

    internal virtual string DataType()
    {
        return ConversionStrategy.GetDataType(this);
    }

    internal virtual object GetTableKey()
    {
        return ConversionStrategy.GetTableKey(this);
    }

    #endregion

    #region Cloning Operations (Strategy Pattern)

    internal virtual Var Clone()
    {
        return CloningStrategy.Clone(this);
    }

    #endregion

    #region Formatting Operations (Strategy Pattern)

    public override string ToString()
    {
        return FormattingStrategy.ToString(this);
    }

    internal virtual string DumpString()
    {
        return FormattingStrategy.DumpString(this);
    }

    internal virtual string DebugString()
    {
        return FormattingStrategy.DebugString(this);
    }

    #endregion

    #region Static Helper Methods

    internal static bool ToInteger(string inString, out long integerOut)
    {
        integerOut = 0;

        // Convert empty string to 0
        if (inString == "")
            return true;

        if (!long.TryParse(inString, out var lTry))
            return false;

        integerOut = lTry;
        return true;
    }

    internal static bool ToReal(string inString, out double realOut)
    {
        realOut = 0.0;

        // Convert empty string to 0.0
        if (inString == "")
            return true;

        if (!double.TryParse(inString, out var dTry))
            return false;

        realOut = dTry;
        return !double.IsNaN(dTry) && !double.IsInfinity(dTry);
    }

    internal static bool ToNumeric(Var varIn, out bool isInteger, out long l, out double d, Executive exec)
    {
        isInteger = false;
        l = 0L;
        d = double.NaN;

        if (varIn is NameVar nameVar)
            varIn = exec.IdentifierTable[nameVar.Pointer];

        switch (varIn)
        {
            case IntegerVar integerVar:
                l = integerVar.Data;
                isInteger = true;
                return true;

            case RealVar realVar:
                d = realVar.Data;
                return true;

            case StringVar stringVar:
                if (!ToInteger(stringVar.Data, out l))
                    return ToReal(stringVar.Data, out d);

                isInteger = true;
                return true;

            default:
                return false;
        }
    }

    #endregion
}