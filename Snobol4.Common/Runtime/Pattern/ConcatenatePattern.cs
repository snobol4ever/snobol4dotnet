using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Class to encapsulate concatenation of patterns
/// </summary>
[DebuggerDisplay("{DebugString()}")]
internal class ConcatenatePattern : NonTerminalPattern
{
    #region Construction

    internal ConcatenatePattern(Pattern left, Pattern right)
    {
        Left = left;
        Right = right;
    }

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        if (Left == null || Right == null)
            throw new ApplicationException("Pattern.Clone");

        return new ConcatenatePattern(Left, Right);
    }

    #endregion

    #region Debugging

    internal string DebugString()
    {
        return $"{Left} & {Right}";
    }

    #endregion'
}