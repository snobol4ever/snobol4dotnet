using System.Buffers;
using System.Diagnostics;

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
/// <para>
/// NOTANY can accept either a literal string or an expression that evaluates to a string at match time.
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
///
/// // Dynamic character set (expression)
/// chars = 'abc'
/// pattern = notany(chars)         // Evaluates chars at match time
/// </code>
/// </example>
[DebuggerDisplay("{DebugString()}")]
internal class NotAnyPattern : TerminalPattern
{
    #region Members

    private string _charList;
    private readonly ExpressionVar? _expression;

    /// <summary>
    /// Optimized character search values using hardware acceleration when available.
    /// SearchValues provides vectorized character matching for significantly improved performance.
    /// Nullable to support dynamic expression evaluation.
    /// </summary>
    private SearchValues<char>? _searchValues;

    /// <summary>
    /// Cached character list from last expression evaluation to avoid recreating SearchValues.
    /// Only used when _expression is not null.
    /// </summary>
    private string _lastEvaluatedCharList = "";

    /// <summary>
    /// Threshold for using SearchValues. For very small character sets (1-3 chars),
    /// direct comparison is faster than SearchValues overhead.
    /// </summary>
    private const int SearchValuesThreshold = 4;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a NOTANY pattern with a literal character set
    /// </summary>
    /// <param name="charList">String of characters to exclude from matching</param>
    internal NotAnyPattern(string charList)
    {
        _charList = charList;
        _expression = null;
        // Create SearchValues only for larger character sets
        _searchValues = charList.Length >= SearchValuesThreshold
            ? SearchValues.Create(charList)
            : null;
    }

    /// <summary>
    /// Creates a NOTANY pattern with an expression that evaluates to a character set
    /// </summary>
    /// <param name="expression">Expression that produces the excluded character set at match time</param>
    internal NotAnyPattern(ExpressionVar expression)
    {
        _charList = "";
        _expression = expression;
        _searchValues = null; // Will be created after expression evaluation
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep copy of this NOTANY pattern
    /// </summary>
    /// <returns>A new NotAnyPattern with the same excluded character set</returns>
    internal override Pattern Clone()
    {
        return _expression != null
            ? new NotAnyPattern(_expression)
            : new NotAnyPattern(_charList);
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
    /// Uses SearchValues for hardware-accelerated character matching with larger character sets,
    /// or direct comparison for small sets (1-3 chars) to avoid SearchValues overhead.
    /// Provides significant performance improvements for character set exclusion checks.
    /// Caches SearchValues for expression-based patterns to avoid recreating on every match.
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        // Check if at end of subject
        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        // If using expression, evaluate it to get the excluded character set
        if (_expression != null)
        {
            _expression.FunctionName(scan.Exec);
            var result = scan.Exec.SystemStack.Pop();

            if (!result.Succeeded || !result.Convert(Executive.VarType.STRING, out _, out var value, scan.Exec))
            {
                scan.Exec.LogRuntimeException(151);
                return MatchResult.Failure(scan);
            }

            _charList = (string)value;

            // Handle empty string - NOTANY with empty exclusion list matches any character
            if (string.IsNullOrEmpty(_charList))
            {
                scan.CursorPosition++;
                return MatchResult.Success(scan);
            }

            // Only recreate SearchValues if charset has changed (optimization for expression patterns)
            if (_charList != _lastEvaluatedCharList)
            {
                _lastEvaluatedCharList = _charList;
                _searchValues = _charList.Length >= SearchValuesThreshold
                    ? SearchValues.Create(_charList)
                    : null;
            }
        }

        var currentChar = scan.Subject[scan.CursorPosition];

        // Optimize for small character sets (1-3 chars) with direct comparison
        bool isInSet;
        if (_searchValues == null)
        {
            // Direct comparison is faster for small sets
            isInSet = _charList.Length switch
            {
                1 => currentChar == _charList[0],
                2 => currentChar == _charList[0] || currentChar == _charList[1],
                3 => currentChar == _charList[0] || currentChar == _charList[1] || currentChar == _charList[2],
                _ => _charList.Contains(currentChar) // Fallback for 0-length (shouldn't happen)
            };
        }
        else
        {
            // Use SearchValues for larger sets (hardware-accelerated)
            isInSet = _searchValues.Contains(currentChar);
        }

        // Match if character is NOT in the excluded list
        if (!isInSet)
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
        return _expression != null
            ? "NOTANY PATTERN [expression]"
            : $"NOTANY PATTERN [exclude: {_charList}]";
    }

    #endregion
}