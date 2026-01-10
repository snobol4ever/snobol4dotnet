namespace Snobol4.Common;

internal class RPosPattern : TerminalPattern
{
    #region Members

    internal int Position;

    #endregion

    #region Construction

    internal RPosPattern(int position)
    {
        Position = position;
    }

    #endregion

    #region Internal Methods

    internal override Pattern Clone()
    {
        return new RPosPattern(Position);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        return Position == scan.Subject.Length - scan.CursorPosition
            ? MatchResult.Success(scan)
            : MatchResult.Failure(scan);
    }

    #endregion
}
