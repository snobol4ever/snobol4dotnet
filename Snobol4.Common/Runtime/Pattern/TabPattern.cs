namespace Snobol4.Common;

internal class TabPattern : TerminalPattern
{
    #region Members

    internal int Position;

    #endregion

    #region Construction

    internal override Pattern Clone()
    {
        return new TabPattern(Position);
    }

    internal TabPattern(int position)
    {
        Position = position;
    }

    #endregion

    #region Internal Methods

    internal override MatchResult Scan(int node, Scanner scan)
    {
        if (scan.CursorPosition > scan.Subject.Length || Position > scan.Subject.Length)
            return MatchResult.Failure(scan);

        scan.CursorPosition = Position;
        return MatchResult.Success(scan);
    }

    #endregion
}