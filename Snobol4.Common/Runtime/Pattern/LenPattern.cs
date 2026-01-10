namespace Snobol4.Common;

internal class LenPattern : TerminalPattern
{
    #region Members

    private readonly int _length;

    #endregion

    #region Constructors

    internal LenPattern(int length)
    {
        _length = length;
    }

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return new LenPattern(_length);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        scan.CursorPosition += _length;
        return scan.CursorPosition <= scan.Subject.Length ? MatchResult.Success(scan) : MatchResult.Failure(scan);
    }

    #endregion
}