using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class ArbNoPattern : TerminalPattern
{
    #region Members

                private readonly Pattern _arbPattern;
                private readonly Pattern _mustAdvance;

    #endregion

    #region Construction

                        internal ArbNoPattern(Pattern arbPattern)
    {
        _arbPattern = arbPattern ?? throw new ArgumentNullException(nameof(arbPattern));
        // Every repetition after the zeroth must advance the cursor, as in SPITBOL: the argument is
        // matched followed by a guard that fails where it started, so the child scanner backtracks
        // into the argument until it finds a non-null match. A null repetition would re-enter this
        // node at the same cursor forever, and the zero-repetition arm already matches null.
        _mustAdvance = new ConcatenatePattern(arbPattern, new MustAdvancePattern());
    }

    #endregion

    #region Methods

                                                                    internal static Pattern Structure(Pattern arbPattern)
    {
        return new ConcatenatePattern(
            new NullPattern(), 
            new AlternatePattern(
                new NullPattern(), 
                new ArbNoPattern(arbPattern)));
    }

                    internal override Pattern Clone()
    {
        return new ArbNoPattern(_arbPattern);
    }

                                                                                                            internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Arbno", scan.Exec);
        
        // Create a fresh scanner for matching the child pattern
        var reScan = new Scanner(scan.Exec);

        // Try to match the child pattern against the remaining subject
        // Use anchor=true to match at the current position only
        var mr = reScan.PatternMatch(
            scan.Subject[scan.CursorPosition..], 
            _mustAdvance, 
            0, 
            true);

        // If child pattern failed or aborted, propagate the result
        if (mr.Outcome is MatchResult.Status.FAILURE or MatchResult.Status.ABORT)
            return mr;

        // Child pattern succeeded - advance cursor past the matched text
        scan.CursorPosition += mr.PostCursor;
        
        // Save alternate to enable backtracking for fewer repetitions
        // This allows trying N-1 repetitions if N repetitions cause later failure
        scan.SaveAlternate(node);
        
        return MatchResult.Success(scan);
    }
    #endregion

    #region Debugging


                                       public override string DebugPattern() => "arbno";

    #endregion
}

/// <summary>
/// Succeeds only past the start of the subject it is matched against. ArbNoPattern appends it to
/// its argument so that each repetition consumes at least one character.
/// </summary>
internal sealed class MustAdvancePattern : TerminalPattern
{
    internal override Pattern Clone()
    {
        return new MustAdvancePattern();
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        return scan.CursorPosition > 0
            ? MatchResult.Success(scan)
            : MatchResult.Failure(scan);
    }

    public override string DebugPattern() => "must-advance";
}
