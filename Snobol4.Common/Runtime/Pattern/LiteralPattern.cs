    using System.Diagnostics;

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
[DebuggerDisplay("{DebugPattern()}")]
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
    /// <remarks>
    /// Uses ReadOnlySpan to avoid substring allocations, significantly improving performance
    /// for pattern matching operations.
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Literal", scan.Exec);

        var remainingLength = scan.Subject.Length - scan.CursorPosition;

        // Early exit if not enough characters remain
        if (remainingLength < Literal.Length)
            return MatchResult.Failure(scan);

        // Use ReadOnlySpan to avoid substring allocation
        var subjectSpan = scan.Subject.AsSpan(scan.CursorPosition, Literal.Length);

        // Use SequenceEqual for optimized comparison
        if (!subjectSpan.SequenceEqual(Literal.AsSpan()))
            return MatchResult.Failure(scan);

        // Advance cursor past the matched literal
        scan.CursorPosition += Literal.Length;
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

    /// <summary>
    /// Returns a debug string representation of this pattern for diagnostic purposes.
    /// </summary>
    /// <returns>A string in the format "literal (&lt;text&gt;)" where &lt;text&gt; is the literal string to match.</returns>
    /// <remarks>
    /// This method is used by the debugger display attribute and diagnostic tools
    /// to provide a concise, human-readable representation of the pattern.
    /// </remarks>
    public override string DebugPattern() => "literal";

    #endregion
}