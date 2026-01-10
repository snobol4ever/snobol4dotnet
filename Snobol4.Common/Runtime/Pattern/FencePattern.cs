// FENCE identifier is equivalent to the pattern NULL | ABORT.
// See JF Gimpel, Algorithms in SNOBOL4 on page 109
// The FENCE function is defined here a compound

namespace Snobol4.Common;

// ReSharper disable once UnusedMember.Global
internal class FencePattern : Pattern
{
    #region Methods

    // ReSharper disable once UnusedMember.Global
    internal static Pattern Structure(Pattern pattern)
    {
        return new AlternatePattern(pattern, new AbortPattern());
    }

    internal override Pattern Clone()
    {
        return Left == null ? throw new ApplicationException("Pattern.Clone") : new AlternatePattern(Left.Clone(), new AbortPattern());
    }

    #endregion
}