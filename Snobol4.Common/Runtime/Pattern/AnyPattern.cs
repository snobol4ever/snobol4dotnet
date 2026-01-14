using System.Buffers;
using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that matches a single character from a specified set.
/// In SNOBOL4, this is created using the ANY() function.
/// </summary>
/// <remarks>
/// <para>
/// ANY matches exactly one character that appears in the specified character set.
/// It's a fast way to match one of several possible characters without using alternation.
/// </para>
/// <para>
/// The pattern fails if:
/// - The cursor is at the end of the subject string
/// - The current character is not in the character set
/// </para>
/// <para>
/// ANY can accept either a literal string or an expression that evaluates to a string at match time.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Match a single vowel
/// vowel = any('aeiou')
/// subject = 'vacuum'
/// subject vowel . v1              // v1 = "a"
///
/// // Match consecutive vowels
/// dvowel = any('aeiou') any('aeiou')
/// subject = 'vacuum'
/// subject dvowel . v1             // v1 = "uu"
///
/// // Common use: parse character by character
/// digit = any('0123456789')
/// subject = '42'
/// subject digit . d1 digit . d2   // d1 = "4", d2 = "2"
///
/// // Dynamic character set (expression)
/// chars = 'abc'
/// pattern = any(chars)            // Evaluates chars at match time
/// </code>
/// </example>
[DebuggerDisplay("{DebugPattern()}")]
internal class AnyPattern : TerminalPattern
{
    #region Members

    private string _charList;
    private Executive.DeferredCode? _functionName;

    /// <summary>
    /// Optimized character search values using hardware acceleration when available.
    /// Nullable to support dynamic expression evaluation and small character set optimization.
    /// </summary>
    private SearchValues<char>? _searchValues;

    /// <summary>
    /// Threshold for using SearchValues. For very small character sets (1-3 chars),
    /// direct comparison is faster than SearchValues overhead.
    /// </summary>
    private const int SearchValuesThreshold = 4;

    // Cache the last evaluated character set for expressions
    private string _lastEvaluatedCharList = "";

    #endregion

    #region Construction

    /// <summary>
    /// Creates an ANY pattern with a literal character set
    /// </summary>
    /// <param name="charList">String containing characters to match</param>
    internal AnyPattern(string charList)
    {
        _charList = charList;
        _functionName = null;
        // Create SearchValues only for larger character sets
        _searchValues = charList.Length >= SearchValuesThreshold
            ? SearchValues.Create(charList)
            : null;
    }

    /// <summary>
    /// Creates an ANY pattern with an expression that evaluates to a character set
    /// </summary>
    /// <param name="functionName">Method that produces the character set at match time</param>
    internal AnyPattern(Executive.DeferredCode functionName)
    {
        _charList = "";
        _functionName = functionName;
        _searchValues = null; // Will be created after expression evaluation
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep copy of this ANY pattern
    /// </summary>
    /// <returns>A new AnyPattern with the same character set</returns>
    internal override AnyPattern Clone()
    {
        return _functionName != null
            ? new AnyPattern(_functionName)
            : new AnyPattern(_charList);
    }

    /// <summary>
    /// Attempts to match a single character from the character set
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>
    /// Success if current character is in the set and cursor advances by one,
    /// Failure if at end of subject or character not in set
    /// </returns>
    /// <remarks>
    /// Uses SearchValues for hardware-accelerated character matching with larger character sets,
    /// or direct comparison for small sets (1-3 chars) to avoid SearchValues overhead.
    /// Provides significant performance improvements for character set lookups.
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        // Check if at end of subject
        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        // If using expression, evaluate it to get the character set
        if (_functionName != null)
        {
            _functionName(scan.Exec);
            var result = scan.Exec.SystemStack.Pop();

            if (!result.Succeeded || !result.Convert(Executive.VarType.STRING, out _, out var value, scan.Exec))
            {
                scan.Exec.LogRuntimeException(43);
                return MatchResult.Failure(scan);
            }

            _charList = (string)value;

            // Only recreate SearchValues if charset has changed
            if (_charList != _lastEvaluatedCharList)
            {
                _lastEvaluatedCharList = _charList;
                _searchValues = _charList.Length >= SearchValuesThreshold
                    ? SearchValues.Create(_charList)
                    : null;
            }
        }

        var currentChar = scan.Subject[scan.CursorPosition];

        // Fast path for single-character sets (common case)
        if (_charList.Length == 1)
        {
            if (currentChar == _charList[0])
            {
                scan.CursorPosition++;
                return MatchResult.Success(scan);
            }
            return MatchResult.Failure(scan);
        }

        // Optimize for small character sets (2-3 chars) with direct comparison
        bool isInSet;
        if (_searchValues == null)
        {
            // Direct comparison is faster for small sets
            isInSet = _charList.Length switch
            {
                1 => currentChar == _charList[0],
                2 => currentChar == _charList[0] || currentChar == _charList[1],
                3 => currentChar == _charList[0] || currentChar == _charList[1] || currentChar == _charList[2],
                _ => false // Empty string - no characters to match
            };
        }
        else
        {
            // Use SearchValues for larger sets (hardware-accelerated)
            isInSet = _searchValues.Contains(currentChar);
        }

        if (isInSet)
        {
            scan.CursorPosition++;
            return MatchResult.Success(scan);
        }

        return MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

    /// <summary>
    /// Returns a debug string representation of this pattern for diagnostic purposes.
    /// </summary>
    /// <returns>
    /// Either "any(*)" for expression-based patterns, or "any[&lt;characters&gt;]" 
    /// for literal character set patterns, where &lt;characters&gt; is the character set string.
    /// </returns>
    /// <remarks>
    /// This method is used by the debugger display attribute and diagnostic tools
    /// to provide a concise, human-readable representation of the pattern.
    /// The asterisk (*) indicates the character set is determined by evaluating an expression at match time.
    /// </remarks>
    public override string DebugPattern() => "any";

    #endregion
}