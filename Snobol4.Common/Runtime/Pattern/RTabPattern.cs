namespace Snobol4.Common;

internal class RTabPattern : TerminalPattern
{
    #region Members

    internal int Position;

    #endregion

    #region Construction

    internal RTabPattern(int position)
    {
        Position = position;
    }

    #endregion

    #region Internal Methods

    internal override Pattern Clone()
    {
        return new RTabPattern(Position);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        if (scan.CursorPosition > scan.Subject.Length || Position > scan.Subject.Length)
            return MatchResult.Failure(scan);

        scan.CursorPosition = scan.Subject.Length - Position;
        return MatchResult.Success(scan);
    }

    #endregion
}