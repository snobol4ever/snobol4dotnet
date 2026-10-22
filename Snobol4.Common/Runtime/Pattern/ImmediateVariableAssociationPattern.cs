using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class ImmediateVariableAssociation1 : NullPattern
{
    #region Methods

                                                                        internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("$1", scan.Exec);


        // The start of P is stacked, not kept here: P can be matched again inside itself before its $ fires
        scan.StartImmediate();
        return MatchResult.Success(scan);
    }

                                    internal override Pattern Clone()
    {
        return new ImmediateVariableAssociation1();
    }

    #endregion
}

internal class ImmediateVariableAssociation2 : NullPattern
{
    #region Members

                internal Var Assignee;

    #endregion

    #region Construction

                                    internal ImmediateVariableAssociation2(Var assignee)
    {
        Assignee = assignee;
    }

    #endregion

    #region Methods

                                internal override Pattern Clone()
    {
        return new ImmediateVariableAssociation2(Assignee);
    }

                                                                                                                                                                        internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("$2", scan.Exec);

        // Extract the matched substring using range syntax
        List<Var> arguments =
        [
            Assignee,
            new StringVar(scan.Subject[scan.FinishImmediate()..scan.CursorPosition])
        ];

        // Perform the immediate assignment
        scan.Exec.Assign(arguments);

        // If Assignee evaluation (for deferred forms like *fn(...)) caused a
        // FRETURN failure during Assign, this $-association must fail so the
        // outer scanner backtracks to any saved alternate.  Without this check,
        // a `pat $ *fn(...)` in an alternation reports false success here even
        // when fn() FRETURNed, breaking alternation backtracking.
        // Minimal repro:
        //   patA = SPAN(&UCASE &LCASE) $ tx $ *match(listA, lstPat)
        //   patB = SPAN(&UCASE &LCASE) $ tx $ *match(listB, lstPat)
        //   'FULLSCAN' (patA | patB)  ; SPITBOL: PASS, dot pre-fix: FAIL.
        if (scan.Exec.Failure)
            return MatchResult.Failure(scan);

        // Return success without advancing cursor
        var mr = MatchResult.Success(scan);

        return mr;
    }

    #endregion
}