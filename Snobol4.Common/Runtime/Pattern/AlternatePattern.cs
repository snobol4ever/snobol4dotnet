using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that tries alternative patterns, succeeding if either matches.
/// In SNOBOL4, alternation is expressed using the | operator.
/// </summary>
/// <remarks>
/// <para>
/// Alternation provides a way to specify multiple pattern choices. The left pattern
/// is tried first, and if it fails, the right pattern is tried. This enables
/// fallback matching and pattern variations.
/// </para>
/// <para>
/// Alternation is a composite (non-terminal) pattern with left and right children.
/// It implements backtracking: if the left pattern initially succeeds but the
/// overall match later fails, the system backtracks and tries the right pattern.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Match either singular or plural
/// subject = 'dogs'
/// subject ('dog' | 'dogs')        // Matches 'dogs'
///
/// // Multiple alternatives
/// vowel = ('a' | 'e' | 'i' | 'o' | 'u')
/// subject = 'hello'
/// subject 'h' vowel               // Matches 'he'
///
/// // With backtracking
/// subject = 'CATALOG FOR SEADOGS'
/// pattern = ('CAT' arb 'DOG') | ('DOG' arb 'CAT')
/// subject pattern                 // Matches first alternative
///
/// // Commonly used with ABORT to prevent backtracking
/// pattern = any('ab') | '1' abort
/// </code>
/// </example>
[DebuggerDisplay("{DebugString()}")]
internal class AlternatePattern : NonTerminalPattern
{
    #region Construction

    /// <summary>
    /// Creates a new alternation pattern
    /// </summary>
    /// <param name="left">The first pattern to try</param>
    /// <param name="right">The alternative pattern to try if left fails</param>
    /// <exception cref="ApplicationException">Thrown if either pattern is null</exception>
    internal AlternatePattern(Pattern left, Pattern right)
    {
        if (left == null || right == null)
            throw new ApplicationException("AlternatePattern");

        Left = left;
        Right = right;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep copy of this alternation pattern
    /// </summary>
    /// <returns>A new AlternatePattern with cloned left and right patterns</returns>
    /// <exception cref="ApplicationException">Thrown if left or right pattern is null (should never happen)</exception>
    internal override Pattern Clone()
    {
        if (Left == null || Right == null)
            throw new ApplicationException("Pattern.Clone");

        return new AlternatePattern(Left.Clone(), Right.Clone());
    }

    #endregion

    #region Debugging

    /// <summary>
    /// Returns a debug string representation of this alternation
    /// </summary>
    /// <returns>A string showing left | right pattern</returns>
    internal string DebugString()
    {
        return $"{Left} | {Right}";
    }

    #endregion
}