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

        // SPITBOL @N: immediately assign current cursor position (integer) to the
        // named variable. Write directly to IdentifierTable to avoid SystemStack
        // side-effects from the general Assign() path that corrupt pattern matching.
        var symbol = _assignee is NameVar nv ? nv.Pointer : _assignee.Symbol;
        if (symbol != "")
        {
            var cursorVar = new IntegerVar(scan.CursorPosition) { Symbol = symbol };
            _exec.IdentifierTable[symbol] = cursorVar;
        }

        // Always succeed without advancing the cursor
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

                                                    public override string DebugPattern() => "@";

    #endregion

}