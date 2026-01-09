#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugString()}")]
public class PatternVar : Var
{
    #region Data

    internal Pattern Data;

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

    /// <summary>
    /// Match this pattern against a subject string
    /// </summary>
    public MatchResult Match(string subject, Executive executive, int startPosition = 0, bool anchor = false)
    {
        var scanner = new Scanner(executive);
        return scanner.PatternMatch(subject, Data, startPosition, anchor);
    }

    /// <summary>
    /// Concatenate this pattern with another pattern
    /// </summary>
    public PatternVar Concatenate(PatternVar other)
    {
        return new PatternVar(new ConcatenatePattern(Data, other.Data));
    }

    /// <summary>
    /// Create an alternation between this pattern and another pattern
    /// </summary>
    public PatternVar Alternate(PatternVar other)
    {
        return new PatternVar(new AlternatePattern(Data, other.Data));
    }

    /// <summary>
    /// Create a pattern that matches zero or more occurrences of this pattern
    /// </summary>
    public PatternVar ArbNo()
    {
        return new PatternVar(ArbNoPattern.Structure(Data));
    }

    /// <summary>
    /// Get information about the pattern structure
    /// </summary>
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
    {
        executive.LogRuntimeException(2); // Right operand of + is not numeric
        return StringVar.Null();
    }

    protected internal override Var AddReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(2); // Right operand of + is not numeric
        return StringVar.Null();
    }

    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(33); // Right operand of - is not numeric
        return StringVar.Null();
    }

    protected internal override Var SubtractReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(33); // Right operand of - is not numeric
        return StringVar.Null();
    }

    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(27); // Right operand of * is not numeric
        return StringVar.Null();
    }

    protected internal override Var MultiplyReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(27); // Right operand of * is not numeric
        return StringVar.Null();
    }

    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(13); // Right operand of / is not numeric
        return StringVar.Null();
    }

    protected internal override Var DivideReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(13); // Right operand of / is not numeric
        return StringVar.Null();
    }

    #endregion
}