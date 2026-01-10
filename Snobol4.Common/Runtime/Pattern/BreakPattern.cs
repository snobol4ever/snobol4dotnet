namespace Snobol4.Common;

internal class BreakPattern : TerminalPattern
{
    #region Members

    private readonly string _charList;
    private readonly ExpressionVar? _expression;

    #endregion

    #region Construction

    internal BreakPattern(string charList)
    {
        _charList = charList;
        _expression = null;
    }

    internal BreakPattern(ExpressionVar expressionVar)
    {
        _charList = "";
        _expression = expressionVar;
    }

    internal BreakPattern(string charList, ExpressionVar? expressionVar)
    {
        _charList = charList;
        _expression = expressionVar;
    }

    #endregion

    #region Methods

    internal override Pattern Clone()
    {
        return new BreakPattern(_charList, _expression);
    }

    internal override MatchResult Scan(int node, Scanner scan)
    {
        var charList = _charList;

        if (_expression != null)
        {
            _expression.FunctionName(scan.Exec);
            var charVar = scan.Exec.SystemStack.Pop();

            if (!charVar.Convert(Executive.VarType.STRING, out _, out var str, scan.Exec) || string.IsNullOrEmpty((string)str))
            {
                scan.Exec.LogRuntimeException(59);
                return MatchResult.Failure(scan);
            }
            charList = (string)str;
        }

        if (scan.Subject.Length == 0)
            return MatchResult.Failure(scan);

        var index = scan.Subject.IndexOfAny(charList.ToCharArray(), scan.CursorPosition);

        if (index < 0)
            return MatchResult.Failure(scan);

        scan.CursorPosition = index;
        return MatchResult.Success(scan);
    }

    #endregion
}