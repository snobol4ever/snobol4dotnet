using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class NullPattern : LiteralPattern
{
    #region Construction

                internal NullPattern() : base("")
    {
    }

    #endregion

    #region Methods

                            internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Null", scan.Exec);

        return MatchResult.Success(scan);
    }

                    internal override Pattern Clone()
    {
        return new NullPattern();
    }

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "null";

    #endregion
}