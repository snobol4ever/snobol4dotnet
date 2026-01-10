namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that succeeds only if the cursor is at a specific position.
/// In SNOBOL4, this is created using the POS() function.
/// </summary>
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