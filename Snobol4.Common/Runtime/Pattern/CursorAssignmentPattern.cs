namespace Snobol4.Common;

internal class AtSign : TerminalPattern
{
    #region Members

    private readonly Var _assignee;
    private readonly Executive _exec;

    #endregion

    #region Constructors

    internal AtSign(Var assignee, Executive exec)
    {
        _assignee = assignee;
        _exec = exec;
    }

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return new AtSign(_assignee, _exec);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        List<Var> arguments =
        [
            _assignee,
            new IntegerVar(scan.CursorPosition)
        ];

        _exec.Assign(arguments);
        _exec.SystemStack.Pop();
        return MatchResult.Success(scan);
    }

    #endregion
}