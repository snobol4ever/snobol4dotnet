using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class RemPattern : TerminalPattern
{
    #region Internal Methods

                    internal override Pattern Clone()
    {
        return new RemPattern();
    }

                            internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Rem", scan.Exec);

        // Advance cursor to end of subject
        scan.CursorPosition = scan.Subject.Length;
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "rem";

    #endregion
}