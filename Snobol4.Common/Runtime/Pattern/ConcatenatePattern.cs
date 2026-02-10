using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class ConcatenatePattern : NonTerminalPattern
{
    #region Construction

                                                                                                                                                internal ConcatenatePattern(Pattern left, Pattern right)
    {
        LeftPattern = left ?? throw new ArgumentNullException(nameof(left));
        RightPattern = right ?? throw new ArgumentNullException(nameof(right));
    }

    #endregion

    #region Methods

                                                                                                                                                                internal override Pattern Clone()
    {
        return new ConcatenatePattern(LeftPattern, RightPattern);
    }

    #endregion

    #region Debugging

                                                                public override string DebugPattern()
    {
        return "&";
    }

    #endregion
}