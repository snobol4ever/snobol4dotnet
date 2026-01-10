namespace Snobol4.Common;

internal class ArbNoPattern : TerminalPattern
{
    #region Members

    private readonly Pattern _arbPattern;

    #endregion

    #region Construction

    internal ArbNoPattern(Pattern arbPattern)
    {
        _arbPattern = arbPattern;
    }

    #endregion

    #region Methods

    internal static Pattern Structure(Pattern arbPattern)
    {
        return new ConcatenatePattern(new NullPattern(), new AlternatePattern(new NullPattern(), new ArbNoPattern(arbPattern)));
    }

    internal override Pattern Clone()
    {
        return new ArbNoPattern(_arbPattern);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        var reScan = new Scanner(scan.Exec);

        var mr = reScan.PatternMatch(scan.Subject[scan.CursorPosition..], _arbPattern, 0, true);

        if (mr.Outcome is MatchResult.Status.FAILURE or MatchResult.Status.ABORT)
            return mr;

        scan.CursorPosition += mr.PostCursor;
        scan.SaveAlternate(node);
        return MatchResult.Success(scan);
    }
    #endregion
}