using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class SucceedPattern : TerminalPattern
{
    #region Methods

                    internal override Pattern Clone()
    {
        return new SucceedPattern();
    }

                            internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Success", scan.Exec);

        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "succeed";

    #endregion
}