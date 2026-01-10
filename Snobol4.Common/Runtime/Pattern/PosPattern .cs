namespace Snobol4.Common;

internal class PosPattern : TerminalPattern
{
    #region Internal Members

    internal int Position;

    #endregion

    #region Construction

    internal PosPattern(int position)
    {
        Position = position;
    }

    #endregion

    #region Internal Methods

    internal override Pattern Clone()
    {
        return new PosPattern(Position);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        return Position == scan.CursorPosition ? MatchResult.Success(scan) : MatchResult.Failure(scan);
    }

    #endregion
}