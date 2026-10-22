#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// The value of a pattern match, S ? P: a string, the substring P matched, as SPITBOL gives it wherever a value is
/// wanted (V = (S ? P), (S ? P) 'X', SIZE(S ? P), a subscript, an argument).  It also keeps the subject and where P
/// matched, for the one use that needs them: as the left operand of =, S ? P = R replaces the matched substring in S.
/// A copy of it is a plain string (StringVar's cloning), so a variable, element or parameter given it never holds a
/// match to replace into.
/// </summary>
[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class SubjectVar : StringVar
{
    #region Data

    internal string Subject;
    internal MatchResult MatchResult;

    #endregion

    #region Constructors

    internal SubjectVar(string subject, MatchResult matchResult)
        : base(subject[matchResult.PreCursor..matchResult.PostCursor])
    {
        Subject = subject;
        MatchResult = matchResult;
    }

    #endregion

    #region SubjectVar-Specific Methods

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

    #endregion
}
