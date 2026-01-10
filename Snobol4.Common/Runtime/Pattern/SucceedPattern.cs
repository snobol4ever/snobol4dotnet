namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that always succeeds (like NULL) but is typically used with different semantics.
/// In SNOBOL4, this is the SUCCEED or &SUCCEED keyword.
/// </summary>
/// <remarks>
/// <para>
/// SUCCEED always succeeds without consuming input, similar to NULL. However, it's
/// semantically different: SUCCEED is typically used to force success in contexts
/// where you want to ensure a pattern branch succeeds regardless of other conditions.
/// </para>
/// <para>
/// The difference between SUCCEED and NULL is primarily semantic:
/// - NULL: Represents "no pattern" or empty match
/// - SUCCEED: Explicitly forces success, often used for control flow
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Always succeeds
/// subject = 'anything'
/// subject succeed                 // Success, cursor unchanged
///
/// // Force a pattern branch to succeed
/// pattern = complex_pattern | succeed
/// // If complex_pattern fails, SUCCEED ensures overall success
///
/// // Used in control flow patterns
/// pattern = (test1 test2) | succeed
/// // Try test1 and test2, but don't fail if they don't match
/// </code>
/// </example>
internal class SucceedPattern : TerminalPattern
{
    #region Methods

    /// <summary>
    /// Creates a deep copy of this SUCCEED pattern
    /// </summary>
    /// <returns>A new SucceedPattern instance</returns>
    internal override Pattern Clone()
    {
        return new SucceedPattern();
    }

    /// <summary>
    /// Always succeeds without advancing the cursor
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>Always returns Success without advancing cursor</returns>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        return MatchResult.Success(scan);
    }

    #endregion
}