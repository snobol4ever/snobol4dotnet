namespace Snobol4.Common;

/// <summary>
/// SPITBOL evaluates the operands of an expression from left to right, and an input-associated variable reads its record when it is
/// evaluated: '1 ' X ' ' X reads the first record into the first X. The port reads an input association when an operator or a call
/// takes its operands, which is the same order unless an operand stands on the stack while the operands after it are read by an
/// operator of their own (concatenation is right associative, so in A ' ' B the inner concatenation reads B before the outer reads A).
/// This pass puts an OPERAND_VALUE token, a read of the value on top of the stack, right after such a variable: one that can be
/// input-associated (the compiler sees it named: INPUT, TERMINAL, a variable written .X, a string INPUT is called with), whose value
/// is taken (not the subject of =, the target of . or $, the operand of the name operator) and that has a variable or a call between
/// it and the operator that takes it. A postfix whose shape it does not know is left as it is.
/// </summary>
internal static class OperandReadOrder
{
    // What stands on the simulated stack: the index in the postfix of the variable it is, or -1 for any other value, or a mark
    private const int Other = -1, ArrayMark = -2, FunctionMark = -3, ChoicePart = -4;

    internal static List<Token> Insert(List<Token> postfix, ISet<string> candidates, Func<string, string> fold)
    {
        if (candidates.Count == 0 || !postfix.Any(t => t.TokenType == Token.Type.IDENTIFIER && candidates.Contains(fold(t.MatchedString))))
            return postfix;

        var reads = new SortedSet<int>();
        var stack = new List<int>();

        // the operand taken at `position` (0 the first) of `count` by the operator at `consumer`, whose value is taken unless `name`
        bool Take(int count, int consumer, Func<int, bool> name)
        {
            if (stack.Count < count)
                return false;
            for (var position = 0; position < count; position++)
            {
                var operand = stack[stack.Count - count + position];
                if (operand is ArrayMark or FunctionMark or ChoicePart)
                    return false;
                if (operand < 0 || name(position) || !candidates.Contains(fold(postfix[operand].MatchedString)))
                    continue;
                for (var k = operand + 1; k < consumer; k++)
                    if (postfix[k].TokenType is Token.Type.IDENTIFIER or Token.Type.IDENTIFIER_FUNCTION)
                    {
                        reads.Add(operand);
                        break;
                    }
            }

            stack.RemoveRange(stack.Count - count, count);
            return true;
        }

        for (var i = 0; i < postfix.Count; i++)
        {
            var t = postfix[i];
            switch (t.TokenType)
            {
                case Token.Type.IDENTIFIER:
                    stack.Add(i);
                    break;
                case Token.Type.STRING or Token.Type.INTEGER or Token.Type.REAL or Token.Type.NULL or Token.Type.EXPRESSION:
                    stack.Add(Other);
                    break;
                case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
                    stack.Add(ArrayMark);
                    break;
                case Token.Type.IDENTIFIER_FUNCTION:
                    stack.Add(FunctionMark);
                    break;
                case Token.Type.L_PAREN_CHOICE or Token.Type.SUBJECT_VALUE:
                    break;

                case Token.Type.R_ANGLE or Token.Type.R_SQUARE:
                {
                    // the subscripts are values; under them the array's mark, or the value the brackets follow
                    if (!Take((int)t.IntegerValue, i, _ => false) || stack.Count == 0 || stack[^1] is FunctionMark or ChoicePart)
                        return postfix;
                    stack[^1] = Other;
                    break;
                }

                case Token.Type.R_PAREN_FUNCTION:
                    if (!Take((int)t.IntegerValue, i, _ => false) || stack.Count == 0 || stack[^1] != FunctionMark)
                        return postfix;
                    stack[^1] = Other;
                    break;

                case Token.Type.COMMA_CHOICE:
                    if (!Take(1, i, _ => false))
                        return postfix;
                    stack.Add(ChoicePart);
                    break;

                case Token.Type.R_PAREN_CHOICE:
                    if (!Take(1, i, _ => false))
                        return postfix;
                    while (stack.Count > 0 && stack[^1] == ChoicePart)
                        stack.RemoveAt(stack.Count - 1);
                    stack.Add(Other);
                    break;

                case Token.Type.UNARY_OPERATOR:
                {
                    // the name operator takes a name; the others a value
                    var nameOperator = t.MatchedString == ".";
                    if (!Take(1, i, _ => nameOperator))
                        return postfix;
                    stack.Add(Other);
                    break;
                }

                case Token.Type.BINARY_EQUAL or Token.Type.BINARY_QUESTION:
                    // the subject of an assignment, a match or a replacement is a name
                    if (!Take(2, i, position => position == 0))
                        return postfix;
                    stack.Add(Other);
                    break;

                case Token.Type.BINARY_PERIOD or Token.Type.BINARY_DOLLAR:
                    // the target of a capture is a name
                    if (!Take(2, i, position => position == 1))
                        return postfix;
                    stack.Add(Other);
                    break;

                case Token.Type.BINARY_AMPERSAND or Token.Type.BINARY_AT or Token.Type.BINARY_CARET or Token.Type.BINARY_CONCAT
                    or Token.Type.BINARY_HASH or Token.Type.BINARY_MINUS or Token.Type.BINARY_PERCENT or Token.Type.BINARY_PIPE
                    or Token.Type.BINARY_PLUS or Token.Type.BINARY_SLASH or Token.Type.BINARY_STAR or Token.Type.BINARY_TILDE:
                    if (!Take(2, i, _ => false))
                        return postfix;
                    stack.Add(Other);
                    break;

                default:
                    return postfix;
            }
        }

        if (reads.Count == 0)
            return postfix;
        var result = new List<Token>(postfix.Count + reads.Count);
        for (var i = 0; i < postfix.Count; i++)
        {
            result.Add(postfix[i]);
            if (reads.Contains(i))
                result.Add(new Token(Token.Type.OPERAND_VALUE, "", 0));
        }

        return result;
    }

    /// <summary>
    /// The variables of the statements that can be input-associated, as the compiler sees them named: INPUT and TERMINAL, a variable
    /// written as the operand of the name operator (.X), a string INPUT is called with first (INPUT('X', 5, 'f')).
    /// </summary>
    internal static void Collect(IEnumerable<List<Token>> lexBodies, ISet<string> candidates, Func<string, string> fold)
    {
        foreach (var body in lexBodies)
            for (var i = 0; i + 1 < body.Count; i++)
            {
                if (body[i] is { TokenType: Token.Type.UNARY_OPERATOR, MatchedString: "." } && body[i + 1].TokenType == Token.Type.IDENTIFIER)
                    candidates.Add(fold(body[i + 1].MatchedString));
                else if (body[i].TokenType == Token.Type.IDENTIFIER_FUNCTION && fold(body[i].MatchedString) == fold("INPUT"))
                {
                    var j = i + 1;
                    while (j < body.Count && body[j].TokenType is Token.Type.L_PAREN_FUNCTION or Token.Type.SPACE)
                        j++;
                    if (j < body.Count && body[j].TokenType == Token.Type.STRING)
                        candidates.Add(fold(body[j].MatchedString));
                }
            }
    }
}
