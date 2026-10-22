using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class RPosPattern : TerminalPattern
{
    #region Members

                private long _position;
    private readonly Executive.DeferredCode? _functionName;

    #endregion

    #region Construction
    internal RPosPattern(long position)
    {
        _position = position;
        _functionName = null;
    }

    internal RPosPattern(Executive.DeferredCode functionName)
    {
        _position = 0;
        _functionName = functionName;
    }
    #endregion

    #region Internal Methods

    internal override Pattern Clone()
    {
        return _functionName is not null
            ? new RPosPattern(_functionName)
            : new RPosPattern(_position);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("RPos", scan.Exec);

        if (_functionName is not null)
        {
            if (!EvaluatedArgument.Integer(scan, _functionName, 52, 53, out var n))
                return MatchResult.Failure(scan);
            _position = n;
        }
        
        return _position == scan.Subject.Length - scan.CursorPosition
            ? MatchResult.Success(scan)
            : MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "rpos";

    #endregion
}
