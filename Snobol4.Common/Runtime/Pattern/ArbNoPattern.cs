using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class ArbNoPattern : TerminalPattern
{
    #region Members

    private readonly Pattern _arbPattern;

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

        // One more repetition: the argument entered as a call (Scanner.Call) whose end returns to the loop, the NULL
        // beside this node in NULL | ARBNO-node, which tries no more and keeps this node as its alternate.  So every
        // repetition's alternatives sit on the outer alternate stack and backtracking reaches into them, as in
        // SPITBOL.  Each repetition after the zeroth must advance the cursor; a null one would re-enter this node at
        // the same cursor forever, and the zero-repetition arm already matches null.
        var loop = scan.GetNode(scan.GetNode(node).ParentIndex).LeftChild;
        scan.Call(loop, scan.CursorPosition);
        return MatchResult.Goto(scan, scan.Enter(_arbPattern));
    }

    #endregion

    #region Debugging

    public override string DebugPattern() => "arbno";

    #endregion
}
