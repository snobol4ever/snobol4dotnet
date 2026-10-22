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


        if (_functionName is not null)
        {
            if (!EvaluatedArgument.Integer(scan, _functionName, 47, 48, out var n))
                return MatchResult.Failure(scan);
            _length = n;
        }

        scan.CursorPosition += (int)_length;
        return scan.CursorPosition <= scan.Subject.Length
            ? MatchResult.Success(scan)
            : MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

                    public override string DebugPattern() => "len";

    #endregion
}