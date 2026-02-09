using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that matches the empty string (always succeeds without consuming input).
/// In SNOBOL4, this is the NULL keyword.
/// </summary>
/// <remarks>
/// <para>
/// NULL always succeeds immediately without advancing the cursor. It matches
/// zero characters and is primarily used as a building block for more complex
/// patterns or to create patterns that succeed without consuming input.
/// </para>
/// <para>
/// NULL is internally implemented as a special case of LiteralPattern with an empty string.
/// It's used in the internal structure of ARB and other complex patterns.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Always succeeds, matches nothing
/// subject = 'hello'
/// subject null                    // Success, cursor unchanged
///
/// // Used to create FENCE pattern
/// fence = null | abort            // Succeeds without consuming, or aborts
///
/// // Part of ARB structure (internal)
/// // ARB = NULL (NULL | ARB)
///
/// // Anchor pattern at specific position
/// subject = 'programmer'
/// subject 'pro' null 'gram'       // NULL is redundant but valid
/// </code>
/// </example>
[DebuggerDisplay("{DebugPattern()}")]
internal class NullPattern : LiteralPattern
{
    #region Construction

    /// <summary>
    /// Creates a new NULL pattern (matches empty string)
    /// </summary>
    internal NullPattern() : base("")
    {
    }

    #endregion

    #region Methods

    /// <summary>
    /// Matches the empty string (always succeeds without advancing cursor)
    /// </summary>
    /// <param name="node">Index of this pattern in the Abstract Syntax Tree</param>
    /// <param name="scan">Scanner containing the subject string and cursor state</param>
    /// <returns>Always returns Success without advancing the cursor</returns>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Null", scan.Exec);

        return MatchResult.Success(scan);
    }

    /// <summary>
    /// Creates a deep copy of this NULL pattern
    /// </summary>
    /// <returns>A new NullPattern instance</returns>
    internal override Pattern Clone()
    {
        return new NullPattern();
    }

    #endregion

    #region Debugging

    /// <summary>
    /// Returns a debug string representation of this pattern for diagnostic purposes.
    /// </summary>
    /// <returns>The string "null" indicating this is a NULL pattern.</returns>
    /// <remarks>
    /// This method is used by the debugger display attribute and diagnostic tools
    /// to provide a concise, human-readable representation of the pattern.
    /// </remarks>
    public override string DebugPattern() => "null";

    #endregion
}