namespace Snobol4.Common;

/// <summary>
/// Represents the result of a pattern matching operation.
/// </summary>
/// <remarks>
/// <para>
/// MatchResult encapsulates three key pieces of information:
/// 1. The outcome (SUCCESS, FAILURE, or ABORT)
/// 2. The cursor position before the match (PreCursor)
/// 3. The cursor position after the match (PostCursor)
/// </para>
/// <para>
/// This information is used by the pattern matching engine to:
/// - Determine if a pattern matched successfully
/// - Calculate the length of the matched substring
/// - Support backtracking by restoring cursor positions
/// - Handle ABORT conditions that prevent backtracking
/// </para>
/// </remarks>
public class MatchResult
{
    /// <summary>
    /// The cursor position before the match attempt
    /// </summary>
    internal int PreCursor { get; }
    
    /// <summary>
    /// The cursor position after the match attempt
    /// </summary>
    internal int PostCursor { get; }
    
    /// <summary>
    /// The outcome of the match attempt
    /// </summary>
    internal Status Outcome { get; }

    /// <summary>
    /// Possible outcomes of a pattern match operation
    /// </summary>
    public enum Status 
    { 
        /// <summary>Pattern matched successfully</summary>
        SUCCESS, 
        
        /// <summary>Pattern failed to match (allows backtracking)</summary>
        FAILURE, 
        
        /// <summary>Pattern aborted (prevents backtracking)</summary>
        ABORT 
    }

    /// <summary>
    /// Gets whether the match was successful
    /// </summary>
    public bool IsSuccess => Outcome == Status.SUCCESS;
    
    /// <summary>
    /// Gets whether the match failed
    /// </summary>
    public bool IsFailure => Outcome == Status.FAILURE;
    
    /// <summary>
    /// Gets whether the match was aborted
    /// </summary>
    public bool IsAbort => Outcome == Status.ABORT;

    /// <summary>
    /// Gets the length of the matched substring
    /// </summary>
    /// <remarks>
    /// For successful matches, this is the number of characters consumed.
    /// For failures and aborts, this is typically 0.
    /// </remarks>
    public int MatchLength => PostCursor - PreCursor;

    #region Factory Methods

    /// <summary>
    /// Creates a failure result from a scanner
    /// </summary>
    /// <param name="scan">The scanner at the failure point</param>
    /// <returns>A MatchResult indicating failure</returns>
    internal static MatchResult Failure(Scanner scan)
        => new(scan.CursorPosition, scan.CursorPosition, Status.FAILURE);

    /// <summary>
    /// Creates a failure result from scanner state
    /// </summary>
    /// <param name="state">The scanner state at the failure point</param>
    /// <returns>A MatchResult indicating failure</returns>
    internal static MatchResult Failure(ScannerState state)
        => new(state.CursorPosition, state.CursorPosition, Status.FAILURE);

    /// <summary>
    /// Creates an abort result from a scanner
    /// </summary>
    /// <param name="scan">The scanner at the abort point</param>
    /// <returns>A MatchResult indicating abort</returns>
    internal static MatchResult Abort(Scanner scan)
        => new(scan.CursorPosition, scan.CursorPosition, Status.ABORT);

    /// <summary>
    /// Creates an abort result from scanner state
    /// </summary>
    /// <param name="state">The scanner state at the abort point</param>
    /// <returns>A MatchResult indicating abort</returns>
    internal static MatchResult Abort(ScannerState state)
        => new(state.CursorPosition, state.CursorPosition, Status.ABORT);

    /// <summary>
    /// Creates a success result from a scanner
    /// </summary>
    /// <param name="scan">The scanner after the successful match</param>
    /// <returns>A MatchResult indicating success</returns>
    internal static MatchResult Success(Scanner scan)
        => new(scan.PreviousCursorPosition, scan.CursorPosition, Status.SUCCESS);

    /// <summary>
    /// Creates a success result from scanner state
    /// </summary>
    /// <param name="state">The scanner state after the successful match</param>
    /// <returns>A MatchResult indicating success</returns>
    internal static MatchResult Success(ScannerState state)
        => new(state.PreviousCursorPosition, state.CursorPosition, Status.SUCCESS);

    #endregion

    /// <summary>
    /// Creates a new MatchResult
    /// </summary>
    /// <param name="preCursor">Cursor position before the match</param>
    /// <param name="postCursor">Cursor position after the match</param>
    /// <param name="status">The outcome of the match</param>
    private MatchResult(int preCursor, int postCursor, Status status)
    {
        PreCursor = preCursor;
        PostCursor = postCursor;
        Outcome = status;
    }
}