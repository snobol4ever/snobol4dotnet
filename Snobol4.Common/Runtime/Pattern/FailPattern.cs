using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class FailPattern : TerminalPattern
{
    #region Methods

                    internal override FailPattern Clone()
    {
        return new FailPattern();
    }

                            internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Failure", scan.Exec);

        return MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "fail";

    #endregion

}