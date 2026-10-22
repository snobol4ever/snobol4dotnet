namespace Snobol4.Common;

internal class ScannerState
{
    public int PreviousCursorPosition { get; set; }
    public int CursorPosition { get; set; }
    public string Subject { get; }

    // One entry of the alternate stack: the node to resume at the cursor saved with it, or a sentinel.
    // Base is used only by the entries of an immediate assignment (see StartImmediate).
    private readonly record struct Alternate(int Node, int Cursor, int Base = -1);

    private readonly List<Alternate> _alternates = [];

    // Sentinel values for an entry's Node:
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
    //   ImmediateStart, ImmediateInner : the entries of P $ V (see StartImmediate); transparent like a mark,
    //        except that backup through one makes its Base the immediate base again.
    //
    // Reference: J.F. Gimpel, "A Theory of Discrete Patterns and Their Implementation
    //            in SNOBOL4", CACM 16(2), Feb 1973, pp.91-100.  The mark/seal pair
    //            corresponds to the "PUSH(NULL)/PUSH(CURSOR)" boundary marker for
    //            unevaluated expressions in Fig.11 — adapted for the FENCE compound.
    private const int ImmediateStart = int.MinValue;
    private const int ImmediateInner = int.MinValue + 1;

    // The index of the ImmediateStart entry of the innermost P $ V whose P is being matched, or -1 (SPITBOL's pmhbs)
    private int _immediateBase = -1;

    public ScannerState(string subject, int startPosition)
    {
        Subject = subject;
        PreviousCursorPosition = CursorPosition = startPosition;

        // Initialize with sentinel values
        _alternates.Add(new Alternate(-1, -1));
    }

    private static bool IsTransparent(int node) => node is -3 or ImmediateStart or ImmediateInner;

    public void SaveAlternate(int nodeIndex)
    {
        _alternates.Add(new Alternate(nodeIndex, CursorPosition));
    }

    public (int nodeIndex, int cursorPosition) RestoreAlternate()
    {
        // Skip any stale -3 marks: they're transparent on backtrack.
        // (They become stale when a FENCE'd P fails before its Seal could fire,
        //  and the surrounding outer backtrack walks past the mark.)
        // Backup through an entry of P $ V restores the immediate base it holds (SPITBOL p_imb, p_imd).
        while (IsTransparent(_alternates[^1].Node))
        {
            if (_alternates[^1].Node != -3)
                _immediateBase = _alternates[^1].Base;
            _alternates.RemoveAt(_alternates.Count - 1);
        }

        var (node, cursor, _) = _alternates[^1];
        _alternates.RemoveAt(_alternates.Count - 1);
        CursorPosition = cursor;
        return (node, cursor);
    }

    public bool HasAlternates()
    {
        // Walk past any transparent entries on top; HasAlternates is true iff there's
        // a real alternate (or a -2 seal) below them.
        for (var i = _alternates.Count - 1; i >= 0; i--)
        {
            if (IsTransparent(_alternates[i].Node)) continue;
            return _alternates[i].Node != -1;
        }
        return false;
    }

    public void ClearAlternates()
    {
        _alternates.Clear();
        _alternates.Add(new Alternate(-1, -1));
        _immediateBase = -1;
    }

    // P $ V, entering P (SPITBOL p_ima): stack the cursor P starts at with the immediate base it replaces, and make
    // this entry the base.  The start is kept on the stack, not in the pattern, because the same P can be matched again
    // inside itself (LIST = ('(' *LIST ')') $ GRP) before it ends here.
    public void StartImmediate()
    {
        _alternates.Add(new Alternate(ImmediateStart, CursorPosition, _immediateBase));
        _immediateBase = _alternates.Count - 1;
    }

    // P $ V, P matched (SPITBOL p_imc): returns the cursor P started at and makes the outer base the base again.  If P
    // left alternates above its entry, stack one that makes it the base again when backup re-enters P (p_imd); if not,
    // nothing can re-enter P and its entry goes.
    public int FinishImmediate()
    {
        var start = _immediateBase;
        var (_, cursor, outer) = _alternates[start];
        _immediateBase = outer;
        if (start == _alternates.Count - 1)
            _alternates.RemoveAt(start);
        else
            _alternates.Add(new Alternate(ImmediateInner, CursorPosition, start));
        return cursor;
    }

    // MarkAlternates: push a -3 mark, captured at FENCE entry.  SealAlternates
    // later pops down to and through the most recent -3, preserving outer
    // alternates that were on the stack before FENCE was entered.
    public void MarkAlternates()
    {
        _alternates.Add(new Alternate(-3, CursorPosition));
    }

    // SealAlternates: pop entries until we find the most recent -3 mark;
    // pop the mark itself; then push -2 seal sentinel atop the preserved
    // outer alternates.  If no -3 is found (defensive: floor reached),
    // push -2 atop the floor — equivalent to old "wipe" behaviour for
    // patterns built without a paired Mark.
    // Backup through FENCE(P) still undoes what P did (SPITBOL: backup passes through it), so an
    // alternate that only undoes a side effect of P — the backup of a conditional assignment, named by
    // undoes — is kept under the seal, in its order; every other alternate inside P is dropped.  The
    // entries of every P $ V inside it go too: each such P has ended, and backup cannot re-enter it.
    public void SealAlternates(Func<int, bool> undoes)
    {
        List<Alternate> kept = [];
        while (_alternates.Count > 0)
        {
            var top = _alternates[^1];
            if (top.Node == -3)
            {
                _alternates.RemoveAt(_alternates.Count - 1);
                break;
            }
            if (top.Node == -1)
                break; // hit floor — no paired mark; preserve floor
            _alternates.RemoveAt(_alternates.Count - 1);
            if (top.Node >= 0 && undoes(top.Node))
                kept.Add(top with { Node = -4 - top.Node });
            else if (top.Node <= -4 && top.Node is not (ImmediateStart or ImmediateInner))
                kept.Add(top);   // an undo an inner FENCE kept
        }
        for (var i = kept.Count - 1; i >= 0; i--)
            _alternates.Add(kept[i]);
        _alternates.Add(new Alternate(-2, CursorPosition));
    }
}
