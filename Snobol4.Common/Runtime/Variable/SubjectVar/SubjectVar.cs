#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents a string subject with a successful pattern match result.
/// Used for pattern replacement operations.
/// </summary>
[DebuggerDisplay("{DebugString()}")]
public class SubjectVar : Var
{
    #region Data

    internal string Subject;
    internal MatchResult MatchResult;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly SubjectArithmeticStrategy _arithmeticStrategy = new();
    private static readonly SubjectComparisonStrategy _comparisonStrategy = new();
    private static readonly SubjectConversionStrategy _conversionStrategy = new();
    private static readonly SubjectCloningStrategy _cloningStrategy = new();
    private static readonly SubjectFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors

    internal SubjectVar(string subject, MatchResult matchResult)
    {
        Subject = subject;
        MatchResult = matchResult;
    }

    internal SubjectVar(SubjectVar template)
    {
        Subject = template.Subject;
        MatchResult = template.MatchResult;
        Symbol = template.Symbol;
        InputChannel = template.InputChannel;
        OutputChannel = template.OutputChannel;
    }

    #endregion

    #region SubjectVar-Specific Methods

    /// <summary>
    /// Replace the matched portion of the subject with a replacement string
    /// </summary>
    internal StringVar MatchReplace(string replacement)
    {
        // Build the result string
        var before = Subject[..MatchResult.PreCursor];
        var after = Subject[MatchResult.PostCursor..];
        var result = before + replacement + after;

        // Create a new StringVar with the replaced content
        return new StringVar(result)
        {
            Symbol = Symbol,
            InputChannel = InputChannel,
            OutputChannel = OutputChannel
        };
    }

    /// <summary>
    /// Get the matched portion of the subject string
    /// </summary>
    public string GetMatchedString()
    {
        return Subject.Substring(MatchResult.PreCursor, MatchResult.PostCursor - MatchResult.PreCursor);
    }

    /// <summary>
    /// Get the portion before the match
    /// </summary>
    public string GetBeforeMatch()
    {
        return Subject[..MatchResult.PreCursor];
    }

    /// <summary>
    /// Get the portion after the match
    /// </summary>
    public string GetAfterMatch()
    {
        return Subject[MatchResult.PostCursor..];
    }

    /// <summary>
    /// Get the match start position
    /// </summary>
    public int GetMatchStart()
    {
        return MatchResult.PreCursor;
    }

    /// <summary>
    /// Get the match end position
    /// </summary>
    public int GetMatchEnd()
    {
        return MatchResult.PostCursor;
    }

    /// <summary>
    /// Get the length of the matched portion
    /// </summary>
    public int GetMatchLength()
    {
        return MatchResult.PostCursor - MatchResult.PreCursor;
    }

    #endregion

    #region Double Dispatch Methods

    // Subject variables don't support arithmetic operations with other types

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