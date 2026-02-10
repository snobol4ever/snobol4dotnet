using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class ArbNoPattern : TerminalPattern
{
    #region Members

                private readonly Pattern _arbPattern;

    #endregion

    #region Construction

                        internal ArbNoPattern(Pattern arbPattern)
    {
        _arbPattern = arbPattern ?? throw new ArgumentNullException(nameof(arbPattern));
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
            _arbPattern, 
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