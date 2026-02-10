using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class AbortPattern : TerminalPattern
{
    #region Methods

                            internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Abort", scan.Exec);
        return MatchResult.Abort(scan);
    }

                    internal override Pattern Clone() => new AbortPattern();

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "abort";

    #endregion
}