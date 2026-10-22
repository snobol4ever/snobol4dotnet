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

    /// <summary>
    /// True when the char list is a static string (no deferred-code evaluation).
    /// Used by Scanner.IsPureTerminal to decide whether failure can be memoized.
    /// </summary>
    internal bool IsStaticCharList => _functionName is null;

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return _functionName is not null
            ? new SpanPattern(_functionName)
            : new SpanPattern(_charList);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Span", scan.Exec);

        if (_functionName is not null)
        {
            if (!EvaluatedArgument.String(scan, _functionName, 56, out var value))
                return MatchResult.Failure(scan);
            _charList = value;
        }

        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

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