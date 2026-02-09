using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that immediately aborts pattern matching, preventing backtracking.
/// In SNOBOL4, this is the ABORT or &ABORT keyword.
/// </summary>
/// <remarks>
/// <para>
/// ABORT is used to terminate pattern matching immediately when reached, with no
/// opportunity for backtracking to try alternative patterns. This is useful for
/// optimizing patterns when you know certain paths should not be explored.
/// </para>
/// <para>
/// The key difference between ABORT and FAIL:
/// - FAIL: Triggers backtracking to try alternatives
/// - ABORT: Terminates matching completely, no alternatives tried
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Prevent backtracking after matching specific pattern
/// subject = '-ab-1-'
/// pattern = any('ab') | '1' abort
/// subject pattern :f(n)          // First match succeeds on 'a'
/// subject pattern :s(y)          // Second attempt matches 'b'
///
/// // Without abort, might try '1' and backtrack
/// subject = '-1a-b-'
/// pattern = any('ab') | '1' abort
/// subject pattern :f(n)          // Matches '1', then aborts
/// subject pattern :s(y)          // No more matches possible - aborted
///
/// // Common use: optimize by preventing futile backtracking
/// pattern = expensive_pattern | quick_check abort
/// // If quick_check matches, don't waste time on expensive_pattern
/// </code>
/// </example>
[DebuggerDisplay("{DebugPattern()}")]
internal class AbortPattern : TerminalPattern
{
    #region Methods

    /// <summary>
    /// Immediately aborts pattern matching with no backtracking
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>Always returns Abort status</returns>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Abort", scan.Exec);
        return MatchResult.Abort(scan);
    }

    /// <summary>
    /// Creates a deep copy of this ABORT pattern
    /// </summary>
    /// <returns>A new AbortPattern instance</returns>
    internal override Pattern Clone() => new AbortPattern();

    #endregion

    #region Debugging

    /// <summary>
    /// Returns a debug string representation of this pattern for diagnostic purposes.
    /// </summary>
    /// <returns>The string "abort" indicating this is an ABORT pattern.</returns>
    /// <remarks>
    /// This method is used by the debugger display attribute and diagnostic tools
    /// to provide a concise, human-readable representation of the pattern.
    /// </remarks>
    public override string DebugPattern() => "abort";

    #endregion
}