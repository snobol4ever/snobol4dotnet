namespace Snobol4.Common;

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
        List<Var> arguments =
        [
            Assignee,
            new StringVar(scan.Subject[PreCursor..scan.CursorPosition])
        ];

        scan.Exec.Assign(arguments);
        var mr = MatchResult.Success(scan);
        //Console.Error.WriteLine("!!!" + Assignee.Symbol + " " + arguments[1].ToString());
        return mr;
    }

    #endregion
}