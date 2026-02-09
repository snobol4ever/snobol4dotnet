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

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return _functionName != null
            ? new BreakPattern(_functionName, _error)
            : new BreakPattern(_charList, _error);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        if (_functionName != null)
        {
            using var profile1 = Profiler.Start4("Break", scan.Exec);
            
            _functionName(scan.Exec);
            var result = scan.Exec.SystemStack.Pop();

            if (!result.Convert(Executive.VarType.STRING, out _, out var str, scan.Exec) || string.IsNullOrEmpty((string)str))
            {
                scan.Exec.LogRuntimeException(_error);
                return MatchResult.Failure(scan);
            }

            _charList = (string)str;
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