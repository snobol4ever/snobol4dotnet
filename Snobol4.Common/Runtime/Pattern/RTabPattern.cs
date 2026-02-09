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
        return _functionName != null
            ? new RTabPattern(_functionName)
            : new RTabPattern(_position);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("RTab", scan.Exec);

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

        var targetPosition = scan.Subject.Length - _position;

        if (scan.CursorPosition > targetPosition || targetPosition < 0)
            return MatchResult.Failure(scan);

        scan.CursorPosition = (int)targetPosition;
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

    /// <summary>
    /// Returns a debug string representation of this pattern for diagnostic purposes.
    /// </summary>
    /// <returns>A string in the format "rtab(&lt;n&gt;)" where &lt;n&gt; is the target position from the end.</returns>
    /// <remarks>
    /// This method is used by the debugger display attribute and diagnostic tools
    /// to provide a concise, human-readable representation of the pattern.
    /// </remarks>
    public override string DebugPattern() => "rtab";

    #endregion
}