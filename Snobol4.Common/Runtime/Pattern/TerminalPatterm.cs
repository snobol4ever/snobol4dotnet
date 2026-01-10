namespace Snobol4.Common;

internal abstract class TerminalPattern : Pattern
{
    /// <summary>
    /// Main scanner that needs to be written for each terminal.
    /// </summary>
    /// <param name="node">Index to the node being scanned in the AbstractSyntaxTree</param>
    /// <param name="scan">root of the AbstractSyntaxTree</param>
    /// <returns>MatchResult</returns>
    internal abstract MatchResult Scan(int node, Scanner scan);

}