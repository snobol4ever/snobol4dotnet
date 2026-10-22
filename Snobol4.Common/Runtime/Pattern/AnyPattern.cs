using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class AnyPattern : TerminalPattern
{
    #region Members

    private string _charList;
    private readonly Executive.DeferredCode? _functionName;

    #endregion

    #region Construction

    internal AnyPattern(string charList)
    {
        _charList = charList;
        _functionName = null;
    }

    internal AnyPattern(Executive.DeferredCode functionName)
    {
        _charList = "";
        _functionName = functionName;
    }

    #endregion

    #region Methods

    internal override AnyPattern Clone()
    {
        return _functionName is not null
            ? new AnyPattern(_functionName)
            : new AnyPattern(_charList);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Any", scan.Exec);

        if (_functionName != null)
        {
            if (!EvaluatedArgument.String(scan, _functionName, 43, out var str))
                return MatchResult.Failure(scan);
            _charList = str;
        }

        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        if (!_charList.Contains(scan.Subject[scan.CursorPosition]))
            return MatchResult.Failure(scan);
        scan.CursorPosition++;
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

    public override string DebugPattern() => "any";

    #endregion
}