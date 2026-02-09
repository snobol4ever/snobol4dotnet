using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Implements the first phase of immediate variable assignment in SNOBOL4 pattern matching.
/// Records the starting cursor position before a pattern matches.
/// </summary>
/// <remarks>
/// <para>
/// Immediate variable assignment is similar to conditional assignment (.) but performs
/// the assignment immediately when the pattern matches, rather than deferring until
/// the entire pattern succeeds. This is typically used with the $ operator in SNOBOL4.
/// </para>
/// <para>
/// The immediate assignment mechanism uses a two-pattern structure:
/// <list type="number">
/// <item><description><b>ImmediateVariableAssociation1</b>: Records the pre-cursor position (start of match)</description></item>
/// <item><description><b>ImmediateVariableAssociation2</b>: Records post-cursor position and performs assignment</description></item>
/// </list>
/// </para>
/// <para>
/// Unlike conditional assignment which uses AlphaStack/BetaStack for deferred assignment,
/// immediate assignment performs the variable update as soon as the pattern matches,
/// sharing state directly between the two pattern phases.
/// </para>
/// <para>
/// Key difference from conditional assignment:
/// - Conditional (.): Assignment deferred until entire pattern succeeds
/// - Immediate ($): Assignment happens immediately when pattern matches
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // SNOBOL4 immediate assignment with $ operator
/// subject = 'test123data'
/// subject span(&amp;lcase) $ word span(&amp;digit) $ num
/// // word = "test" (assigned immediately)
/// // num = "123" (assigned immediately)
/// // Even if subsequent patterns fail, these assignments remain
///
/// // Compare with conditional assignment
/// subject = 'test123'
/// subject span(&amp;lcase) . word 'xyz'
/// // Pattern fails, word is NOT assigned
///
/// subject span(&amp;lcase) $ word 'xyz'
/// // Pattern fails, but word IS assigned "test" (immediate)
///
/// // Internal structure (C#)
/// // pattern $ variable becomes:
/// // ImmediateVariableAssociation1
/// // pattern
/// // ImmediateVariableAssociation2
/// </code>
/// </example>
[DebuggerDisplay("{DebugPattern()}")]
internal class ImmediateVariableAssociation1 : NullPattern
{
    #region Members

    /// <summary>
    /// Reference to the second phase pattern that will perform the actual assignment.
    /// Shared between phases to communicate the pre-cursor position.
    /// </summary>
    private readonly ImmediateVariableAssociation2 _va2;

    #endregion

    #region Construction

    /// <summary>
    /// Creates the first phase of an immediate variable assignment pattern.
    /// </summary>
    /// <param name="va2">
    /// The second phase pattern that will complete the assignment.
    /// This reference allows sharing the pre-cursor position between phases.
    /// </param>
    /// <remarks>
    /// The two-phase structure is necessary to capture the substring boundaries:
    /// - Phase 1 records where the pattern starts matching (pre-cursor)
    /// - Phase 2 records where the pattern ends matching (post-cursor)
    /// - Phase 2 then extracts and assigns the substring between these positions
    /// </remarks>
    internal ImmediateVariableAssociation1(ImmediateVariableAssociation2 va2)
    {
        _va2 = va2;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Records the starting cursor position for the immediate assignment.
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the cursor position</param>
    /// <returns>Always returns Success without advancing the cursor</returns>
    /// <remarks>
    /// <para>
    /// This pattern executes before the pattern being matched. It stores the current
    /// cursor position in the shared ImmediateVariableAssociation2 instance, which
    /// will use it later to calculate the matched substring boundaries.
    /// </para>
    /// <para>
    /// Unlike conditional assignment which uses a stack-based mechanism, immediate
    /// assignment uses direct field communication between phase 1 and phase 2.
    /// </para>
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("$1", scan.Exec);


        _va2.PreCursor = scan.CursorPosition;
        return MatchResult.Success(scan);
    }

    /// <summary>
    /// Creates a deep copy of this pattern.
    /// </summary>
    /// <returns>A new ImmediateVariableAssociation1 with the same phase 2 reference</returns>
    /// <remarks>
    /// The clone shares the same ImmediateVariableAssociation2 reference, which is
    /// intentional as they must communicate the pre-cursor position.
    /// </remarks>
    internal override Pattern Clone()
    {
        return new ImmediateVariableAssociation1(_va2);
    }

    #endregion
}

/// <summary>
/// Implements the second phase of immediate variable assignment in SNOBOL4 pattern matching.
/// Records the ending cursor position and performs the immediate assignment.
/// </summary>
/// <remarks>
/// <para>
/// This pattern completes the immediate assignment by:
/// 1. Using the pre-cursor position set by ImmediateVariableAssociation1
/// 2. Using the current cursor position as the post-cursor
/// 3. Extracting the substring between these positions
/// 4. Immediately assigning it to the variable
/// </para>
/// <para>
/// The assignment happens immediately and unconditionally. Unlike conditional assignment,
/// if a subsequent pattern in the overall match fails, this assignment is NOT rolled back.
/// The variable retains the assigned value.
/// </para>
/// <para>
/// This immediate behavior is useful for:
/// - Capturing partial matches even when overall pattern fails
/// - Side-effect assignments during pattern matching
/// - Debugging pattern matches by recording intermediate results
/// </para>
/// </remarks>
internal class ImmediateVariableAssociation2 : NullPattern
{
    #region Members

    /// <summary>
    /// The cursor position at the start of the matched substring.
    /// Set by ImmediateVariableAssociation1 during phase 1.
    /// </summary>
    internal int PreCursor;

    /// <summary>
    /// The variable that will receive the matched substring.
    /// </summary>
    internal Var Assignee;

    #endregion

    #region Construction

    /// <summary>
    /// Creates the second phase of an immediate variable assignment pattern.
    /// </summary>
    /// <param name="assignee">The variable to receive the matched substring</param>
    /// <remarks>
    /// The PreCursor field will be set later by ImmediateVariableAssociation1
    /// when it executes during pattern matching.
    /// </remarks>
    internal ImmediateVariableAssociation2(Var assignee)
    {
        Assignee = assignee;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep copy of this pattern.
    /// </summary>
    /// <returns>A new ImmediateVariableAssociation2 with the same assignee</returns>
    /// <remarks>
    /// The PreCursor field is not copied as it will be set dynamically during matching.
    /// </remarks>
    internal override Pattern Clone()
    {
        return new ImmediateVariableAssociation2(Assignee);
    }

    /// <summary>
    /// Extracts the matched substring and performs the immediate assignment.
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the subject string and current cursor position</param>
    /// <returns>Always returns Success without advancing the cursor</returns>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// 1. Extracts the substring from PreCursor (set by phase 1) to current cursor position
    /// 2. Creates an argument list with the assignee variable and the substring
    /// 3. Calls the executive's Assign method to perform the assignment
    /// 4. Returns Success without modifying the cursor position
    /// </para>
    /// <para>
    /// The assignment is immediate and permanent. Unlike conditional assignment (.),
    /// this assignment is NOT rolled back if subsequent patterns fail. Once this
    /// pattern executes, the variable contains the matched substring regardless of
    /// what happens later in the pattern match.
    /// </para>
    /// <para>
    /// The matched substring is: <c>scan.Subject[PreCursor..scan.CursorPosition]</c>
    /// This uses C# range syntax to extract the substring efficiently.
    /// </para>
    /// <para>
    /// This pattern doesn't consume input (cursor doesn't advance), it only performs
    /// a side-effect (assignment). This is similar to the @ operator but assigns a
    /// substring instead of a position.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Execution example:
    /// // subject = "test123"
    /// // PreCursor = 0 (set by phase 1)
    /// // Current cursor = 4 (after matching "test")
    /// // Extracted substring: subject[0..4] = "test"
    /// // Assignment: variable = "test"
    /// // Cursor remains at 4
    /// </code>
    /// </example>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("$2", scan.Exec);

        // Extract the matched substring using range syntax
        List<Var> arguments =
        [
            Assignee,
            new StringVar(scan.Subject[PreCursor..scan.CursorPosition])
        ];

        // Perform the immediate assignment
        scan.Exec.Assign(arguments);

        // Return success without advancing cursor
        var mr = MatchResult.Success(scan);

        return mr;
    }

    #endregion
}