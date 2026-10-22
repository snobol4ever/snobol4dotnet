using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class BreakPattern : TerminalPattern
{
    #region Members

    private string _charList;
    private readonly Executive.DeferredCode? _functionName;
    private readonly int _error;

    #endregion

    #region Construction

    internal BreakPattern(string charList, int error)
    {
        _charList = charList;
        _functionName = null;
        _error = error;
    }

    internal BreakPattern(Executive.DeferredCode functionName, int error)
    {
        _charList = "";
        _functionName = functionName;
        _error = error;
    }

    /// <summary>
    /// True when the char list is a static string (no deferred-code evaluation).
    /// Used by Scanner.IsPureTerminal to decide whether failure can be memoized.
    /// </summary>
    internal bool IsStaticCharList => _functionName is null;

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return _functionName is not null
            ? new BreakPattern(_functionName, _error)
            : new BreakPattern(_charList, _error);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        if (_functionName is not null)
        {
            using var profile1 = Profiler.Start4("Break", scan.Exec);
            
            if (!EvaluatedArgument.String(scan, _functionName, _error, out var str))
                return MatchResult.Failure(scan);

            _charList = str;
        }

        var index = scan.Subject.IndexOfAny(_charList.ToCharArray(), scan.CursorPosition);

        if (index < 0)
            return MatchResult.Failure(scan);

        scan.CursorPosition = index;
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

    public override string DebugPattern() => "break";

    #endregion
}