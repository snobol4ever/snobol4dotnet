using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that matches zero or more consecutive occurrences of another pattern.
/// In SNOBOL4, this is created using the ARBNO() function.
/// </summary>
/// <remarks>
/// <para>
/// ARBNO is one of SNOBOL4's most powerful pattern construction tools. It creates a pattern
/// that matches its argument pattern zero or more times consecutively. ARBNO always succeeds
/// (matching zero occurrences if necessary) and uses backtracking to try progressively more matches.
/// </para>
/// <para>
/// ARBNO implements a greedy matching strategy with backtracking:
/// 1. Initially matches zero occurrences (always succeeds)
/// 2. On backtracking, tries to match one occurrence
/// 3. Continues trying more occurrences until no more matches possible
/// </para>
/// <para>
/// Implementation: ARBNO is structured as NULL (NULL | ARBNO(pattern)) to enable proper
/// backtracking behavior. The alternation allows the matcher to try zero matches first,
/// then progressively more matches via backtracking.
/// </para>
/// <para>
/// Important: ARBNO differs from simple repetition because it participates in backtracking.
/// If a subsequent pattern fails, ARBNO can adjust how many times it matched its argument.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Match a list of comma-separated items
/// item = span('0123456789')
/// list = pos(0) '(' item arbno(',' item) ')' rpos(0)
/// subject = '(12,34,56)'
/// subject list                    // Success - arbno matches ',34' and ',56'
///
/// // Match zero occurrences
/// subject = '(12)'
/// subject list                    // Success - arbno matches zero times
///
/// // Match whitespace (zero or more spaces)
/// spaces = arbno(' ')
/// subject = 'hello   world'
/// subject 'hello' spaces 'world'  // Success - arbno matches three spaces
///
/// // Match repeated pattern with backtracking
/// subject = 'aaabbb'
/// pattern = arbno('a') arbno('b')
/// subject pattern                 // Success - first arbno matches 'aaa'
///
/// // Common pattern: optional sign followed by digits
/// digits = '0123456789'
/// integer = arbno(any('+-')) span(digits)
/// subject = '-42'
/// subject integer . num           // num = "-42"
///
/// // Parse variable-length input
/// letter = any('abcdefghijklmnopqrstuvwxyz')
/// word = span(letter)
/// sentence = word arbno(' ' word)
/// subject = 'the quick brown fox'
/// subject sentence                // Matches entire sentence
///
/// // ARBNO with complex patterns
/// subject = 'test1test2test3'
/// pattern = arbno('test' any('0123456789'))
/// subject pattern                 // Matches all three occurrences
/// </code>
/// </example>
[DebuggerDisplay("{DebugPattern()}")]
internal class ArbNoPattern : TerminalPattern
{
    #region Members

    /// <summary>
    /// The pattern to match zero or more times
    /// </summary>
    private readonly Pattern _arbPattern;

    #endregion

    #region Construction

    /// <summary>
    /// Creates an ARBNO pattern that matches the specified pattern zero or more times
    /// </summary>
    /// <param name="arbPattern">The pattern to repeat. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if arbPattern is null</exception>
    internal ArbNoPattern(Pattern arbPattern)
    {
        _arbPattern = arbPattern ?? throw new ArgumentNullException(nameof(arbPattern));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates the composite pattern structure for ARBNO.
    /// Implements as: NULL (NULL | ARBNO(pattern)) to enable backtracking.
    /// </summary>
    /// <param name="arbPattern">The pattern to repeat</param>
    /// <returns>
    /// A composite pattern that matches zero or more occurrences of arbPattern
    /// with proper backtracking support
    /// </returns>
    /// <remarks>
    /// The structure ensures that:
    /// - Initially matches zero occurrences (via the first NULL)
    /// - Alternation provides backtracking opportunity
    /// - Second NULL allows zero-match exit
    /// - ARBNO(pattern) allows progressive matching via recursion
    /// </remarks>
    internal static Pattern Structure(Pattern arbPattern)
    {
        return new ConcatenatePattern(
            new NullPattern(), 
            new AlternatePattern(
                new NullPattern(), 
                new ArbNoPattern(arbPattern)));
    }

    /// <summary>
    /// Creates a deep copy of this ARBNO pattern
    /// </summary>
    /// <returns>A new ArbNoPattern with a cloned child pattern</returns>
    internal override Pattern Clone()
    {
        return new ArbNoPattern(_arbPattern.Clone());
    }

    /// <summary>
    /// Attempts to match one occurrence of the child pattern
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>
    /// Success if the child pattern matches (advances cursor and saves alternate),
    /// Failure or Abort if the child pattern fails or aborts
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is called for each repetition attempt. The backtracking mechanism
    /// is handled by the alternation structure created in Structure().
    /// </para>
    /// <para>
    /// The scan process:
    /// 1. Creates a new scanner for the child pattern match
    /// 2. Attempts to match the child pattern against remaining subject
    /// 3. If successful, advances cursor and saves alternate for more repetitions
    /// 4. If failed or aborted, returns the failure/abort status
    /// </para>
    /// <para>
    /// Saving an alternate enables backtracking: if a subsequent pattern fails,
    /// the matcher can return here and try fewer ARBNO repetitions.
    /// </para>
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Arbno", scan.Exec);
        
        // Create a fresh scanner for matching the child pattern
        var reScan = new Scanner(scan.Exec);

        // Try to match the child pattern against the remaining subject
        // Use anchor=true to match at the current position only
        var mr = reScan.PatternMatch(
            scan.Subject[scan.CursorPosition..], 
            _arbPattern, 
            0, 
            true);

        // If child pattern failed or aborted, propagate the result
        if (mr.Outcome is MatchResult.Status.FAILURE or MatchResult.Status.ABORT)
            return mr;

        // Child pattern succeeded - advance cursor past the matched text
        scan.CursorPosition += mr.PostCursor;
        
        // Save alternate to enable backtracking for fewer repetitions
        // This allows trying N-1 repetitions if N repetitions cause later failure
        scan.SaveAlternate(node);
        
        return MatchResult.Success(scan);
    }
    #endregion

    #region Debugging


   /// <summary>
    /// Returns a debug string representation of this pattern for diagnostic purposes.
    /// </summary>
    /// <returns>A string in the format "arbno(&lt;pattern&gt;)" showing the repeated pattern structure.</returns>
    /// <remarks>
    /// This method is used by the debugger display attribute and diagnostic tools
    /// to provide a concise, human-readable representation of the pattern.
    /// The nested pattern's DebugPattern() is recursively included to show the complete structure.
    /// </remarks>
    public override string DebugPattern() => "arbno";

    #endregion
}