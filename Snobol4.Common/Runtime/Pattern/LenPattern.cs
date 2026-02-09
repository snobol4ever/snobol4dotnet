using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class LenPattern : TerminalPattern
{
    #region Members

    private long _length;
    private readonly Executive.DeferredCode? _functionName;

    #endregion

    #region Constructors

    internal LenPattern(long length)
    {
        _length = length;
        _functionName = null;
    }

    internal LenPattern(Executive.DeferredCode functionName)
    {
        _length = 0;
        _functionName = functionName;
    }

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return _functionName == null
            ? new LenPattern(_length)
            : new LenPattern(_functionName);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Len", scan.Exec);


        if (_functionName != null)
        {
            _functionName(scan.Exec);
            var result = scan.Exec.SystemStack.Pop();
            if (!result.Succeeded || !result.Convert(Executive.VarType.INTEGER, out _, out var n, scan.Exec))
            {
                scan.Exec.LogRuntimeException(43);
                return MatchResult.Failure(scan);
            }
            _length = (long)n;
        }

        scan.CursorPosition += (int)_length;
        return scan.CursorPosition <= scan.Subject.Length
            ? MatchResult.Success(scan)
            : MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

    /// <summary>
    /// Returns a debug string representation of this alternation
    /// </summary>
    /// <returns>A string showing this pattern</returns>
    public override string DebugPattern() => "len";

    #endregion
}