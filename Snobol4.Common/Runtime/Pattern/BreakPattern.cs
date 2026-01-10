using System.Buffers;

namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that matches characters up to (but not including) a character from a specified set.
/// In SNOBOL4, this is created using the BREAK() function.
/// </summary>
/// <remarks>
/// <para>
/// BREAK matches a sequence of zero or more characters, stopping immediately before
/// the first character that appears in the specified character set (the "break characters").
/// The break character itself is not consumed by BREAK.
/// </para>
/// <para>
/// BREAK always succeeds if any break character is found in the remaining subject string,
/// even if it matches zero characters. This makes BREAK useful for finding delimiters or boundaries.
/// </para>
/// <para>
/// The pattern fails if:
/// - No break character is found in the remaining subject string
/// </para>
/// <para>
/// BREAK can accept either a literal string or an expression that evaluates to a string at match time.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Match up to a comma
/// subject = 'one,two,three'
/// subject break(',') . word       // word = "one", cursor before comma
///
/// // Match up to whitespace
/// letters = 'abcdefghijklmnopqrstuvwxyz'
/// word = break(' \t\n')
/// subject = 'hello world'
/// subject word . w1               // w1 = "hello"
///
/// // Extract key-value pairs
/// subject = 'NAME:VALUE'
/// subject break(':') . key ':'    // key = "NAME"
///         rem . value             // value = "VALUE"
///
/// // BREAK can match zero characters
/// subject = ',data'
/// subject break(',') . prefix     // prefix = "", succeeds immediately
/// </code>
/// </example>
internal class BreakPattern : TerminalPattern
{
    #region Members

    private string _charList;
    private readonly ExpressionVar? _expression;

    /// <summary>
    /// Optimized character search values using hardware acceleration when available.
    /// SearchValues provides vectorized character matching for significantly improved performance.
    /// Nullable to support dynamic expression evaluation and small character set optimization.
    /// </summary>
    private SearchValues<char>? _searchValues;

    /// <summary>
    /// Cached character list from last expression evaluation to avoid recreating SearchValues.
    /// Only used when _expression is not null.
    /// </summary>
    private string _lastEvaluatedCharList = "";

    /// <summary>
    /// Threshold for using SearchValues. For very small character sets (1-2 chars),
    /// direct IndexOfAny is competitive with SearchValues overhead.
    /// </summary>
    private const int SearchValuesThreshold = 3;

    #endregion

    #region Construction

    /// <summary>
    /// Creates a BREAK pattern with a literal set of break characters
    /// </summary>
    /// <param name="charList">String containing characters that stop the match</param>
    internal BreakPattern(string charList)
    {
        _charList = charList;
        _expression = null;
        // Create SearchValues only for larger character sets
        _searchValues = !string.IsNullOrEmpty(charList) && charList.Length >= SearchValuesThreshold
            ? SearchValues.Create(charList)
            : null;
    }

    /// <summary>
    /// Creates a BREAK pattern with an expression that evaluates to break characters
    /// </summary>
    /// <param name="expressionVar">Expression that produces the break characters at match time</param>
    internal BreakPattern(ExpressionVar expressionVar)
    {
        _charList = "";
        _expression = expressionVar;
        _searchValues = null; // Will be created after expression evaluation
    }

    /// <summary>
    /// Creates a BREAK pattern with both literal and expression
    /// </summary>
    /// <param name="charList">Literal break characters</param>
    /// <param name="expressionVar">Expression for additional break characters</param>
    internal BreakPattern(string charList, ExpressionVar? expressionVar)
    {
        _charList = charList;
        _expression = expressionVar;
        // Create SearchValues only for larger character sets
        _searchValues = !string.IsNullOrEmpty(charList) && charList.Length >= SearchValuesThreshold
            ? SearchValues.Create(charList)
            : null;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep copy of this BREAK pattern
    /// </summary>
    /// <returns>A new BreakPattern with the same break characters</returns>
    internal override Pattern Clone()
    {
        return new BreakPattern(_charList, _expression);
    }

    /// <summary>
    /// Matches characters up to a break character
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>
    /// Success if a break character is found (advances cursor to that position),
    /// Failure if no break character found in remaining subject
    /// </returns>
    /// <remarks>
    /// Uses SearchValues for hardware-accelerated character searching with larger character sets,
    /// or ReadOnlySpan.IndexOfAny for small sets to avoid SearchValues overhead.
    /// Caches SearchValues for expression-based patterns to avoid recreating on every match.
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        var charList = _charList;

        // If using expression, evaluate it to get the break characters
        if (_expression != null)
        {
            _expression.FunctionName(scan.Exec);
            var charVar = scan.Exec.SystemStack.Pop();

            if (!charVar.Convert(Executive.VarType.STRING, out _, out var str, scan.Exec) ||
                string.IsNullOrEmpty((string)str))
            {
                scan.Exec.LogRuntimeException(59);
                return MatchResult.Failure(scan);
            }

            charList = (string)str;

            // Only recreate SearchValues if charset has changed (optimization for expression patterns)
            if (charList != _lastEvaluatedCharList)
            {
                _lastEvaluatedCharList = charList;
                _searchValues = charList.Length >= SearchValuesThreshold
                    ? SearchValues.Create(charList)
                    : null;
            }
        }

        if (scan.Subject.Length == 0)
            return MatchResult.Failure(scan);

        // Use ReadOnlySpan to avoid allocating character arrays
        var searchSpan = scan.Subject.AsSpan(scan.CursorPosition);

        // Use SearchValues for larger sets, direct IndexOfAny for small sets
        int index;
        if (_searchValues != null)
        {
            // Hardware-accelerated search for larger character sets
            index = searchSpan.IndexOfAny(_searchValues);
        }
        else
        {
            // Direct IndexOfAny is efficient for small character sets
            index = searchSpan.IndexOfAny(charList.AsSpan());
        }

        if (index < 0)
            return MatchResult.Failure(scan);

        // Advance cursor to the break character (but don't consume it)
        scan.CursorPosition += index;
        return MatchResult.Success(scan);
    }

    #endregion
}