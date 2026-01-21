#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class RealVar : Var
{
    #region Data

    public double Data;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly RealArithmeticStrategy _arithmeticStrategy = new();
    private static readonly RealComparisonStrategy _comparisonStrategy = new();
    private static readonly RealConversionStrategy _conversionStrategy = new();
    private static readonly RealCloningStrategy _cloningStrategy = new();
    private static readonly RealFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors


    public RealVar(double data)
    {
        InputChannel = string.Empty;
        OutputChannel = string.Empty;
        Symbol = string.Empty;
        Data = data;
    }


    public RealVar(RealVar template)
    {
        Symbol = template.Symbol;
        Data = template.Data;
        InputChannel = template.InputChannel;
        OutputChannel = template.OutputChannel;
    }


    public RealVar(string symbol, double data, string inputChannel, string outputChannel)
    {
        Symbol = symbol;
        Data = data;
        InputChannel = inputChannel;
        OutputChannel = outputChannel;
    }

    #endregion

    #region Double Dispatch Methods

    // These methods handle type-specific arithmetic with real numbers


    protected internal override Var AddInteger(IntegerVar left, Executive executive)
    {
        return new RealVar(left.Data + Data);
    }


    protected internal override Var AddReal(RealVar left, Executive executive)
    {
        var result = left.Data + Data;

        if (!double.IsFinite(result))
        {
            executive.LogRuntimeException(261);
            return StringVar.Null();
        }

        return new RealVar(result);
    }


    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
    {
        return new RealVar(left.Data - Data);
    }


    protected internal override Var SubtractReal(RealVar left, Executive executive)
    {
        var result = left.Data - Data;

        if (!double.IsFinite(result))
        {
            executive.LogRuntimeException(264);
            return StringVar.Null();
        }

        return new RealVar(result);
    }


    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
    {
        return new RealVar(left.Data * Data);
    }


    protected internal override Var MultiplyReal(RealVar left, Executive executive)
    {
        var result = left.Data * Data;

        if (!double.IsFinite(result))
        {
            executive.LogRuntimeException(263);
            return StringVar.Null();
        }

        return new RealVar(result);
    }


    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
    {
        if (Data == 0.0)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        return new RealVar(left.Data / Data);
    }


    protected internal override Var DivideReal(RealVar left, Executive executive)
    {
        if (Data == 0.0)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        var result = left.Data / Data;

        if (!double.IsFinite(result))
        {
            executive.LogRuntimeException(262);
            return StringVar.Null();
        }

        return new RealVar(result);
    }

    #endregion
}