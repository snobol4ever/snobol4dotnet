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
        return _functionName != null
            ? new NotAnyPattern(_functionName)
            : new NotAnyPattern(_charList);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("NotAny", scan.Exec);

        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        if (_functionName == null)
        {
            return !_charList.Contains(scan.Subject[scan.CursorPosition++])
                ? MatchResult.Success(scan)
                : MatchResult.Failure(scan);
        }

        _functionName(scan.Exec);
        var result = scan.Exec.SystemStack.Pop();

        if (!result.Succeeded || !result.Convert(Executive.VarType.STRING, out _, out var str, scan.Exec))
        {
            scan.Exec.LogRuntimeException(49);
            return MatchResult.Failure(scan);
        }

        _charList = (string)str;
        return !_charList.Contains(scan.Subject[scan.CursorPosition++]) 
            ? MatchResult.Success(scan) 
            : MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

    public override string DebugPattern() => "notany";

    #endregion
}