using System;
using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class ArbPattern : TerminalPattern
{
    #region Methods

                        internal static Pattern Structure()
    {
        return new ConcatenatePattern(new NullPattern(), new AlternatePattern(new NullPattern(), new ArbPattern()));
    }

                    internal override Pattern Clone()
    {
        return new ArbPattern();
    }

                                                            internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Arb", scan.Exec);

        // Can't match beyond end of subject
        if (scan.CursorPosition == scan.Subject.Length)
            return MatchResult.Failure(scan);

        // Match one character and save alternate for longer match
        scan.CursorPosition++;
        scan.SaveAlternate(node);
        return MatchResult.Success(scan);
    }
    #endregion

    #region Debugging

                                    public override string DebugPattern() => "arb";

    #endregion

}