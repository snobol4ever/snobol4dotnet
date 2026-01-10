#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents a string subject with a successful pattern match result.
/// Used for pattern replacement operations.
/// </summary>
[DebuggerDisplay("{DebugString()}")]
public sealed class SubjectVar : Var
{
    #region Data

    internal string Subject;
    internal MatchResult MatchResult;

    #endregion

    #region Strategy Instances (Singletons for performance)

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
        // Optimized: Use string.Create for zero-allocation string building when possible,
        // or use span-based operations to minimize allocations
        var beforeLength = MatchResult.PreCursor;
        var afterStart = MatchResult.PostCursor;
        var afterLength = Subject.Length - afterStart;
        var totalLength = beforeLength + replacement.Length + afterLength;

        // For small strings, use stackalloc span; for large ones, use string.Create
        if (totalLength <= 256)
        {
            Span<char> buffer = stackalloc char[totalLength];
            Subject.AsSpan(0, beforeLength).CopyTo(buffer);
            replacement.AsSpan().CopyTo(buffer.Slice(beforeLength));
            Subject.AsSpan(afterStart, afterLength).CopyTo(buffer.Slice(beforeLength + replacement.Length));

            return new StringVar(new string(buffer))
            {
                Symbol = Symbol,
                InputChannel = InputChannel,
                OutputChannel = OutputChannel
            };
        }

        // For larger strings, use string.Create to avoid intermediate allocations
        var result = string.Create(totalLength, (Subject, replacement, beforeLength, afterStart, afterLength),
            (span, state) =>
            {
                state.Subject.AsSpan(0, state.beforeLength).CopyTo(span);
                state.replacement.AsSpan().CopyTo(span.Slice(state.beforeLength));
                state.Subject.AsSpan(state.afterStart, state.afterLength).CopyTo(span.Slice(state.beforeLength + state.replacement.Length));
            });

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
        // Optimized: Direct length calculation and substring
        var length = MatchResult.PostCursor - MatchResult.PreCursor;
        return Subject.Substring(MatchResult.PreCursor, length);
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