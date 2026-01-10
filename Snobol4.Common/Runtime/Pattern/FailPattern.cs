namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that always fails immediately.
/// In SNOBOL4, this is the FAIL or &FAIL keyword.
/// </summary>
/// <remarks>
/// <para>
/// FAIL is used to force pattern matching failure, which triggers backtracking
/// to try alternative patterns. It's commonly used in combination with alternation
/// to explore all possible matches systematically.
/// </para>
/// <para>
/// Unlike ABORT which terminates all backtracking, FAIL allows the pattern matcher
/// to try other alternatives if they exist.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Force failure to explore alternatives
/// subject = 'programmer'
/// pattern = fail                    // Always fails
/// subject pattern                   // Failure
///
/// // Used to iterate through all matches via backtracking
/// subject = '((A+(B*C))+D)'
/// pattern = bal . result fail       // Match each balanced substring
/// subject pattern                   // First match, then backtrack
///
/// // Common pattern: match then force retry
/// loop    subject pattern . x fail :f(done)
///         output = x              :(loop)
/// done    // Process all matches
/// </code>
/// </example>
internal class FailPattern : TerminalPattern
{
    #region Methods

    /// <summary>
    /// Creates a deep copy of this FAIL pattern
    /// </summary>
    /// <returns>A new FailPattern instance</returns>
    internal override FailPattern Clone()
    {
        return new FailPattern();
    }

    /// <summary>
    /// Always returns failure, triggering backtracking
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>Always returns Failure</returns>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        return MatchResult.Failure(scan);
    }

    #endregion
}