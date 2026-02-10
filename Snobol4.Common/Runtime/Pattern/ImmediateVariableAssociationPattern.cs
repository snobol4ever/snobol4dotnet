using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class ImmediateVariableAssociation1 : NullPattern
{
    #region Members

                    private readonly ImmediateVariableAssociation2 _va2;

    #endregion

    #region Construction

                                                        internal ImmediateVariableAssociation1(ImmediateVariableAssociation2 va2)
    {
        _va2 = va2;
    }

    #endregion

    #region Methods

                                                                        internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("$1", scan.Exec);


        _va2.PreCursor = scan.CursorPosition;
        return MatchResult.Success(scan);
    }

                                    internal override Pattern Clone()
    {
        return new ImmediateVariableAssociation1(_va2);
    }

    #endregion
}

internal class ImmediateVariableAssociation2 : NullPattern
{
    #region Members

                    internal int PreCursor;

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