namespace Snobol4.Common;

public class DeferredExpression
{
    internal List<Token> ExpressionList;
    public bool Parsed;
    // the line, as &LINE reads it, of the statement the expression is written in: a function it calls returns to that line
    internal int CodeLine;

    internal DeferredExpression(List<Token> tokens)
    {
        ExpressionList = tokens;
        Parsed = false;
    }
}

