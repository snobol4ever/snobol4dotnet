using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that is evaluated lazily at match time.
/// This allows patterns to be constructed dynamically based on runtime conditions.
/// </summary>
/// <remarks>
/// <para>
/// UnevaluatedPattern defers pattern evaluation until the pattern match occurs.
/// This enables powerful dynamic pattern construction where the actual pattern
/// depends on runtime values or computations.
/// </para>
/// <para>
/// The pattern is created as a composite structure: NULL UnevaluatedPattern(no-rescan)
/// followed by alternation with NULL | UnevaluatedPattern(rescan). This structure
/// enables backtracking to re-evaluate the expression and try different matches.
/// </para>
/// <para>
/// When matched, the expression is evaluated, must return a pattern, and that
/// pattern is then matched against the remaining subject.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Dynamic pattern based on variable
/// patternType = 'digit'
/// pattern = patternType == 'digit' ? span('0123456789') : span(letters)
/// // Pattern adapts based on patternType value
///
/// // Pattern that depends on previous match
/// firstChar . c                   // Capture first character
/// *c                             // Match it again (unevaluated)
/// // Matches: 'aa', 'bb', 'cc', etc.
/// </code>
/// </example>
[DebuggerDisplay("{DebugPattern()}")]
internal class UnevaluatedPattern : TerminalPattern
{
    #region Members

    private readonly Executive.DeferredCode _functionName;
    private readonly bool _reScan;

    #endregion

    #region Construction

    /// <summary>
    /// Creates an unevaluated pattern with a deferred function
    /// </summary>
    /// <param name="functionName">Function that produces the pattern at match time</param>
    /// <param name="reScan">Whether to save alternate for backtracking and re-evaluation</param>
    internal UnevaluatedPattern(Executive.DeferredCode functionName, bool reScan)
    {
        _functionName = functionName;
        _reScan = reScan;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates the composite structure for an unevaluated pattern
    /// </summary>
    /// <param name="functionName">Function to evaluate at match time</param>
    /// <returns>A composite pattern enabling evaluation and backtracking</returns>
    internal static Pattern Structure(Executive.DeferredCode functionName)
    {
        var con2 = new ConcatenatePattern(
            new NullPattern(), 
            new UnevaluatedPattern(functionName, false));
        var alt = new AlternatePattern(
            new NullPattern(), 
            new UnevaluatedPattern(functionName, true));
        return new ConcatenatePattern(con2, alt);
    }

    internal override Pattern Clone()
    {
        return new UnevaluatedPattern(_functionName, _reScan);
    }

    /// <summary>
    /// Evaluates the deferred function and matches the resulting pattern
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>
    /// Success/Failure/Abort based on the evaluated pattern's match result,
    /// with optional backtracking support if rescan is enabled
    /// </returns>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("*", scan.Exec);

        // Evaluate the function to get the actual pattern
        _functionName(scan.Exec);

        if (scan.Exec.Failure)
            return MatchResult.Failure(scan);

        var evaluatedExpression = scan.Exec.SystemStack.Pop();
        if (!evaluatedExpression.Convert(Executive.VarType.PATTERN, out _, out var p, scan.Exec))
        {
            scan.Exec.LogRuntimeException(46);
            return MatchResult.Failure(scan);
        }

        // Match the evaluated pattern against the remaining subject
        var pattern = (Pattern)p;
        Scanner scanner = new(scan.Exec);
        var mr = scanner.PatternMatch(scan.Subject[scan.CursorPosition..], pattern, 0, true);
        scan.CursorPosition += mr.PostCursor;

        // If rescan is enabled and match succeeded, save alternate for backtracking
        if (_reScan && mr.Outcome == MatchResult.Status.SUCCESS)
            scan.SaveAlternate(node);

        return mr;
    }

    #endregion

    #region Debugging

    /// <summary>
    /// Returns a debug string representation of this pattern for diagnostic purposes.
    /// </summary>
    /// <returns>The string "*(expression)" indicating this is an unevaluated expression pattern.</returns>
    /// <remarks>
    /// This method is used by the debugger display attribute and diagnostic tools
    /// to provide a concise, human-readable representation of the pattern.
    /// The asterisk (*) operator in SNOBOL4 indicates deferred evaluation of a pattern expression.
    /// </remarks>
    public override string DebugPattern() => "*(expression)";

    #endregion
}
