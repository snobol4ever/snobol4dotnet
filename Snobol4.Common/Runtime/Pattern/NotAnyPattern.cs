using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class NotAnyPattern : TerminalPattern
{
    #region Members

    private string _charList;
    private readonly Executive.DeferredCode? _functionName;

    #endregion

    #region Constructors

    internal NotAnyPattern(string charList)
    {
        _charList = charList;
        _functionName = null;
    }

    internal NotAnyPattern(Executive.DeferredCode functionName)
    {
        _charList = "";
        _functionName = functionName;
    }

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return _functionName is not null
            ? new NotAnyPattern(_functionName)
            : new NotAnyPattern(_charList);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("NotAny", scan.Exec);

        if (_functionName != null)
        {
            if (!EvaluatedArgument.String(scan, _functionName, 49, out var str))
                return MatchResult.Failure(scan);
            _charList = str;
        }

        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        if (_charList.Contains(scan.Subject[scan.CursorPosition]))
            return MatchResult.Failure(scan);
        scan.CursorPosition++;
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

    public override string DebugPattern() => "notany";

    #endregion
}