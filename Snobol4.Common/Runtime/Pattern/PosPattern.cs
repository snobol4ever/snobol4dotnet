using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class PosPattern : TerminalPattern
{
    #region Internal Members

                private long _position;
    private readonly Executive.DeferredCode? _functionName;

    #endregion

    #region Construction

    internal PosPattern(long position)
    {
        _position = position;
        _functionName = null;
    }

    internal PosPattern(Executive.DeferredCode functionName)
    {
        _position = 0;
        _functionName = functionName;
    }

    #endregion

    #region Internal Methods

    internal override Pattern Clone()
    {
        return _functionName != null
            ? new RPosPattern(_functionName)
            : new RPosPattern(_position);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Pos", scan.Exec);

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

        return _position == scan.CursorPosition
            ? MatchResult.Success(scan)
            : MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "pos";

    #endregion
}