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
        var con2 = new ConcatenatePattern(
            new NullPattern(), 
            new UnevaluatedPattern(functionName, false));
        var alt = new AlternatePattern(
            new NullPattern(), 
            new UnevaluatedPattern(functionName, true));
        return new ConcatenatePattern(con2, alt);
    }

    internal override Pattern Clone()
    {
        return new UnevaluatedPattern(_functionName, _reScan);
    }

                                        internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("*", scan.Exec);

        // Evaluate the function to get the actual pattern
        _functionName(scan.Exec);

        if (scan.Exec.Failure)
            return MatchResult.Failure(scan);

        var evaluatedExpression = scan.Exec.SystemStack.Pop();
        if (!evaluatedExpression.Convert(Executive.VarType.PATTERN, out _, out var p, scan.Exec))
        {
            scan.Exec.LogRuntimeException(46);
            return MatchResult.Failure(scan);
        }

        // Match the evaluated pattern against the remaining subject
        var pattern = (Pattern)p;
        Scanner scanner = new(scan.Exec);
        var mr = scanner.PatternMatch(scan.Subject[scan.CursorPosition..], pattern, 0, true);
        scan.CursorPosition += mr.PostCursor;

        // If rescan is enabled and match succeeded, save alternate for backtracking
        if (_reScan && mr.Outcome == MatchResult.Status.SUCCESS)
            scan.SaveAlternate(node);

        return mr;
    }

    #endregion

    #region Debugging

                                        public override string DebugPattern() => "*(expression)";

    #endregion
}
