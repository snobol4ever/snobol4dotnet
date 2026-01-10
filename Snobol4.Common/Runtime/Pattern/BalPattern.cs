namespace Snobol4.Common;

/// <summary>
/// Represents a pattern that matches balanced parentheses.
/// In SNOBOL4, this is the BAL or &BAL keyword.
/// </summary>
/// <remarks>
/// <para>
/// BAL matches strings where parentheses are properly balanced, or strings
/// without parentheses. It uses a complex three-part structure internally to
/// handle backtracking and finding progressively longer balanced strings.
/// </para>
/// <para>
/// BAL matches:
/// 1. The shortest possible string without any parentheses
/// 2. Any string starting with '(' and ending with ')', where parentheses are balanced
/// 3. Any combination of the above
/// </para>
/// <para>
/// BAL is implemented as: NULL GBal1 GBal, where:
/// - NULL: Stores initial cursor position
/// - GBal1: NULL pattern for storing state
/// - GBal: The actual balancing logic with backtracking
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Extract all balanced substrings
/// subject = '((A+(B*C))+D)'
/// pattern = bal . result fail
/// subject pattern                 // Matches progressively:
///                                 // "((A+(B*C))+D)", "(A+(B*C))", "A", etc.
///
/// // Match function calls
/// subject = 'func(arg1, arg2)'
/// pattern = span(letters) . name '(' bal . args ')'
/// // name = "func", args = "arg1, arg2"
///
/// // Parse nested expressions
/// subject = '(x*(y+z))'
/// pattern = '(' bal . expr ')'    // expr = "x*(y+z)"
/// </code>
/// </example>
internal class BalPattern : Pattern
{
    #region Members

    internal NullPattern GBal0;
    internal GBal1Pattern GBal1;
    internal GBalPattern GBal;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new BAL pattern with its three-part structure
    /// </summary>
    internal BalPattern()
    {
        GBal0 = new NullPattern();
        GBal1 = new GBal1Pattern();
        GBal = new GBalPattern();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates the composite pattern structure for BAL
    /// </summary>
    /// <returns>A concatenation of NULL GBal1 GBal</returns>
    public static Pattern Structure()
    {
        var bal = new BalPattern();
        return new ConcatenatePattern(bal.GBal0, new ConcatenatePattern(bal.GBal1, bal.GBal));
    }

    /// <summary>
    /// Creates a deep copy of this BAL pattern
    /// </summary>
    /// <returns>A new BalPattern instance</returns>
    internal override Pattern Clone()
    {
        return new BalPattern();
    }

    #endregion

    #region Embedded Classes

    /// <summary>
    /// First pattern is a NULL pattern augmented to store the cursor position for
    /// failure of BAL.
    /// </summary>
    internal class GBal1Pattern : TerminalPattern
    {
        internal override MatchResult Scan(int node, Scanner scan)
        {
            return MatchResult.Success(scan);
        }

        internal override Pattern Clone()
        {
            return new GBal1Pattern();
        }

    }

    /// <summary>
    /// GBal pattern matches:
    ///      (1) the shortest possible string without any parentheses,
    ///      (2) any string that starts with a left parenthesis and
    ///          ends with a right parenthesis, where parentheses are matched.
    ///      (3) any combination of 1 and 2.
    /// </summary>
    internal class GBalPattern : TerminalPattern
    {
        internal override Pattern Clone()
        {
            return new GBalPattern();
        }

        /// <summary>
        /// Matches balanced parentheses or strings without parentheses
        /// </summary>
        /// <param name="node">The AST node index for this pattern</param>
        /// <param name="scan">The scanner containing the subject string and cursor state</param>
        /// <returns>
        /// Success if a balanced string is found (with backtracking support),
        /// Failure if at end of subject or next character is ')'
        /// </returns>
        /// <remarks>
        /// Uses ReadOnlySpan to avoid substring allocations during parenthesis matching,
        /// improving performance for balanced expression parsing.
        /// </remarks>
        internal override MatchResult Scan(int node, Scanner scan)
        {
            // Fail if there are no more characters to scan or
            // the next character is a closing parenthesis
            if (scan.CursorPosition == scan.Subject.Length ||
                scan.Subject[scan.CursorPosition] == ')')
                return MatchResult.Failure(scan);

            // If the next character in the subject is not a parenthesis, then the
            // match succeeds. This pattern is pushed. If a subsequent fails,
            // the Bal pattern can be extended.
            if (scan.Subject[scan.CursorPosition] != '(')
            {
                var mr = MatchResult.Success(scan);
                scan.CursorPosition++;
                scan.SaveAlternate(node);
                return mr;
            }

            // If the next character in the subject is a parenthesis, look for a balanced match
            // Use ReadOnlySpan to avoid substring allocation
            var subject = scan.Subject.AsSpan(scan.CursorPosition);
            var parenCount = 1;
            var pos = 1;

            while (pos < subject.Length && parenCount > 0)
            {
                switch (subject[pos])
                {
                    case ')':
                        parenCount--;
                        break;
                    case '(':
                        parenCount++;
                        break;
                }

                pos++;
            }

            // Fail if there are no more characters to scan and no balance was found.
            if (parenCount > 0)
                return MatchResult.Failure(scan);

            // If a balanced string was matched, succeed.
            // This pattern is pushed. If a subsequent fails,
            // the Bal pattern can be extended.
            scan.CursorPosition += pos;
            scan.SaveAlternate(node);
            return MatchResult.Success(scan);
        }
    }

    #endregion
}
