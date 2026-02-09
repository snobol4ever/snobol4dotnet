using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Represents the cursor assignment operator (@) in SNOBOL4 pattern matching.
/// Assigns the current cursor position to a variable without consuming input.
/// </summary>
/// <remarks>
/// <para>
/// In SNOBOL4, the @ (at-sign) operator is used to capture the current cursor position
/// during pattern matching. The syntax is: <c>@variable</c>
/// </para>
/// <para>
/// The cursor position is an integer representing the number of characters from the
/// start of the subject string to the current position in the match. Position 0 is
/// at the beginning of the string.
/// </para>
/// <para>
/// Unlike conditional assignment (.), cursor assignment happens immediately and
/// unconditionally. The variable is assigned as soon as this pattern is encountered,
/// regardless of whether the overall pattern match succeeds or fails.
/// </para>
/// <para>
/// The @ operator is particularly useful for:
/// - Recording positions of matches within a string
/// - Calculating lengths of matched substrings (difference between two positions)
/// - Implementing position-dependent pattern logic
/// - Debugging pattern matches by tracking cursor movement
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Record cursor position at start
/// subject = 'Hello World'
/// subject @start 'World' @end
/// // start = 6, end = 11
/// // length = end - start = 5
///
/// // Track multiple positions
/// subject = 'one,two,three'
/// subject @p1 break(',') @p2 ',' @p3
/// // p1 = 0 (start)
/// // p2 = 3 (before comma)
/// // p3 = 4 (after comma)
///
/// // Use with POS to verify position
/// subject = 'test'
/// subject pos(0) @start 'test' rpos(0) @finish
/// // start = 0, finish = 4
///
/// // Calculate matched substring length
/// subject = 'programming'
/// subject 'gram' @p1 | 'prog' @p2
/// // If first alternative matches: p1 = 7
/// // If second matches: p2 = 4
///
/// // Position tracking in complex patterns
/// subject = '((A+B)*C)'
/// subject @before bal @after
/// // before = 0, after = 9 (entire string)
/// </code>
/// </example>
[DebuggerDisplay("{DebugPattern()}")]
internal class AtSign : TerminalPattern
{
    #region Members

    /// <summary>
    /// The variable that will receive the current cursor position.
    /// </summary>
    private readonly Var _assignee;

    /// <summary>
    /// The executive context that performs the assignment operation.
    /// </summary>
    private readonly Executive _exec;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new cursor assignment pattern that assigns the cursor position to a variable.
    /// </summary>
    /// <param name="assignee">The variable to receive the cursor position value</param>
    /// <param name="exec">The executive context for performing the assignment</param>
    /// <remarks>
    /// <para>
    /// The assignee variable will be assigned an integer value representing the cursor
    /// position (0-based index from the start of the subject string) when this pattern
    /// is executed during matching.
    /// </para>
    /// <para>
    /// The executive context is needed to perform the actual assignment operation,
    /// which may involve symbol table updates, validation, and other runtime checks.
    /// </para>
    /// </remarks>
    internal AtSign(Var assignee, Executive exec)
    {
        _assignee = assignee;
        _exec = exec;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep copy of this cursor assignment pattern.
    /// </summary>
    /// <returns>A new AtSign pattern with the same assignee and executive references</returns>
    /// <remarks>
    /// The clone shares the same assignee variable and executive context references,
    /// which is intentional as these are references to runtime objects that should
    /// be shared across pattern instances.
    /// </remarks>
    internal override Pattern Clone()
    {
        return new AtSign(_assignee, _exec);
    }

    /// <summary>
    /// Assigns the current cursor position to the variable and succeeds without advancing the cursor.
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and cursor position</param>
    /// <returns>Always returns Success without advancing the cursor</returns>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// 1. Creates an argument list with the assignee variable and current cursor position
    /// 2. Calls the executive's Assign method to perform the assignment
    /// 3. Pops the assignment result from the system stack (discarding it)
    /// 4. Returns Success without modifying the cursor position
    /// </para>
    /// <para>
    /// The assignment is immediate and unconditional. Unlike conditional assignment (.),
    /// this assignment is not deferred - the variable is updated immediately when this
    /// pattern is encountered, regardless of whether subsequent patterns succeed or fail.
    /// </para>
    /// <para>
    /// Since this pattern doesn't consume any input (cursor doesn't advance), it can
    /// be used multiple times at the same position to assign the same position value
    /// to different variables.
    /// </para>
    /// <para>
    /// The cursor position is a 0-based integer index:
    /// - Position 0: Beginning of string
    /// - Position N: After N characters
    /// - Position (string length): End of string
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Scan execution example
    /// // subject = "Hello"
    /// // cursor at position 2 (after "He")
    /// // @variable
    /// // Result: variable = 2, cursor remains at 2
    /// </code>
    /// </example>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("@", scan.Exec);

        // Create arguments for assignment: [variable, cursor_position]
        List<Var> arguments =
        [
            _assignee,
            new IntegerVar(scan.CursorPosition)
        ];

        // Perform the assignment using the executive's Assign method
        _exec.Assign(arguments);

        // Pop the assignment result from the stack (we don't need it)
        _exec.SystemStack.Pop();

        // Always succeed without advancing the cursor
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

    /// <summary>
    /// Returns a debug string representation of this pattern for diagnostic purposes.
    /// </summary>
    /// <returns>
    /// A string in the format "@ &lt;variable&gt;" showing the cursor assignment operation.
    /// </returns>
    /// <remarks>
    /// This method is used by the debugger display attribute and diagnostic tools
    /// to provide a concise, human-readable representation of the pattern.
    /// The @ symbol represents the cursor assignment operator in SNOBOL4.
    /// The assignee variable's DebugPattern() is included to show which variable receives the position.
    /// </remarks>
    public override string DebugPattern() => "@";

    #endregion

}