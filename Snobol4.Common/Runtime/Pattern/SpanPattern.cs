using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class SpanPattern : TerminalPattern
{
    #region Members

    private string _charList;
    public Executive.DeferredCode? _functionName;

    #endregion


    #region Constructors

    internal SpanPattern(string charList)
    {
        _charList = charList;
        _functionName = null;
    }

    internal SpanPattern(Executive.DeferredCode functionName)
    {
        _charList = "";
        _functionName = functionName;
    }

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return _functionName != null
            ? new SpanPattern(_functionName)
            : new SpanPattern(_charList);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Span", scan.Exec);

        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        if (_functionName != null)
        {
            _functionName(scan.Exec);
            var result = scan.Exec.SystemStack.Pop();

            if (!result.Succeeded || !result.Convert(Executive.VarType.STRING, out _, out var value, scan.Exec))
            {
                scan.Exec.LogRuntimeException(56);
                return MatchResult.Failure(scan);
            }

            _charList = (string)value;
        }

        if (string.IsNullOrEmpty(_charList))
        {
            scan.CursorPosition++;
            return MatchResult.Success(scan);
        }

        var match = false;

        while (scan.CursorPosition < scan.Subject.Length && _charList.Contains(scan.Subject[scan.CursorPosition]))
        {
            ++scan.CursorPosition;
            match = true;
        }

        return match ? MatchResult.Success(scan) : MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

    public override string DebugPattern() => "span";

    #endregion
}