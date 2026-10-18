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

        // An element (@A<1>) or a deferred target (@*V, @*$('CUR' N), @*A<2>, @*F() with an NRETURN) is
        // assigned as any target is, as SPITBOL does; the assignment's own stack effects are undone
        if (_assignee is ExpressionVar || _assignee.Collection is not null)
        {
            var depth = _exec.SystemStack.Count;
            _exec.Assign([_assignee, new IntegerVar(scan.CursorPosition)]);
            var failed = _exec.Failure;
            while (_exec.SystemStack.Count > depth)
                _exec.SystemStack.Pop();
            return failed ? MatchResult.Failure(scan) : MatchResult.Success(scan);
        }

        if (symbol != "")
        {
            var cursorVar = new IntegerVar(scan.CursorPosition) { Symbol = symbol };
            _exec.IdentifierTable[symbol] = cursorVar;
            // a VALUE trace on the variable fires, as for any assignment (SPITBOL's asign)
            _exec.TraceIdentifierValue(symbol, cursorVar);
        }

        // Always succeed without advancing the cursor
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

                                                    public override string DebugPattern() => "@";

    #endregion

}