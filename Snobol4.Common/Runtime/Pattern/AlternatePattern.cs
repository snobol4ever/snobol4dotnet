using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class AlternatePattern : NonTerminalPattern
{
    #region Construction

                            internal AlternatePattern(Pattern leftPattern, Pattern rightPattern)
    {
        LeftPattern = leftPattern;
        RightPattern = rightPattern;
    }

    #endregion

    #region Methods

                        internal override Pattern Clone()
    {
        return new AlternatePattern(LeftPattern!, RightPattern!);
    }

    #endregion

    #region Debugging

                                                    public override string DebugPattern() => "alternate";

    #endregion
}