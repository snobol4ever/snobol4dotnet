#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class IntegerVar : Var
{
    #region Data

    public long Data;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly IntegerArithmeticStrategy _arithmeticStrategy = new();
    private static readonly IntegerComparisonStrategy _comparisonStrategy = new();
    private static readonly IntegerConversionStrategy _conversionStrategy = new();
    private static readonly IntegerCloningStrategy _cloningStrategy = new();
    private static readonly IntegerFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors

            
    public static IntegerVar Create(long value)
    {
        return new IntegerVar(value);
    }
    
    public IntegerVar(long data)
    {
        Data = data;
    }

    public IntegerVar(
        long data = 0,
        string symbol = "",
        bool isKeyword = false,
        bool isReadOnly = false,
        string inputChannel = "",
        string outputChannel = "",
        ValidationDelegate? validation = null)
    {
        Data = data;
        Symbol = symbol;
        IsKeyword = isKeyword;
        IsReadOnly = isReadOnly;
        InputChannel = inputChannel;
        OutputChannel = outputChannel;
        Validation = validation;
    }

    internal IntegerVar(IntegerVar template)
    {
        OutputChannel = template.OutputChannel;
        InputChannel = template.InputChannel;
        Symbol = template.Symbol;
        Data = template.Data;
    }

    #endregion

    #region Double Dispatch Methods

    // These methods handle type-specific arithmetic with integers


    protected internal override Var AddInteger(IntegerVar left, Executive executive)
    {
        var leftData = left.Data;
        var rightData = Data;

        // Use checked arithmetic for overflow detection
        try
        {
            var result = checked(leftData + rightData);
            return Create(result);
        }
        catch (OverflowException)
        {
            executive.LogRuntimeException(3);
            return StringVar.Null();
        }
    }


    protected internal override Var AddReal(RealVar left, Executive executive)
    {
        return new RealVar(left.Data + Data);
    }


    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
    {
        var leftData = left.Data;
        var rightData = Data;

        // Use checked arithmetic for overflow detection
        try
        {
            var result = checked(leftData - rightData);
            return Create(result);
        }
        catch (OverflowException)
        {
            executive.LogRuntimeException(34);
            return StringVar.Null();
        }
    }


    protected internal override Var SubtractReal(RealVar left, Executive executive)
    {
        return new RealVar(left.Data - Data);
    }


    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
    {
        var leftData = left.Data;
        var rightData = Data;

        // Fast path for common cases using pattern matching
        switch (leftData, rightData)
        {
            case (0, _) or (_, 0):
                return Create(0);
            case (1, _):
                return Create(rightData);
            case (_, 1):
                return Create(leftData);
            case (-1, _):
                return Create(-rightData);
            case (_, -1):
                return Create(-leftData);
        }

        // Use checked arithmetic for overflow detection
        try
        {
            var result = checked(leftData * rightData);
            return Create(result);
        }
        catch (OverflowException)
        {
            executive.LogRuntimeException(28);
            return StringVar.Null();
        }
    }


    protected internal override Var MultiplyReal(RealVar left, Executive executive)
    {
        return new RealVar(left.Data * Data);
    }


    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
    {
        var divisor = Data;

        if (divisor == 0)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        var dividend = left.Data;

        // Check for overflow case: MinValue / -1 causes overflow
        if (dividend == long.MinValue && divisor == -1)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        return Create(dividend / divisor);
    }


    protected internal override Var DivideReal(RealVar left, Executive executive)
    {
        if (Data == 0)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        return new RealVar(left.Data / Data);
    }

    #endregion
}