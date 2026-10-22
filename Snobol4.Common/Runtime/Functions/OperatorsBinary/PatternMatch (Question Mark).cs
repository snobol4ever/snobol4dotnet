namespace Snobol4.Common;

//"pattern match right operand is not pattern" /* 240 */,
//"pattern match left operand is not a string" /* 241 */,

public partial class Executive
{
                    public void PatternMatch(List<Var> arguments)
    {
        // arguments[0]: Subject and left operand
        // arguments[1]: Pattern and right operand

        // Save arguments[0] (subject) before evaluating any ExpressionVar in arguments[1].
        // _reusableArgList is shared: inner Function() calls (e.g. from RunExpressionThread
        // when evaluating *fn()) will Clear() and refill it, clobbering arguments[0] with
        // the inner call's argument (e.g. the IntegerVar(5) arg to LEN inside *mypat()).
        // Saving to a local Var is zero-allocation and survives the inner call.
        // Found: GOAL-NET-BEAUTY-SELF session #81 — *mypat() returning LEN(5) matched
        // subject "5" (= coerced IntegerVar from LEN's arg) instead of "hello".
        var savedSubject = arguments[0];
        // A deferred pattern that calls a program-defined function (*ONCE(N)) runs a body whose calls clear
        // _reusableArgList, the list arguments may be, outright; the match works on its own copy
        if (arguments[1] is ExpressionVar)
            arguments = [.. arguments];
        while (arguments[1] is ExpressionVar expressionVar1)
        {
            expressionVar1.FunctionName(this);
            if (Failure) { Failure = false; NonExceptionFailure(); return; }
            if (SystemStack.Count == 0) { NonExceptionFailure(); return; }
            arguments[1] = SystemStack.Pop();
            // Restore subject — inner Function() calls clobber _reusableArgList[0]
            arguments[0] = savedSubject;
        }

        // Right argument must resolve to a pattern
        if (!arguments[1].Convert(VarType.PATTERN, out _, out var patternValue, this))
        {
            LogRuntimeException(240);
            return;
        }

        // Left argument must resolve to a string
        if (!arguments[0].Convert(VarType.STRING, out var subject, out var subjectValue, this))
        {
            LogRuntimeException(241);
            return;
        }

        // If the subject is converted from something other than a string, it loses its symbol.
        // So preserve it here.
        subject.Symbol = arguments[0].Symbol;

        // Try the match.
        // Save and temporarily replace BetaStack so that any nested PatternMatch invoked
        // during the match (e.g. via `$ *fn(...)` immediate-assignment side effects calling
        // a user function that itself runs a nested `?`) does not see -- or commit -- the
        // outer PatternMatch's own conditional-assignment accumulations.
        // Without this isolation, the inner PatternMatch's BetaStack.Reverse() commit loop
        // walks the outer BetaStack and causes spurious extra function calls.
        // Found via beauty self-host sync-step monitor: divergence at step #2839 --
        // spl emits RETURN match (NRETURN) while dot emits a 19th CALL upr because
        // the outer BetaStack's `.`-captures were being committed by the inner PatternMatch.
        // See GOAL-NET-BEAUTY-SELF S-2-bridge-7-fullscan session #71.
        var anchor = AmpAnchor;
        var savedBetaStack = BetaStack;
        BetaStack = [];
        // AlphaStack also needs isolation: CVA1 (.1) pushes entries during match;
        // CVABackup1 (.2) pops them on backtrack.  Without save/restore, an unanchored
        // retry loop leaves stale AlphaStack entries from previous cursor positions,
        // and the next CVABackup1.Scan throws InvalidOperationException on the empty
        // stack.  Mirror the BetaStack isolation pattern exactly.
        // Found: beauty self-host S-2-bridge-7-fullscan session #81.
        var savedAlphaStack = AlphaStack;
        AlphaStack = [];
        Scanner scanner = new(this);

        var mr = scanner.PatternMatch((string)subjectValue, (Pattern)patternValue, 0, anchor != 0);

        if (mr.Outcome != MatchResult.Status.SUCCESS)
        {
            BetaStack = savedBetaStack;
            AlphaStack = savedAlphaStack;
            NonExceptionFailure();
            return;
        }

        // Perform conditional assignments from this PatternMatch's own BetaStack. An assignment whose deferred target fails or is an
        // error (the run went on because &ERRLIMIT lets it) fails the statement, and the assignments after it are not made, as in SPITBOL;
        // an error raised inside a function the target calls fails only that function's statement, and its return is the target's
        var committed = true;
        foreach (var nameListEntry in BetaStack.Reverse())
        {
            List<Var> assignment =
            [
                nameListEntry.Assignee,
                new StringVar(nameListEntry.Scan.Subject[nameListEntry.PreCursor..nameListEntry.PostCursor])
            ];

            // an assignment that was not made (the evaluation of its deferred target failed or was an error) leaves nothing to take off
            var depth = SystemStack.Count;
            Failure = false;
            Assign(assignment);
            while (SystemStack.Count > depth)
                SystemStack.Pop();
            if (Failure)
            {
                committed = false;
                break;
            }
        }

        // Restore the caller's BetaStack and AlphaStack now that this PatternMatch has committed its own.
        BetaStack = savedBetaStack;
        AlphaStack = savedAlphaStack;
        if (!committed)
        {
            NonExceptionFailure();
            return;
        }

        // After a successful pattern match, the statement-level Failure flag must
        // reflect the match outcome (SUCCESS), not the side-effect outcome of any
        // deferred conditional-assignment commits in BetaStack.  Without this clear,
        // a deferred `*fn() . cap`-style assignee whose underlying fn() returns
        // FRETURN sets Failure=true on the way out, and the SUCCESSFUL match is
        // misreported as a failure to the statement engine -- taking :F when the
        // match clearly succeeded.  Found via beauty self-host: `&FULLSCAN = 1`
        // parsed correctly but mainErr1 still fired.  See GOAL-NET-BEAUTY-SELF
        // S-2-bridge-7-betastack-failure-leak.
        Failure = false;

        // Store object reference to save SubjectVar in a symbol table
        var subjectVar = new SubjectVar((string)subjectValue, mr)
        {
            Symbol = subject.Symbol
        };

        // an element this statement read (A[K] P = R) is the replacement's target, as in SPITBOL
        if (savedSubject is { Key: not null, Collection: ArrayVar or TableVar } && savedSubject.LvalueMark == StatementSerial)
        {
            subjectVar.Key = savedSubject.Key;
            subjectVar.Collection = savedSubject.Collection;
        }
        SystemStack.Push(subjectVar);
    }
}
