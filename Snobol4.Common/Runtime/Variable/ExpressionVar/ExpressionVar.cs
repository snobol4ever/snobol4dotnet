#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class ExpressionVar : Var
{
    #region Data

    public Executive.DeferredCode FunctionName;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly ExpressionArithmeticStrategy _arithmeticStrategy = new();
    private static readonly ExpressionComparisonStrategy _comparisonStrategy = new();
    private static readonly ExpressionConversionStrategy _conversionStrategy = new();
    private static readonly ExpressionCloningStrategy _cloningStrategy = new();
    private static readonly ExpressionFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors


    internal ExpressionVar(Executive.DeferredCode functionName)
    {
        FunctionName = functionName;
    }


    internal ExpressionVar(ExpressionVar template)
    {
        OutputChannel = template.OutputChannel;
        InputChannel = template.InputChannel;
        Symbol = template.Symbol;
        FunctionName = template.FunctionName;
    }

    #endregion

    #region Expression-Specific Methods

            
    public void Evaluate(Executive executive)
    {
        FunctionName(executive);
    }

            
    public Executive.DeferredCode GetDelegate()
    {
        return FunctionName;
    }

    #endregion

    #region Double Dispatch Methods

    // Expressions don't support arithmetic operations with other types


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