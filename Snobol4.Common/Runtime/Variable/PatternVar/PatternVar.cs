#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public class PatternVar : Var
{
    #region Data

    public Pattern Data;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly PatternArithmeticStrategy _arithmeticStrategy = new();
    private static readonly PatternComparisonStrategy _comparisonStrategy = new();
    private static readonly PatternConversionStrategy _conversionStrategy = new();
    private static readonly PatternCloningStrategy _cloningStrategy = new();
    private static readonly PatternFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors


    public PatternVar(Pattern data)
    {
        Data = data;
    }


    public PatternVar(
        Pattern data,
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


    public PatternVar(PatternVar template)
    {
        OutputChannel = template.OutputChannel;
        InputChannel = template.InputChannel;
        Symbol = template.Symbol;
        Validation = template.Validation;
        IsReadOnly = template.IsReadOnly;
        Succeeded = template.Succeeded;
        Data = template.Data;
    }

    #endregion

    #region Pattern-Specific Methods

            
    public MatchResult Match(string subject, Executive executive, int startPosition = 0, bool anchor = false)
    {
        var scanner = new Scanner(executive);
        return scanner.PatternMatch(subject, Data, startPosition, anchor);
    }

            
    public PatternVar Concatenate(PatternVar other)
    {
        return new PatternVar(new ConcatenatePattern(Data, other.Data));
    }

            
    public PatternVar Alternate(PatternVar other)
    {
        return new PatternVar(new AlternatePattern(Data, other.Data));
    }

            
    public PatternVar ArbNo()
    {
        return new PatternVar(ArbNoPattern.Structure(Data));
    }

                public string GetPatternInfo()
    {
        return Data switch
        {
            LiteralPattern lit => $"Literal: '{lit.Literal}'",
            ConcatenatePattern => "Concatenation",
            AlternatePattern => "Alternation",
            ArbPattern => "Arbitrary string",
            ArbNoPattern => "Zero or more repetitions",
            BalPattern => "Balanced parentheses",
            _ => "Pattern"
        };
    }

    #endregion

    #region Double Dispatch Methods

    // Patterns don't support arithmetic operations with other types


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