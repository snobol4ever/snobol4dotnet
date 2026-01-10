namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that advances the cursor to a specific position.
/// In SNOBOL4, this is created using the TAB() function.
/// </summary>
/// <remarks>
/// <para>
/// TAB advances the cursor to a specific 0-based position in the subject string,
/// matching all characters between the current position and the target position.
/// TAB can only move forward - if the cursor is already at or past the target
/// position, the pattern fails.
/// </para>
/// <para>
/// Position interpretation:
/// - TAB(0): Fail if not already at beginning (can't move backward)
/// - TAB(n): Advance to position n (after n characters)
/// - TAB(length): Advance to end of string
/// </para>
/// <para>
/// TAB is commonly used for:
/// - Skipping fixed-width fields
/// - Advancing to specific positions in formatted text
/// - Fixed-position parsing (column-based data)
/// - Moving cursor forward by calculated amount
/// </para>
/// <para>
/// Unlike POS which only checks position, TAB actually moves the cursor forward,
/// consuming the characters between current and target position.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Skip to position
/// subject = 'test data here'
/// subject tab(5) rem . rest     // rest = "data here", skips "test "
///
/// // Fixed-width field parsing
/// subject = 'John    Smith   42'
/// subject tab(0) len(8) . first
///         tab(8) len(8) . last
///         tab(16) rem . age
/// // first = "John    ", last = "Smith   ", age = "42"
///
/// // Conditional advancement
/// subject = 'prefix-data'
/// subject 'prefix' tab(10) rem . rest
/// // Advances to position 10 if cursor is before it
///
/// // Will fail if already past position
/// subject = 'test'
/// subject 'test' tab(2)         // Fails, cursor at 4, can't go back to 2
///
/// // TAB to end
/// subject = 'data'
/// subject tab(4)                // Advances to end (position 4)
/// </code>
/// </example>
internal class TabPattern : TerminalPattern
{
    #region Members

    /// <summary>
    /// The target cursor position (0-based index from start of subject).
    /// </summary>
    internal int Position;

    #endregion

    #region Construction

    /// <summary>
    /// Creates a TAB pattern that advances to a specific position.
    /// </summary>
    /// <param name="position">The target 0-based position in the subject string</param>
    /// <remarks>
    /// Position 0 is the beginning of the string, position n is after n characters.
    /// If position exceeds string length, the pattern will fail during matching.
    /// </remarks>
    internal TabPattern(int position)
    {
        Position = position;
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Creates a deep copy of this TAB pattern.
    /// </summary>
    /// <returns>A new TabPattern with the same target position</returns>
    internal override Pattern Clone()
    {
        return new TabPattern(Position);
    }

    /// <summary>
    /// Advances the cursor to the target position if possible.
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the cursor position and subject string</param>
    /// <returns>
    /// Success if cursor can advance to target position (cursor moves to Position),
    /// Failure if cursor is already past target or if target exceeds subject length
    /// </returns>
    /// <remarks>
    /// <para>
    /// TAB can only move forward. The pattern fails if:
    /// - Cursor is already past the target position (can't move backward)
    /// - Target position exceeds the subject string length
    /// </para>
    /// <para>
    /// On success, the cursor is set to the target position, effectively consuming
    /// all characters between the old cursor position and the target position.
    /// </para>
    /// <para>
    /// TAB(current_position) always succeeds trivially without moving the cursor.
    /// </para>
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        // Single check: fail if cursor already past target or target exceeds subject length
        if (scan.CursorPosition > Position || Position > scan.Subject.Length)
            return MatchResult.Failure(scan);

        // Advance cursor to target position
        scan.CursorPosition = Position;
        return MatchResult.Success(scan);
    }

    #endregion
}