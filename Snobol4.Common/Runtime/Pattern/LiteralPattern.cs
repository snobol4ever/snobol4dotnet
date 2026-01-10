namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that matches a specific literal string.
/// In SNOBOL4, literal patterns are created by enclosing text in quotes.
/// </summary>
/// <remarks>
/// This is the simplest and most common pattern type. It performs an exact
/// string comparison at the current cursor position during pattern matching.
/// The match succeeds only if the subject string contains the literal string
/// at the current position.
/// </remarks>
/// <example>
/// <code>
/// // Matches 'gram' in 'programmer'
/// subject = 'programmer'
/// subject 'gram'              // Success, matches at position 3
/// 
/// // Case-sensitive matching
/// subject = 'SNOBOL'
/// subject 'snobol'            // Fails - case doesn't match
/// 
/// // Can be used in concatenation
/// subject = 'hello world'
/// subject 'hello' ' ' 'world' // Success
/// </code>
/// </example>
internal class LiteralPattern : TerminalPattern
{
    #region Members

    /// <summary>
    /// The literal string to match against the subject
    /// </summary>
    internal string Literal;

    #endregion

    #region Construction

    /// <summary>
    /// Creates a new literal pattern that matches a specific string
    /// </summary>
    /// <param name="literal">The string to match. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when literal is null</exception>
    internal LiteralPattern(string literal)
    {
        Literal = literal ?? throw new ArgumentNullException(nameof(literal));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep copy of this literal pattern
    /// </summary>
    /// <returns>A new LiteralPattern with the same literal string</returns>
    internal override Pattern Clone()
    {
        return new LiteralPattern(Literal);
    }

    /// <summary>
    /// Attempts to match the literal string at the current cursor position
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>
    /// Success if the literal matches at the current position,
    /// Failure otherwise
    /// </returns>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        // Check if the remaining subject starts with our literal
        if (!scan.Subject[scan.CursorPosition..].StartsWith(Literal))
            return MatchResult.Failure(scan);

        // Advance cursor past the matched literal
        scan.CursorPosition += Literal.Length;
        return MatchResult.Success(scan);
    }

    #endregion
}