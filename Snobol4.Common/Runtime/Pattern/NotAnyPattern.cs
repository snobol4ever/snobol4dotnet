using System.Buffers;

namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that matches a single character NOT in a specified set.
/// In SNOBOL4, this is created using the NOTANY() function.
/// </summary>
/// <remarks>
/// <para>
/// NOTANY is the inverse of ANY - it matches exactly one character that does NOT
/// appear in the specified character set. This is useful for finding characters
/// that don't match certain criteria.
/// </para>
/// <para>
/// The pattern fails if:
/// - The cursor is at the end of the subject string
/// - The current character IS in the excluded character set
/// </para>
/// <para>
/// NOTANY is particularly useful for skipping unwanted characters or finding
/// boundaries between different character classes.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Match a single non-vowel
/// notVowel = notany('aeiou')
/// subject = 'vacuum'
/// subject notVowel . v1           // v1 = "v"
///
/// // Combine with ANY
/// vowelThenConsonant = any('aeiou') notany('aeiou')
/// subject = 'vacuum'
/// subject vowelThenConsonant . v1 // v1 = "ac"
///
/// // Match any character except digits
/// notDigit = notany('0123456789')
/// subject = 'abc123'
/// subject notDigit . c            // c = "a"
///
/// // Skip whitespace
/// notSpace = notany(' \t\n\r')
/// </code>
/// </example>
internal class NotAnyPattern : TerminalPattern
{
    #region Members

    private readonly string _charList;

    /// <summary>
    /// Optimized character search values using hardware acceleration when available.
    /// SearchValues provides vectorized character matching for significantly improved performance.
    /// </summary>
    private readonly SearchValues<char> _searchValues;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a NOTANY pattern that matches characters not in the specified set
    /// </summary>
    /// <param name="charList">String of characters to exclude from matching</param>
    internal NotAnyPattern(string charList)
    {
        _charList = charList;
        // Create SearchValues for hardware-accelerated character matching
        _searchValues = SearchValues.Create(charList);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep copy of this NOTANY pattern
    /// </summary>
    /// <returns>A new NotAnyPattern with the same excluded character set</returns>
    internal override Pattern Clone()
    {
        return new NotAnyPattern(_charList);
    }

    /// <summary>
    /// Attempts to match a single character NOT in the excluded set
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>
    /// Success if current character is not in the excluded set and cursor advances by one,
    /// Failure if at end of subject or character is in the excluded set
    /// </returns>
    /// <remarks>
    /// Uses SearchValues for hardware-accelerated character matching, providing significant
    /// performance improvements for character set exclusion checks.
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        // Check if at end of subject
        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        var currentChar = scan.Subject[scan.CursorPosition];

        // Use SearchValues for optimized inverse matching
        // Match if character is NOT in the excluded list
        if (!_searchValues.Contains(currentChar))
        {
            scan.CursorPosition++;
            return MatchResult.Success(scan);
        }

        return MatchResult.Failure(scan);
    }

    #endregion
}