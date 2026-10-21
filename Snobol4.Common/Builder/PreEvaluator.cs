namespace Snobol4.Common;

/// <summary>
/// SPITBOL compiles an arithmetic operator, or a call of a function marked for pre-evaluation (svpre in its sources), whose
/// operands are all constants by evaluating it at once. An error in it (1 / 0, LEN(-1), 'a' + 1) is then an error in the statement
/// being compiled: it is reported on the listing page with no column, and nothing runs. This walks the postfix form of each
/// expression of a statement with a stack that holds a constant's value, or null for anything else, and calls the very builtins
/// the run time calls. Only the first error of a statement is reported, as SPITBOL stops compiling the statement at it.
/// </summary>
internal sealed class PreEvaluator(Builder parent)
{
    // The functions SPITBOL pre-evaluates (those its sources mark svfnp), by the lower-case symbol of their entry
    private static readonly HashSet<string> Functions =
    [
        "ln", "any", "cos", "exp", "len", "pos", "sin", "tab", "tan", "atan", "char", "chop", "lpad", "rpad", "rpos", "rtab", "size",
        "span", "sqrt", "arbno", "break", "fence", "remdr", "breakx", "notany", "replace", "reverse", "datatype"
    ];

    // A function name or an array on the stack, waiting for its arguments or subscripts
    private sealed record Mark(string Name);

    private static readonly Mark ArrayMark = new("");

    private readonly Parser _parser = new(parent);

    // The number of the first error found pre-evaluating the statement, or 0 for none: its body, then its gotos
    internal int FirstError(SourceLine line)
    {
        if (parent.Execute is null)
            return 0;
        foreach (var tokens in new[] { line.LexBody, line.LexSuccessGoto, line.LexFailureGoto, line.LexUnconditionalGoto })
            if (FirstError(tokens) is var code and not 0)
                return code;
        return 0;
    }

    private int FirstError(List<Token> lexTokens)
    {
        if (!HasCandidate(lexTokens))
            return 0;
        var postfix = _parser.ToPostfix(lexTokens);
        return postfix is null ? 0 : Walk(postfix);
    }

    // Whether the list holds an operator or call that could be pre-evaluated, or a deferred expression that could hold one
    private bool HasCandidate(List<Token> lexTokens) => lexTokens.Any(token => token.TokenType switch
    {
        Token.Type.BINARY_PLUS or Token.Type.BINARY_MINUS or Token.Type.BINARY_STAR or Token.Type.BINARY_SLASH
            or Token.Type.BINARY_CARET or Token.Type.EXPRESSION => true,
        Token.Type.UNARY_OPERATOR => token.MatchedString is "-" or "+",
        Token.Type.IDENTIFIER_FUNCTION => FunctionEntry(token.MatchedString) is not null,
        _ => false
    });

    private FunctionTableEntry? FunctionEntry(string name)
    {
        var entry = parent.Execute!.FunctionTable[parent.FoldCase(name)];
        return entry is not null && Functions.Contains(entry.Symbol) ? entry : null;
    }

    private int Walk(List<Token> postfix)
    {
        List<object?> stack = [];
        foreach (var token in postfix)
        {
            switch (token.TokenType)
            {
                case Token.Type.INTEGER:
                    stack.Add(new IntegerVar(token.IntegerValue));
                    break;
                case Token.Type.REAL:
                    stack.Add(new RealVar(token.DoubleValue));
                    break;
                case Token.Type.STRING:
                    stack.Add(new StringVar(token.MatchedString));
                    break;
                case Token.Type.NULL:
                    stack.Add(StringVar.Null());
                    break;
                case Token.Type.IDENTIFIER:
                    stack.Add(null);
                    break;

                case Token.Type.EXPRESSION:
                {
                    // a deferred expression is compiled where it stands, and its constant parts are pre-evaluated then
                    var index = int.Parse(token.MatchedString[4..]);
                    if (index < parent.ExpressionList.Count && FirstError(parent.ExpressionList[index].ExpressionList) is var inner and not 0)
                        return inner;
                    stack.Add(null);
                    break;
                }

                case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
                    stack.Add(ArrayMark);
                    break;
                case Token.Type.R_ANGLE:
                case Token.Type.R_SQUARE:
                    // the subscripts and the array under them become one value
                    while (stack.Count > 0 && !ReferenceEquals(stack[^1], ArrayMark))
                        stack.RemoveAt(stack.Count - 1);
                    if (stack.Count == 0)
                        return 0;
                    stack[^1] = null;
                    break;

                case Token.Type.IDENTIFIER_FUNCTION:
                    stack.Add(new Mark(token.MatchedString));
                    break;
                case Token.Type.R_PAREN_FUNCTION:
                {
                    var count = (int)token.IntegerValue;
                    if (stack.Count < count + 1 || stack.Skip(stack.Count - count).Any(item => item is Mark)
                        || stack[stack.Count - count - 1] is not Mark { Name.Length: > 0 } mark)
                        return 0;
                    var arguments = stack.GetRange(stack.Count - count, count);
                    stack.RemoveRange(stack.Count - count - 1, count + 1);
                    var entry = FunctionEntry(mark.Name);
                    if (entry is null || arguments.Any(argument => argument is null))
                    {
                        stack.Add(null);
                        break;
                    }

                    // SPITBOL gives a function the number of arguments it declares: missing ones are null, extra ones are dropped
                    var list = arguments.Cast<Var>().Take(entry.ArgumentCount).ToList();
                    while (list.Count < entry.ArgumentCount)
                        list.Add(StringVar.Null());
                    if (Evaluate(entry, list, out var value) is var code and not 0)
                        return code;
                    stack.Add(value);
                    break;
                }

                // a choice (A,B) is not a constant; each alternative before the last leaves nothing
                case Token.Type.COMMA_CHOICE:
                    if (stack.Count == 0)
                        return 0;
                    stack.RemoveAt(stack.Count - 1);
                    break;
                case Token.Type.R_PAREN_CHOICE:
                    if (stack.Count == 0)
                        return 0;
                    stack[^1] = null;
                    break;

                case Token.Type.BINARY_PLUS or Token.Type.BINARY_MINUS or Token.Type.BINARY_STAR or Token.Type.BINARY_SLASH
                    or Token.Type.BINARY_CARET:
                {
                    if (stack.Count < 2 || stack[^1] is Mark || stack[^2] is Mark)
                        return 0;
                    var right = stack[^1];
                    var left = stack[^2];
                    stack.RemoveRange(stack.Count - 2, 2);
                    var key = token.TokenType switch
                    {
                        Token.Type.BINARY_PLUS => "__+",
                        Token.Type.BINARY_MINUS => "__-",
                        Token.Type.BINARY_STAR => "__*",
                        Token.Type.BINARY_SLASH => "__/",
                        _ => "__^"
                    };
                    var entry = left is null || right is null ? null : parent.Execute!.FunctionTable[key];
                    if (entry is null)
                    {
                        stack.Add(null);
                        break;
                    }

                    if (Evaluate(entry, [(Var)left!, (Var)right!], out var value) is var code and not 0)
                        return code;
                    stack.Add(value);
                    break;
                }

                case Token.Type.UNARY_OPERATOR or Token.Type.UNARY_STAR:
                {
                    if (stack.Count < 1 || stack[^1] is Mark)
                        return 0;
                    var operand = stack[^1];
                    stack.RemoveAt(stack.Count - 1);
                    var key = token.TokenType == Token.Type.UNARY_OPERATOR ? token.MatchedString switch
                    {
                        "-" => "_-",
                        "+" => "_+",
                        _ => null
                    } : null;
                    var entry = key is null || operand is null ? null : parent.Execute!.FunctionTable[key];
                    if (entry is null)
                    {
                        stack.Add(null);
                        break;
                    }

                    if (Evaluate(entry, [(Var)operand!], out var value) is var code and not 0)
                        return code;
                    stack.Add(value);
                    break;
                }

                // every other operator takes two operands and gives a value that is not a constant
                case Token.Type.BINARY_AMPERSAND or Token.Type.BINARY_AT or Token.Type.BINARY_CONCAT or Token.Type.BINARY_DOLLAR
                    or Token.Type.BINARY_EQUAL or Token.Type.BINARY_HASH or Token.Type.BINARY_PERCENT or Token.Type.BINARY_PERIOD
                    or Token.Type.BINARY_PIPE or Token.Type.BINARY_QUESTION or Token.Type.BINARY_TILDE:
                    if (stack.Count < 2 || stack[^1] is Mark || stack[^2] is Mark)
                        return 0;
                    stack.RemoveRange(stack.Count - 2, 2);
                    stack.Add(null);
                    break;
            }
        }

        return 0;
    }

    // The value of the call, or the number of the error it raised
    private int Evaluate(FunctionTableEntry entry, List<Var> arguments, out object? value)
    {
        value = parent.Execute!.PreEvaluate(entry, arguments, out var code);
        return code;
    }
}
