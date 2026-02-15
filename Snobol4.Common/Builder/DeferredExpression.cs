namespace Snobol4.Common;

public class DeferredExpression
{
    internal List<Token> ExpressionList;
    public bool Parsed;

    internal DeferredExpression(List<Token> tokens)
    {
        ExpressionList = tokens;
        Parsed = false;
    }
}

