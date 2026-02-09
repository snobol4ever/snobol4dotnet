using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class TabPattern : TerminalPattern
{
    #region Members

    private long _position;
    private readonly Executive.DeferredCode? _functionName;

    #endregion

    #region Construction

    internal TabPattern(long position)
    {
        _position = position;
        _functionName = null;
    }

    internal TabPattern(Executive.DeferredCode functionName)
    {
        _position = 0;
        _functionName = functionName;
    }

    #endregion

    #region Internal Methods

    internal override Pattern Clone()
    {
        return _functionName != null
            ? new TabPattern(_functionName)
            : new TabPattern(_position);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Tab", scan.Exec);

        if (_functionName != null)
        {
            _functionName(scan.Exec);
            var result = scan.Exec.SystemStack.Pop();
            if (!result.Succeeded || !result.Convert(Executive.VarType.INTEGER, out _, out var n, scan.Exec))
            {
                scan.Exec.LogRuntimeException(43);
                return MatchResult.Failure(scan);
            }
            _position = (long)n;
        }

        if (scan.CursorPosition > _position || _position > scan.Subject.Length)
            return MatchResult.Failure(scan);

        scan.CursorPosition = (int)_position;
        return MatchResult.Success(scan);
    }

    #endregion

    public override string DebugPattern() => "tab";

}