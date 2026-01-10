namespace Snobol4.Common;

internal class NotAnyPattern : TerminalPattern
{
    #region Members

    private readonly string _charList;

    #endregion

    #region Constructors

    /// <summary>
    /// NOTANY(string) is a primitive function whose values is a pattern  that 
    /// matches a single characters. NOTANY matches any character not appearing in its argument.
    /// Thus, the pattern for NOTANY('AEIOU') matches any non-vowel. The argument of 
    /// NOTANY must be a non-null string when pattern matching is performed. ANY is a fast 
    /// way of looking for one excluded single characters.
    /// </summary>
    /// <param name="charList">String of characters to not match</param>
    internal NotAnyPattern(string charList)
    {
        _charList = charList;
    }

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return new NotAnyPattern(_charList);
    }

    /// <summary>
    /// Scan the Subject against the NOTANY pattern.
    /// </summary>
    /// <returns>Match Result</returns>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        return !_charList.Contains(scan.Subject[scan.CursorPosition++]) ? MatchResult.Success(scan) : MatchResult.Failure(scan);
    }

    #endregion
}