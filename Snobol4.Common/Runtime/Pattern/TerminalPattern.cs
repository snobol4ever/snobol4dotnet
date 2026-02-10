namespace Snobol4.Common;

internal abstract class TerminalPattern : Pattern
{
                                internal abstract MatchResult Scan(int node, Scanner scan);
}