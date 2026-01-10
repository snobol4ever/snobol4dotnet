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
[DebuggerDisplay("{DebugString()}")]
internal class AnyPattern : TerminalPattern
{
    #region Members

    private string _charList;
    private readonly ExpressionVar? _expression;

    /// <summary>
    /// Optimized character search values using hardware acceleration when available.
    /// Nullable to support dynamic expression evaluation.
    /// </summary>
    private SearchValues<char>? _searchValues;

    #endregion

    #region Construction

    /// <summary>
    /// Creates an ANY pattern with a literal character set
    /// </summary>
    /// <param name="charList">String containing characters to match</param>
    internal AnyPattern(string charList)
    {
        _charList = charList;
        // Create SearchValues for hardware-accelerated character matching
        _searchValues = SearchValues.Create(charList);
    }

    /// <summary>
    /// Creates an ANY pattern with an expression that evaluates to a character set
    /// </summary>
    /// <param name="expression">Expression that produces the character set at match time</param>
    internal AnyPattern(ExpressionVar expression)
    {
        _charList = "";
        _expression = expression;
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
        return new AnyPattern(_charList);
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
    /// Uses SearchValues for hardware-accelerated character matching, providing significant
    /// performance improvements for character set lookups.
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        // Check if at end of subject
        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        // If using expression, evaluate it to get the character set
        if (_expression != null)
        {
            _expression.FunctionName(scan.Exec);
            var result = scan.Exec.SystemStack.Pop();

            if (!result.Succeeded || !result.Convert(Executive.VarType.STRING, out _, out var value, scan.Exec))
            {
                scan.Exec.LogRuntimeException(43);
                return MatchResult.Failure(scan);
            }

            _charList = (string)value;
            // Create SearchValues for the evaluated expression
            _searchValues = SearchValues.Create(_charList);
        }

        var currentChar = scan.Subject[scan.CursorPosition];

        // Use SearchValues for optimized character matching
        if (_searchValues!.Contains(currentChar))
        {
            scan.CursorPosition++;
            return MatchResult.Success(scan);
        }

        return MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

    internal string DebugString()
    {
        return $"ANY PATTERN [{_charList}]";
    }
    #endregion
}