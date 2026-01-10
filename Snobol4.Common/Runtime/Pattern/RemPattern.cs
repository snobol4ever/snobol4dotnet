namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that matches the remainder of the subject string.
/// In SNOBOL4, this is the REM or &REM keyword.
/// </summary>
/// <remarks>
/// <para>
/// REM matches all characters from the current cursor position to the end
/// of the subject string. It always succeeds (matching zero or more characters)
/// and advances the cursor to the end of the string.
/// </para>
/// <para>
/// REM is particularly useful for:
/// - Capturing everything after a specific pattern
/// - Ensuring the pattern consumes the entire remaining string
/// - Splitting strings at a delimiter
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Capture everything after a prefix
/// subject = 'programmer'
/// pattern = 'gra' rem . rest
/// subject pattern                // rest = "mmer"
///
/// // Match to end of string
/// subject = 'THE WINTER WINDS'
/// subject 'WIN' rem . captured   // captured = "TER WINDS"
///
/// // When pattern is at end, REM matches empty string
/// subject = 'THE WINTER WINDS'
/// subject 'WINDS' rem . captured // captured = ""
///
/// // Common pattern: split at delimiter
/// subject = 'NAME:VALUE'
/// subject break(':') . key ':' rem . value
/// // key = "NAME", value = "VALUE"
/// </code>
/// </example>
internal class RemPattern : TerminalPattern
{
    #region Internal Methods

    /// <summary>
    /// Creates a deep copy of this REM pattern
    /// </summary>
    /// <returns>A new RemPattern instance</returns>
    internal override Pattern Clone()
    {
        return new RemPattern();
    }

    /// <summary>
    /// Matches all remaining characters in the subject string
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor state</param>
    /// <returns>Always returns Success after advancing cursor to end of subject</returns>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        // Advance cursor to end of subject
        scan.CursorPosition = scan.Subject.Length;
        return MatchResult.Success(scan);
    }

    #endregion
}