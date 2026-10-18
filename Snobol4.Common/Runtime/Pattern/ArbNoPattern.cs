using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class ArbNoPattern : TerminalPattern
{
    #region Members

    private readonly Pattern _arbPattern;

    // One repetition pattern per starting cursor, so a repetition re-entered at the same
    // cursor by backtracking reuses its graft instead of appending another copy.
    private readonly Dictionary<int, Pattern> _repetitions = [];

    #endregion

    #region Construction

    internal ArbNoPattern(Pattern arbPattern)
    {
        _arbPattern = arbPattern ?? throw new ArgumentNullException(nameof(arbPattern));
    }

    #endregion

    #region Methods

    internal static Pattern Structure(Pattern arbPattern)
    {
        return new ArbNoPattern(arbPattern).Repeat();
    }

    // Zero repetitions first, then one more: NULL | ARBNO-node
    private Pattern Repeat()
    {
        return new ConcatenatePattern(
            new NullPattern(),
            new AlternatePattern(
                new NullPattern(),
                this));
    }

    internal override Pattern Clone()
    {
        return new ArbNoPattern(_arbPattern);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Arbno", scan.Exec);

        // One more repetition, grafted into the running match as
        //     argument  MUST-ADVANCE-PAST(cursor)  (NULL | ARBNO-node)
        // so every repetition's alternatives sit on the outer alternate stack and
        // backtracking reaches into them, as in SPITBOL. Each repetition after the
        // zeroth must advance the cursor; a null one would re-enter this node at the
        // same cursor forever, and the zero-repetition arm already matches null.
        var start = scan.CursorPosition;
        if (!_repetitions.TryGetValue(start, out var repetition))
        {
            repetition = new ConcatenatePattern(
                _arbPattern,
                new ConcatenatePattern(new MustAdvancePattern(start), Repeat()));
            _repetitions[start] = repetition;
        }

        var graftedStart = scan.Graft(repetition, scan.GetNode(node).Subsequent);
        return MatchResult.Goto(scan, graftedStart);
    }

    #endregion

    #region Debugging

    public override string DebugPattern() => "arbno";

    #endregion
}

/// <summary>
/// Succeeds only past the cursor its ARBNO repetition started at, so that each
/// repetition consumes at least one character.
/// </summary>
internal sealed class MustAdvancePattern : TerminalPattern
{
    private readonly int _start;

    internal MustAdvancePattern(int start)
    {
        _start = start;
    }

    internal override Pattern Clone()
    {
        return new MustAdvancePattern(_start);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        return scan.CursorPosition > _start
            ? MatchResult.Success(scan)
            : MatchResult.Failure(scan);
    }

    public override string DebugPattern() => "must-advance";
}
