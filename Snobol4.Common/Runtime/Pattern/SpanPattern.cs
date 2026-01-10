namespace Snobol4.Common;

internal class SpanPattern : TerminalPattern
{
    #region Members

    private readonly string _charList;

    #endregion

    #region Constructors

    internal SpanPattern(string charList)
    {
        _charList = charList;
    }

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return new SpanPattern(_charList);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        var match = false;

        while (scan.CursorPosition < scan.Subject.Length && _charList.Contains(scan.Subject[scan.CursorPosition]))
        {
            ++scan.CursorPosition;
            match = true;
        }

        return match ? MatchResult.Success(scan) : MatchResult.Failure(scan);
    }

    #endregion
}