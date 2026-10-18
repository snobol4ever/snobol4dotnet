using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class UnevaluatedPattern : TerminalPattern
{
    #region Members

    private readonly Executive.DeferredCode _functionName;
    private readonly bool _reScan;

    #endregion

    #region Construction

                        internal UnevaluatedPattern(Executive.DeferredCode functionName, bool reScan)
    {
        _functionName = functionName;
        _reScan = reScan;
    }

    #endregion

    #region Methods

                        internal static Pattern Structure(Executive.DeferredCode functionName)
    {
        return new UnevaluatedPattern(functionName, false);
    }

    internal override Pattern Clone()
    {
        return new UnevaluatedPattern(_functionName, _reScan);
    }

    // S-2-bridge-7-byrd-pattern: tag for wire trace (e.g. "*snoString").
    // Uses the delegate's Method.Name when available; otherwise "*?".
    internal string MethodName
    {
        get
        {
            try { return "*" + (_functionName?.Method?.Name ?? "?"); }
            catch { return "*?"; }
        }
    }

                                        internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("*", scan.Exec);

        // Left recursion never moves the cursor; past the limit it is SPITBOL's stack overflow
        if (scan.CursorPosition == scan.StarCursor)
        {
            if (++scan.StarRepeats > Scanner.StarRepeatLimit)
            {
                scan.Exec.LogRuntimeException(246, fatal: true);
                return MatchResult.Failure(scan);
            }
        }
        else
        {
            scan.StarCursor = scan.CursorPosition;
            scan.StarRepeats = 0;
        }

        // Evaluate the function to get the actual pattern
        _functionName(scan.Exec);

        if (scan.Exec.Failure)
            return MatchResult.Failure(scan);

        var evaluatedExpression = scan.Exec.SystemStack.Pop();
        // If the deferred code pushed an ExpressionVar (e.g. *P where P is a plain
        // variable), evaluate it one more level to get the actual value (PatternVar etc.).
        if (evaluatedExpression is ExpressionVar exprVar)
        {
            exprVar.FunctionName(scan.Exec);
            if (scan.Exec.Failure)
                return MatchResult.Failure(scan);
            evaluatedExpression = scan.Exec.SystemStack.Pop();
        }
        if (!evaluatedExpression.Convert(Executive.VarType.PATTERN, out _, out var p, scan.Exec))
        {
            scan.Exec.LogRuntimeException(46);
            return MatchResult.Failure(scan);
        }

        // Graft the evaluated pattern's nodes into the running scanner's AST, wired
        // so the last grafted node continues to whatever follows *X (node.Subsequent,
        // or -1 if *X is the last thing in the pattern).  The Match loop jumps to the
        // grafted start node via GOTO, keeping the same cursor and alternate stack —
        // so ARBNO and other backtracking constructs inside *X work correctly.
        var pattern = (Pattern)p;
        int graftedStart = scan.Graft(pattern, scan.GetNode(node).Subsequent);
        return MatchResult.Goto(scan, graftedStart);
    }

    #endregion

    #region Debugging

                                        public override string DebugPattern() => "*(expression)";

    #endregion
}
