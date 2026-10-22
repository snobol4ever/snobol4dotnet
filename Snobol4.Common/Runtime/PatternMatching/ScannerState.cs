namespace Snobol4.Common;

internal class ScannerState
{
    public int PreviousCursorPosition { get; set; }
    public int CursorPosition { get; set; }
    public string Subject { get; }

    private readonly Stack<int> _alternatePatternStack = [];
    private readonly Stack<int> _alternateCursorStack = [];

    // Sentinel values on the alt-pattern stack:
    //   -1 : floor / "no more alternates" (HasAlternates returns false)
    //   -2 : seal — on backtrack here, Match returns ABORT outward
    //        (per Gimpel 1973: FENCE = NULL | ABORT — backtrack into a
    //         sealed FENCE terminates the entire match, no cursor retry)
    //   <= -4 : an undo kept under a seal: the backup of a conditional assignment made inside FENCE(P),
    //        node index -4 - value.  Backup through FENCE(P) runs it as a side effect only (Scanner.Match);
    //        it is never entered as a node, whose own alternate would reopen a branch of P that was sealed.
    //   -3 : mark — placed at FENCE entry; SealAlternates pops down to (and through)
    //        the most recent mark, preserving outer alternates saved before FENCE
    //        was entered. Stale marks (left when FENCE'd P fails before Seal fires)
    //        are transparent to RestoreAlternate / HasAlternates.
    //
    // Reference: J.F. Gimpel, "A Theory of Discrete Patterns and Their Implementation
    //            in SNOBOL4", CACM 16(2), Feb 1973, pp.91-100.  The mark/seal pair
    //            corresponds to the "PUSH(NULL)/PUSH(CURSOR)" boundary marker for
    //            unevaluated expressions in Fig.11 — adapted for the FENCE compound.

    public ScannerState(string subject, int startPosition)
    {
        Subject = subject;
        PreviousCursorPosition = CursorPosition = startPosition;

        // Initialize with sentinel values
        _alternatePatternStack.Push(-1);
        _alternateCursorStack.Push(-1);
    }

    public void SaveAlternate(int nodeIndex)
    {
        _alternatePatternStack.Push(nodeIndex);
        _alternateCursorStack.Push(CursorPosition);
    }

    public (int nodeIndex, int cursorPosition) RestoreAlternate()
    {
        // Skip any stale -3 marks: they're transparent on backtrack.
        // (They become stale when a FENCE'd P fails before its Seal could fire,
        //  and the surrounding outer backtrack walks past the mark.)
        while (_alternatePatternStack.Peek() == -3)
        {
            _alternatePatternStack.Pop();
            _alternateCursorStack.Pop();
        }

        var cursor = _alternateCursorStack.Pop();
        var node = _alternatePatternStack.Pop();
        CursorPosition = cursor;
        return (node, cursor);
    }

    public bool HasAlternates()
    {
        // Walk past any -3 marks on top; HasAlternates is true iff there's
        // a real alternate (or a -2 seal) below them.
        foreach (var v in _alternatePatternStack)
        {
            if (v == -3) continue;
            return v != -1;
        }
        return false;
    }

    public void ClearAlternates()
    {
        _alternatePatternStack.Clear();
        _alternateCursorStack.Clear();
        _alternatePatternStack.Push(-1);
        _alternateCursorStack.Push(-1);
    }

    // MarkAlternates: push a -3 mark, captured at FENCE entry.  SealAlternates
    // later pops down to and through the most recent -3, preserving outer
    // alternates that were on the stack before FENCE was entered.
    public void MarkAlternates()
    {
        _alternatePatternStack.Push(-3);
        _alternateCursorStack.Push(CursorPosition);
    }

    // SealAlternates: pop entries until we find the most recent -3 mark;
    // pop the mark itself; then push -2 seal sentinel atop the preserved
    // outer alternates.  If no -3 is found (defensive: floor reached),
    // push -2 atop the floor — equivalent to old "wipe" behaviour for
    // patterns built without a paired Mark.
    // Backup through FENCE(P) still undoes what P did (SPITBOL: backup passes through it), so an
    // alternate that only undoes a side effect of P — the backup of a conditional assignment, named by
    // undoes — is kept under the seal, in its order; every other alternate inside P is dropped.
    public void SealAlternates(Func<int, bool> undoes)
    {
        List<(int Node, int Cursor)> kept = [];
        while (_alternatePatternStack.Count > 0)
        {
            var top = _alternatePatternStack.Peek();
            if (top == -3)
            {
                _alternatePatternStack.Pop();
                _alternateCursorStack.Pop();
                break;
            }
            if (top == -1)
                break; // hit floor — no paired mark; preserve floor
            var node = _alternatePatternStack.Pop();
            var cursor = _alternateCursorStack.Pop();
            if (node >= 0 && undoes(node))
                kept.Add((-4 - node, cursor));
            else if (node <= -4)
                kept.Add((node, cursor));   // an undo an inner FENCE kept
        }
        for (var i = kept.Count - 1; i >= 0; i--)
        {
            _alternatePatternStack.Push(kept[i].Node);
            _alternateCursorStack.Push(kept[i].Cursor);
        }
        _alternatePatternStack.Push(-2);
        _alternateCursorStack.Push(CursorPosition);
    }
}
