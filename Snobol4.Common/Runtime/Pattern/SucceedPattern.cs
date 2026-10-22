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

        // SUCCEED matches the null string, and again each time the match backs into it, so a pattern that fails after it tries it
        // again and never gives up (SPITBOL's SUCCEED does not): it is saved as its own alternate
        scan.SaveAlternate(node);
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "succeed";

    #endregion
}