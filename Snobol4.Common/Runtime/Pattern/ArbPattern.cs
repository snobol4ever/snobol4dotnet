namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that matches an arbitrary string of any length.
/// In SNOBOL4, this is the ARB keyword.
/// </summary>
/// <remarks>
/// <para>
/// ARB is one of the most powerful and complex patterns in SNOBOL4. It matches
/// any sequence of characters (including the empty string) and uses backtracking
/// to find the shortest match that allows the overall pattern to succeed.
/// </para>
/// <para>
/// ARB is implemented internally as a complex structure involving alternation and
/// null patterns to enable proper backtracking behavior.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Match text between delimiters
/// subject = 'The [quick] brown fox'
/// subject '[' (arb . content) ']'  // content = "quick"
///
/// // Find shortest match
/// subject = 'MOUNTAIN'
/// subject 'O' (arb . x) 'A'        // x = "UNT" (shortest match)
/// subject 'O' (arb . y) 'U'        // y = "" (empty string is shortest)
///
/// // Greedy matching with backtracking
/// subject = 'CATDOG'
/// subject ('CAT' arb 'DOG') | ('DOG' arb 'CAT')  // Matches 'CATDOG'
/// </code>
/// </example>
internal class ArbPattern : TerminalPattern
{
    #region Methods

    /// <summary>
    /// Creates the composite pattern structure for ARB.
    /// ARB is implemented as: NULL (NULL | ARB) to enable backtracking.
    /// </summary>
    /// <returns>A composite pattern that implements ARB semantics</returns>
    internal static Pattern Structure()
    {
        return new ConcatenatePattern(new NullPattern(), new AlternatePattern(new NullPattern(), new ArbPattern()));
    }

    /// <summary>
    /// Creates a deep copy of this ARB pattern
    /// </summary>
    /// <returns>A new ArbPattern instance</returns>
    internal override Pattern Clone()
    {
        return new ArbPattern();
    }

    /// <summary>
    /// Attempts to match an arbitrary string (advances cursor by one character)
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>
    /// Success after advancing one character and saving an alternate match point,
    /// Failure if already at end of subject
    /// </returns>
    /// <remarks>
    /// The scan advances by one character and saves the current position as an
    /// alternate. This enables backtracking to try progressively longer matches
    /// if the initial (shortest) match fails.
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        // Can't match beyond end of subject
        if (scan.CursorPosition == scan.Subject.Length)
            return MatchResult.Failure(scan);

        // Match one character and save alternate for longer match
        scan.CursorPosition++;
        scan.SaveAlternate(node);
        return MatchResult.Success(scan);
    }
    #endregion
}