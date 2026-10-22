using System.Collections.Generic;

namespace Snobol4.Common;

public class Scanner
{
    internal int PreviousCursorPosition
    {
        get => _state?.PreviousCursorPosition ?? 0;
        set
        {
            if (_state is not null)
                _state.PreviousCursorPosition = value;
        }
    }

    internal int CursorPosition
    {
        get => _state?.CursorPosition ?? 0;
        set
        {
            if (_state is not null)
                _state.CursorPosition = value;
        }
    }

    internal string Subject => _state?.Subject ?? "";
    internal Executive Exec { get; }

    private ScannerState? _state;
    private AbstractSyntaxTree? _ast;

    // S-2-bridge-7-fullscan: terminal-failure memoization cache.
    //
    // Per SNOBOL4 semantics, side-effect-free terminal patterns are
    // purely deterministic: same (node-index, cursor) always produces
    // the same outcome against the same subject.  When the alt-stack
    // accumulates many restore-points all leading back to the same
    // terminal at the same cursor (catastrophic backtracking in nested
    // alternations like beauty.sno's Function|BuiltinVar|SpecialNm
    // arms), memoizing FAILURE eliminates exponential retry.
    //
    // SPITBOL does not retry a terminal at the same cursor once it has
    // failed — this cache makes dot match that behavior.
    //
    // Only PURE terminals are memoized (no side effects):
    //   SpanPattern (static char list only), LiteralPattern, AnyPattern,
    //   BreakPattern, PosPattern, RPosPattern, LenPattern, NullPattern,
    //   TabPattern, RTabPattern, FailPattern, AbortPattern.
    //
    // Terminals with side effects are NOT cached:
    //   SpanPattern with _functionName (dynamic char list via deferred code),
    //   ArbNoPattern, UnevaluatedPattern, ImmediateVariableAssociation*,
    //   ConditionalVariableAssociation*, CursorAssignmentPattern.
    //
    // Cache is cleared at the start of every PatternMatch call.
    private readonly HashSet<(int node, int cursor)> _terminalFailCache = new();

    // *X evaluations since the cursor last moved: a left-recursive pattern (P = *P 'Z' | 'Y') re-enters
    // itself without consuming anything, which in SPITBOL overflows the stack (error 246)
    internal int StarCursor = -1;
    internal int StarRepeats;
    internal const int StarRepeatLimit = 100_000;

    internal Scanner(Executive exec)
    {
        Exec = exec;
    }

    internal MatchResult PatternMatch(string subject, Pattern pattern, int startPosition, bool anchor)
    {
        _ast = AbstractSyntaxTree.Build(pattern);
        _state = new ScannerState(subject, startPosition);
        _terminalFailCache.Clear(); // S-2-bridge-7-fullscan: reset per-match memoization
        StarCursor = -1;
        StarRepeats = 0;

        var length = anchor ? 0 : subject.Length;

        for (var cursorPosition = startPosition; cursorPosition <= length; ++cursorPosition)
        {
            _state.PreviousCursorPosition = _state.CursorPosition = cursorPosition;
            var mr = Match(_ast.StartNode);
            if (mr.IsSuccess || mr.IsAbort)
                return mr;
        }

        return MatchResult.Failure(_state);
    }

    internal void SaveAlternate(int node)
    {
        _state?.SaveAlternate(node);
    }

    internal void SealAlternates()
    {
        _state?.SealAlternates(node => _ast![node].Self is ConditionalVariableAssociationBackup1 or ConditionalVariableAssociationBackup2);
    }

    internal void MarkAlternates()
    {
        _state?.MarkAlternates();
    }

    internal void StartImmediate()
    {
        _state?.StartImmediate();
    }

    internal int FinishImmediate() => _state?.FinishImmediate() ?? 0;

    // The start node of subPattern's nodes, built the first time it is entered in this match (AbstractSyntaxTree.Enter);
    // the Match loop jumps there via MatchResult.Goto
    internal int Enter(Pattern subPattern) => _ast!.Enter(subPattern);

    // Enter a pattern as a call: its end returns to returnNode, failing unless the cursor is then past advanceFrom (an
    // ARBNO repetition; -1 for none).  A call whose returnNode is -1 is in tail position, its caller's own end, and its
    // end returns where its caller's would, so it needs no frame.
    internal void Call(int returnNode, int advanceFrom)
    {
        if (returnNode >= 0 && _state is not null)
            _state.Frames = new ScannerState.Frame(returnNode, advanceFrom, _state.Frames);
    }

    internal AbstractSyntaxTreeNode GetNode(int index) => _ast![index];

    // The nodes the last match built: the statement's pattern and one copy of each pattern it entered
    internal int NodeCount => _ast?.Count ?? 0;

    // The function level the match runs at (Match)
    private long _matchFunctionLevel;

    // S-2-bridge-7-byrd-pattern: tag a pattern node for the wire trace.
    // UnevaluatedPattern reports its function name (e.g. "*snoString").
    // LiteralPattern reports its literal in single-quotes (e.g. 'h', '"').
    // Other terminal patterns report their type name (BreakPattern, SpanPattern, ...).
    private static string NodeTag(AbstractSyntaxTreeNode node)
    {
        if (node.Self is UnevaluatedPattern up) return up.MethodName;
        if (node.Self is LiteralPattern lp)     return "'" + lp.Literal + "'";
        return node.Self.GetType().Name;
    }

    // Returns true if this terminal pattern is side-effect-free and its
    // outcome at a given (node, cursor) is fully determined by the cursor
    // position and subject — safe to memoize.
    //
    // IMPORTANT: NullPattern and LiteralPattern have subclasses with
    // side-effects (ConditionalVariableAssociation*, ImmediateVariableAssociation*).
    // Use GetType() exact-type check for LiteralPattern/NullPattern to
    // avoid memoizing those subclasses.
    private static bool IsPureTerminal(Pattern p)
    {
        // Use GetType() for NullPattern and LiteralPattern because their
        // subclasses (CVA*, IVA*) have side effects and must NOT be memoized.
        var t = p.GetType();
        if (t == typeof(NullPattern))    return true;
        if (t == typeof(LiteralPattern)) return true;
        return p switch
        {
            SpanPattern sp        => sp.IsStaticCharList,   // static char list only
            AnyPattern            => true,
            BreakPattern bp       => bp.IsStaticCharList,   // static char list only
            LenPattern            => true,
            FailPattern           => true,
            AbortPattern          => true,
            PosPattern            => true,
            RPosPattern           => true,
            TabPattern            => true,
            RTabPattern           => true,
            _                     => false,
        };
    }

    private MatchResult Match(AbstractSyntaxTreeNode node)
    {
        _state!.ClearAlternates();
        _matchFunctionLevel = Exec.AmpFunctionLevel;

        // Set when an ARBNO repetition returns to its loop without getting past its start: the loop goes on to what follows, but
        // keeps no alternate to try a further repetition from there
        var noFurtherRepetition = false;

        while (true)
        {
            if (node.HasAlternate() && !noFurtherRepetition)
            {
                _state.SaveAlternate(node.Alternate);
            }
            noFurtherRepetition = false;

            // S-2-bridge-7-byrd-pattern: PM_CALL — entering a node match.
            MonitorIpc.EmitPmCall(NodeTag(node), _state.CursorPosition);

            // S-2-bridge-7-fullscan: terminal-failure memoization.
            // If this is a pure terminal that already failed at this cursor
            // in this PatternMatch invocation, fail immediately without
            // re-invoking Scan.  Same semantics as SPITBOL, which does not
            // retry a deterministic terminal at the same cursor.
            MatchResult mr;
            if (IsPureTerminal(node.Self) && _terminalFailCache.Contains((node.SelfIndex, _state.CursorPosition)))
            {
                mr = MatchResult.Failure(_state);
            }
            else
            {
                Exec.Failure = false;
                var errors = Exec.ErrorSerial;
                mr = ((TerminalPattern)node.Self).Scan(node.SelfIndex, this);

                // An error the scan raised at the match's own level (SPAN(*Y) with Y null, LEN(*'z')) ends the whole match, as in SPITBOL,
                // where it failed only this node and the scan went on to the next cursor, raising it again at each; an error inside a
                // function the scan called failed only that function's statement, and the match goes on
                if (Exec.ErrorSerial != errors && Exec.ErrorFunctionLevel == _matchFunctionLevel)
                    return MatchResult.Abort(_state);

                // Record failure of pure terminals for subsequent alt-stack restores.
                if (mr.Outcome == MatchResult.Status.FAILURE && IsPureTerminal(node.Self))
                    _terminalFailCache.Add((node.SelfIndex, _state.CursorPosition));
            }

            switch (mr.Outcome)
            {
                case MatchResult.Status.SUCCESS:
                    // PM_EXIT — node Scan succeeded; advancing cursor reflected.
                    MonitorIpc.EmitPmExit(NodeTag(node), _state.CursorPosition);
                    if (node.HasSubsequent())
                    {
                        node = node.GetSubsequent()!;
                        break;
                    }
                    // The end of a pattern: the statement's own ends the match; one entered by *X or ARBNO returns
                    // to its caller. An ARBNO repetition that did not get past its start still goes on to what follows
                    // the ARBNO, as in SPITBOL, but no further repetition is tried from it, which would repeat forever
                    if (_state.Frames is not { } frame)
                        return MatchResult.Success(_state);
                    _state.Frames = frame.Parent;
                    noFurtherRepetition = _state.CursorPosition <= frame.AdvanceFrom;
                    node = _ast![frame.ReturnNode];
                    break;

                case MatchResult.Status.FAILURE:
                    if (!_state.HasAlternates())
                    {
                        // PM_FAIL — propagate FAILURE outward (no live alt).
                        MonitorIpc.EmitPmFail(NodeTag(node), _state.CursorPosition);
                        return mr;
                    }
                    var (alternateIndex, _) = _state.RestoreAlternate();
                    // Seal hit on backtrack: backtrack INTO the fenced region is blocked,
                    // but backup passes THROUGH FENCE(P) (SPITBOL manual: it does not abort
                    // the match as &FENCE does).  OUTER alternates saved BEFORE the FENCE
                    // was entered (and which sit BELOW the seal on the alt stack — preserved
                    // by SealAlternates) must still be allowed to fire — they represent
                    // OTHER ways the OUTER pattern could match without re-entering the
                    // FENCE.  Pop the seal and continue to the next non-seal entry.  If only
                    // the floor (-1) remains, the match fails at this starting cursor and an
                    // unanchored match tries the next one.
                    while (alternateIndex == -2 || alternateIndex <= -4)
                    {
                        if (alternateIndex <= -4)
                        {
                            // an undo kept under the seal (see ScannerState): run its side effect, which fails
                            var undo = _ast![-4 - alternateIndex];
                            ((TerminalPattern)undo.Self).Scan(undo.SelfIndex, this);
                        }
                        if (!_state.HasAlternates())
                        {
                            // PM_FAIL — seal-only stack, nothing left to try from this cursor.
                            MonitorIpc.EmitPmFail(NodeTag(node), _state.CursorPosition);
                            return MatchResult.Failure(_state);
                        }
                        (alternateIndex, _) = _state.RestoreAlternate();
                    }
                    node = _ast![alternateIndex];
                    // PM_REDO — backtracked into restored node at restored cursor.
                    MonitorIpc.EmitPmRedo(NodeTag(node), _state.CursorPosition);
                    break;

                case MatchResult.Status.ABORT:
                    // PM_FAIL — explicit ABORT from a Scan() (e.g. AbortPattern, seal).
                    MonitorIpc.EmitPmFail(NodeTag(node), _state.CursorPosition);
                    return mr;

                case MatchResult.Status.GOTO:
                    node = _ast![mr.GotoNode];
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal void Dump(Pattern rootPattern)
    {
        _ast?.Dump();
    }
}