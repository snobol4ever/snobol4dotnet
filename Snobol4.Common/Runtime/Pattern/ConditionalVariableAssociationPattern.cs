namespace Snobol4.Common;

/// <summary>
/// Implements the first phase of conditional variable assignment in SNOBOL4 pattern matching.
/// This pattern records the starting cursor position when entering a conditional assignment.
/// </summary>
/// <remarks>
/// <para>
/// In SNOBOL4, the conditional assignment operator (.) associates a variable with a pattern,
/// assigning the matched substring to the variable only if the entire pattern succeeds.
/// For example: <c>pattern . variable</c>
/// </para>
/// <para>
/// The conditional assignment mechanism uses a four-pattern structure for proper backtracking:
/// <list type="number">
/// <item><description><b>ConditionalVariableAssociation1</b>: Records pre-cursor position (entry point)</description></item>
/// <item><description><b>ConditionalVariableAssociationBackup1</b>: Cleanup on backtrack before pattern matches</description></item>
/// <item><description><b>ConditionalVariableAssociation2</b>: Records post-cursor position (exit point)</description></item>
/// <item><description><b>ConditionalVariableAssociationBackup2</b>: Cleanup on backtrack after pattern matches</description></item>
/// </list>
/// </para>
/// <para>
/// This implementation follows the algorithm described in:
/// Gimpel, J.F. "Algorithms in SNOBOL4." John Wiley &amp; Sons, 1976. pp. 135-136.
/// </para>
/// <para>
/// The pattern uses two stacks (AlphaStack and BetaStack) to track assignment boundaries
/// during matching and backtracking. The AlphaStack holds pending assignments (before pattern matches),
/// while BetaStack holds completed assignments (after pattern matches).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // SNOBOL4 conditional assignment
/// subject = 'programmer'
/// subject 'pro' . prefix 'gram' . middle 'mer'
/// // If entire pattern succeeds:
/// //   prefix = "pro"
/// //   middle = "gram"
/// // If pattern fails, variables are unchanged
/// 
/// // With alternation and backtracking
/// subject = 'test123'
/// subject (span('a-z') . word | span('0-9') . num) span('0-9') . digits
/// // Tries first alternative, if it fails, backtracks and tries second
/// 
/// // Internal structure (C#)
/// // pattern . variable becomes:
/// // (ConditionalVariableAssociation1 | ConditionalVariableAssociationBackup1)
/// // pattern
/// // (ConditionalVariableAssociation2 | ConditionalVariableAssociationBackup2)
/// </code>
/// </example>
internal class ConditionalVariableAssociation1 : NullPattern
{
    #region Members

    /// <summary>
    /// The variable that will receive the matched substring if the pattern succeeds.
    /// </summary>
    internal Var Assignee;

    /// <summary>
    /// The executive context that maintains the AlphaStack and BetaStack for tracking assignments.
    /// </summary>
    internal Executive Exec;

    #endregion

    #region Construction

    /// <summary>
    /// Creates the first phase of a conditional variable assignment pattern.
    /// </summary>
    /// <param name="assignee">The variable to receive the matched substring</param>
    /// <param name="exec">The executive context for managing assignment stacks</param>
    /// <remarks>
    /// This pattern is inserted before the pattern being matched to record the starting
    /// cursor position for the conditional assignment.
    /// </remarks>
    internal ConditionalVariableAssociation1(Var assignee, Executive exec)
    {
        Assignee = assignee;
        Exec = exec;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a deep copy of this pattern.
    /// </summary>
    /// <returns>A new ConditionalVariableAssociation1 with the same assignee and executive</returns>
    internal override Pattern Clone()
    {
        return new ConditionalVariableAssociation1(Assignee, Exec);
    }

    /// <summary>
    /// Records the starting cursor position for the conditional assignment.
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing cursor position and subject</param>
    /// <returns>Always returns Success without advancing the cursor</returns>
    /// <remarks>
    /// <para>
    /// This pattern pushes a NameListEntry onto the AlphaStack containing:
    /// - The assignee variable
    /// - The current cursor position (pre-cursor, start of match)
    /// - Placeholder -1 for post-cursor (set later by ConditionalVariableAssociation2)
    /// - The scanner reference
    /// </para>
    /// <para>
    /// The entry remains on AlphaStack until either:
    /// - The pattern succeeds and ConditionalVariableAssociation2 moves it to BetaStack
    /// - The pattern fails and ConditionalVariableAssociationBackup1 removes it
    /// </para>
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4(".1", scan.Exec);
        
        Exec.AlphaStack.Push(new NameListEntry(Assignee, scan.CursorPosition, -1, scan));
        return MatchResult.Success(scan);
    }

    #endregion
}

/// <summary>
/// Implements the cleanup mechanism for conditional assignment when backtracking occurs
/// before the pattern has been matched.
/// </summary>
/// <remarks>
/// <para>
/// This pattern is invoked during backtracking when the pattern matcher needs to
/// undo a conditional assignment that was started (ConditionalVariableAssociation1)
/// but the associated pattern never successfully matched.
/// </para>
/// <para>
/// This is part of the four-pattern structure for conditional assignment, ensuring
/// proper cleanup during pattern matching with backtracking.
/// </para>
/// </remarks>
internal class ConditionalVariableAssociationBackup1 : NullPattern
{
    /// <summary>
    /// The variable that was going to receive the matched substring.
    /// </summary>
    internal Var Assignee;

    /// <summary>
    /// The executive context containing the AlphaStack to clean up.
    /// </summary>
    internal Executive Exec;

    /// <summary>
    /// Creates a deep copy of this backup pattern.
    /// </summary>
    /// <returns>A new ConditionalVariableAssociationBackup1 with the same assignee and executive</returns>
    internal override Pattern Clone()
    {
        return new ConditionalVariableAssociationBackup1(Assignee, Exec);
    }

    /// <summary>
    /// Creates the backup cleanup pattern for the first phase of conditional assignment.
    /// </summary>
    /// <param name="assignee">The variable associated with this assignment</param>
    /// <param name="exec">The executive context for managing assignment stacks</param>
    internal ConditionalVariableAssociationBackup1(Var assignee, Executive exec)
    {
        Assignee = assignee;
        Exec = exec;
    }

    /// <summary>
    /// Removes the pending assignment from the AlphaStack and triggers backtracking.
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing cursor state</param>
    /// <returns>Always returns Failure to trigger backtracking</returns>
    /// <remarks>
    /// <para>
    /// This pattern is reached via backtracking when the pattern associated with the
    /// conditional assignment fails to match. It pops the NameListEntry that was pushed
    /// by ConditionalVariableAssociation1, removing the pending assignment.
    /// </para>
    /// <para>
    /// By returning Failure, this pattern ensures that backtracking continues to
    /// find alternative matches.
    /// </para>
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4(".2", scan.Exec);
        Exec.AlphaStack.Pop();
        return MatchResult.Failure(scan);
    }
}

/// <summary>
/// Implements the second phase of conditional variable assignment, recording the
/// ending cursor position after the pattern has matched.
/// </summary>
/// <remarks>
/// <para>
/// This pattern is executed after the associated pattern successfully matches.
/// It moves the assignment record from AlphaStack (pending) to BetaStack (completed),
/// updating the post-cursor position to define the matched substring boundaries.
/// </para>
/// <para>
/// The assignment is not actually performed yet - that happens later when the entire
/// pattern match completes successfully. This deferred assignment ensures that variables
/// are only updated if the complete pattern succeeds.
/// </para>
/// </remarks>
internal class ConditionalVariableAssociation2 : NullPattern
{
    /// <summary>
    /// The variable that will receive the matched substring.
    /// </summary>
    internal Var Assignee;

    /// <summary>
    /// The executive context managing the AlphaStack and BetaStack.
    /// </summary>
    internal Executive Exec;

    /// <summary>
    /// Creates a deep copy of this pattern.
    /// </summary>
    /// <returns>A new ConditionalVariableAssociation2 with the same assignee and executive</returns>
    internal override Pattern Clone()
    {
        return new ConditionalVariableAssociation2(Assignee, Exec);
    }

    /// <summary>
    /// Creates the second phase of a conditional variable assignment pattern.
    /// </summary>
    /// <param name="assignee">The variable to receive the matched substring</param>
    /// <param name="exec">The executive context for managing assignment stacks</param>
    /// <remarks>
    /// This pattern is inserted after the pattern being matched to record the ending
    /// cursor position for the conditional assignment.
    /// </remarks>
    internal ConditionalVariableAssociation2(Var assignee, Executive exec)
    {
        Assignee = assignee;
        Exec = exec;
    }

    /// <summary>
    /// Records the ending cursor position and moves the assignment to the BetaStack.
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing the current cursor position</param>
    /// <returns>
    /// Success if the AlphaStack entry exists (pattern matched),
    /// Failure if AlphaStack is empty (should not happen in normal operation)
    /// </returns>
    /// <remarks>
    /// <para>
    /// This pattern:
    /// 1. Pops the NameListEntry from AlphaStack (pushed by ConditionalVariableAssociation1)
    /// 2. Updates its PostCursor field with the current cursor position
    /// 3. Pushes the updated entry onto BetaStack (completed assignments)
    /// </para>
    /// <para>
    /// The NameListEntry now contains complete information:
    /// - PreCursor: Position before the pattern matched
    /// - PostCursor: Position after the pattern matched
    /// - The substring between these positions is what gets assigned
    /// </para>
    /// <para>
    /// If the entire pattern match eventually fails, ConditionalVariableAssociationBackup2
    /// will undo this by moving the entry back from BetaStack to AlphaStack.
    /// </para>
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4(".3", scan.Exec);
   
        if (Exec.AlphaStack.Count == 0)
        {
            return MatchResult.Failure(scan);
        }
        var nameEntry = Exec.AlphaStack.Pop();
        nameEntry.PostCursor = scan.CursorPosition;
        Exec.BetaStack.Push(nameEntry);
        return MatchResult.Success(scan);
    }
}

/// <summary>
/// Implements the cleanup mechanism for conditional assignment when backtracking occurs
/// after the pattern has been matched.
/// </summary>
/// <remarks>
/// <para>
/// This pattern is invoked during backtracking when the pattern matcher needs to undo
/// a conditional assignment that was completed (ConditionalVariableAssociation2 executed)
/// but a subsequent pattern in the overall match failed.
/// </para>
/// <para>
/// This ensures that if backtracking occurs, the assignment boundaries are restored to
/// their previous state, allowing alternative matches to be tried.
/// </para>
/// </remarks>
internal class ConditionalVariableAssociationBackup2 : NullPattern
{
    /// <summary>
    /// The executive context containing the BetaStack to restore from.
    /// </summary>
    internal Executive Exec;

    /// <summary>
    /// The variable associated with this assignment.
    /// </summary>
    internal Var Assignee;

    /// <summary>
    /// Creates a deep copy of this backup pattern.
    /// </summary>
    /// <returns>A new ConditionalVariableAssociationBackup2 with the same assignee and executive</returns>
    internal override Pattern Clone()
    {
        return new ConditionalVariableAssociationBackup2(Assignee, Exec);
    }

    /// <summary>
    /// Creates the backup cleanup pattern for the second phase of conditional assignment.
    /// </summary>
    /// <param name="assignee">The variable associated with this assignment</param>
    /// <param name="exec">The executive context for managing assignment stacks</param>
    internal ConditionalVariableAssociationBackup2(Var assignee, Executive exec)
    {
        Exec = exec;
        Assignee = assignee;
    }

    /// <summary>
    /// Restores the assignment record from BetaStack to AlphaStack and triggers backtracking.
    /// </summary>
    /// <param name="node">The AST node index for this pattern</param>
    /// <param name="scan">The scanner containing cursor state</param>
    /// <returns>Always returns Failure to trigger backtracking</returns>
    /// <remarks>
    /// <para>
    /// This pattern is reached via backtracking when a pattern following the conditional
    /// assignment fails. It:
    /// 1. Pops the NameListEntry from BetaStack (pushed by ConditionalVariableAssociation2)
    /// 2. Resets its PostCursor to 0 (invalidating the end boundary)
    /// 3. Pushes it back onto AlphaStack (returning it to pending state)
    /// 4. Returns Failure to continue backtracking
    /// </para>
    /// <para>
    /// This restoration allows the pattern matcher to try alternative matches for the
    /// pattern associated with this conditional assignment.
    /// </para>
    /// </remarks>
    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4(".4", scan.Exec);

        if (Exec.BetaStack.Count == 0)
        {
            return MatchResult.Failure(scan);
        }
        var nameListEntry = Exec.BetaStack.Pop();
        nameListEntry.PostCursor = 0;
        Exec.AlphaStack.Push(nameListEntry);
        return MatchResult.Failure(scan);
    }
}

/// <summary>
/// Represents a pending or completed conditional variable assignment during pattern matching.
/// </summary>
/// <remarks>
/// <para>
/// NameListEntry tracks the boundaries of a matched substring that will be assigned to
/// a variable if the entire pattern match succeeds. It moves between AlphaStack (pending)
/// and BetaStack (completed) as the pattern match progresses and backtracks.
/// </para>
/// <para>
/// The entry contains all information needed to perform the assignment:
/// - Which variable to assign to (Assignee)
/// - Where the match started (PreCursor)
/// - Where the match ended (PostCursor)
/// - The scanner containing the subject string (Scan)
/// </para>
/// </remarks>
internal class NameListEntry
{
    /// <summary>
    /// The variable that will receive the matched substring.
    /// </summary>
    internal Var Assignee;

    /// <summary>
    /// The cursor position at the start of the matched substring.
    /// Set by ConditionalVariableAssociation1.
    /// </summary>
    internal int PreCursor;

    /// <summary>
    /// The cursor position at the end of the matched substring.
    /// Set by ConditionalVariableAssociation2, or -1 if not yet set.
    /// </summary>
    internal int PostCursor;

    /// <summary>
    /// The scanner containing the subject string and cursor state.
    /// Used to extract the matched substring for assignment.
    /// </summary>
    internal Scanner Scan;

    /// <summary>
    /// Creates a new name list entry for tracking a conditional assignment.
    /// </summary>
    /// <param name="assignee">The variable to receive the matched substring</param>
    /// <param name="pre">The starting cursor position (before the pattern)</param>
    /// <param name="post">The ending cursor position (after the pattern), or -1 if not yet determined</param>
    /// <param name="scan">The scanner containing the subject string</param>
    /// <remarks>
    /// <para>
    /// Initially created with post = -1 by ConditionalVariableAssociation1.
    /// The PostCursor is later updated by ConditionalVariableAssociation2 when the
    /// pattern successfully matches.
    /// </para>
    /// <para>
    /// The matched substring is: Scan.Subject[PreCursor..PostCursor]
    /// </para>
    /// </remarks>
    internal NameListEntry(Var assignee, int pre, int post, Scanner scan)
    {
        Assignee = assignee;
        PreCursor = pre;
        PostCursor = post;
        Scan = scan;
    }
}