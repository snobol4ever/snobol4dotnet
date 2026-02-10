using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
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
        using var profile1 = Profiler.Start4("@", scan.Exec);

        // Create arguments for assignment: [variable, cursor_position]
        List<Var> arguments =
        [
            _assignee,
            new IntegerVar(scan.CursorPosition)
        ];

        // Perform the assignment using the executive's Assign method
        _exec.Assign(arguments);

        // Pop the assignment result from the stack (we don't need it)
        _exec.SystemStack.Pop();

        // Always succeed without advancing the cursor
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

                                                    public override string DebugPattern() => "@";

    #endregion

}