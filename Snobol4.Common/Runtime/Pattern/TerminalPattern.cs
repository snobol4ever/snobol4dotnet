namespace Snobol4.Common;

/// <summary>
/// Base class for terminal patterns that perform actual string matching.
/// Terminal patterns are leaf nodes in the pattern tree.
/// </summary>
internal abstract class TerminalPattern : Pattern
{
    /// <summary>
    /// Main scanner method that must be implemented for each terminal pattern.
    /// Attempts to match the pattern at the current cursor position.
    /// </summary>
    /// <param name="node">Index to the node being scanned in the AbstractSyntaxTree</param>
    /// <param name="scan">Scanner containing the subject string and cursor state</param>
    /// <returns>MatchResult indicating success, failure, or abort</returns>
    internal abstract MatchResult Scan(int node, Scanner scan);
}