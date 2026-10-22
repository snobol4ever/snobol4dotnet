using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class RTabPattern : TerminalPattern
{
    #region Members

    private long _position;
    private readonly Executive.DeferredCode? _functionName;

    #endregion

    #region Construction

    internal RTabPattern(long position)
    {
        _position = position;
        _functionName = null;
    }

    internal RTabPattern(Executive.DeferredCode functionName)
    {
        _position = 0;
        _functionName = functionName;
    }

    #endregion

    #region Internal Methods

    internal override Pattern Clone()
    {
        return _functionName is not null
            ? new RTabPattern(_functionName)
            : new RTabPattern(_position);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("RTab", scan.Exec);

        if (_functionName is not null)
        {
            if (!EvaluatedArgument.Integer(scan, _functionName, 54, 55, out var n))
                return MatchResult.Failure(scan);
            _position = n;
        }

        var targetPosition = scan.Subject.Length - _position;

        if (scan.CursorPosition > targetPosition || targetPosition < 0)
            return MatchResult.Failure(scan);

        scan.CursorPosition = (int)targetPosition;
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "rtab";

    #endregion
}