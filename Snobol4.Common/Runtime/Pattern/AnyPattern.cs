using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugString()}")]
internal class AnyPattern : TerminalPattern
{
    #region Members

    private string _charList;
    private readonly ExpressionVar? _expression;

    #endregion

    #region Construction

    internal AnyPattern(string charList)
    {
        _charList = charList;
    }

    internal AnyPattern(ExpressionVar expression)
    {
        _charList = "";
        _expression = expression;
    }

    #endregion

    #region Methods

    internal override AnyPattern Clone()
    {
        return new AnyPattern(_charList);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        if (scan.CursorPosition >= scan.Subject.Length)
            return MatchResult.Failure(scan);

        if (_expression == null)
            return _charList.Contains(scan.Subject[scan.CursorPosition++])
                ? MatchResult.Success(scan)
                : MatchResult.Failure(scan);

        _expression.FunctionName(scan.Exec);
        var result = scan.Exec.SystemStack.Pop();

        if (!result.Succeeded || !result.Convert(Executive.VarType.STRING, out _, out var value, scan.Exec))
        {
            scan.Exec.LogRuntimeException(43);
            return MatchResult.Failure(scan);
        }

        _charList = (string)value;
        
        return _charList.Contains(scan.Subject[scan.CursorPosition++]) ? MatchResult.Success(scan) : MatchResult.Failure(scan);
    }

    #endregion

    #region Debugging

    internal string DebugString()
    {
        return $"ANY PATTERN [{_charList}]";
    }
    #endregion
}