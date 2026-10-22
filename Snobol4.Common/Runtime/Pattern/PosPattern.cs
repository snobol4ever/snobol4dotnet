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
        return _functionName is not null
            ? new PosPattern(_functionName)
            : new PosPattern(_position);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Pos", scan.Exec);

        if (_functionName is not null)
        {
            if (!EvaluatedArgument.Integer(scan, _functionName, 50, 51, out var n))
                return MatchResult.Failure(scan);
            _position = n;
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