namespace Snobol4.Common;

/// <summary>
/// This class implements the "Shunting Yard Algorithm" to follow the
/// precedence and associativity of  SNOBOL's operators. For more
/// information see:
/// https://aquarchitect.github.io/swift-algorithm-club/Shunting%20Yard/
/// https://literateprograms.org/shunting_yard_algorithm__c_.html
/// https://blog.kallisti.net.nz/2008/02/extension-to-the-shunting-yard-algorithm-to-allow-variable-numbers-of-arguments-to-functions/
/// </summary>
public class Parser
{
    #region Embedded class

    /// <summary>
    /// Class to specify precedence of binary operators
    /// and left or right associativity
    /// </summary>
    internal class Order
    {
        internal Association Associativity { get; }
        internal int Precedence { get; }
        internal bool Unary { get; }

        internal Order(int precedence, Association associativity, bool unary)
        {
            Precedence = precedence;
            Associativity = associativity;
            Unary = unary;
        }

        internal enum Association
        {
            LEFT = 1,
            RIGHT = 2
        }

    }

    #endregion

    #region Members

    private readonly Builder _parent;

    private readonly Stack<Token> _operatorStack = [];

    private static readonly Dictionary<Token.Type, Order> _operators = new()
    {
        { Token.Type.BINARY_EQUAL,new Order(1,Order.Association.RIGHT, false)},
        { Token.Type.BINARY_QUESTION, new Order(2, Order.Association.LEFT, false)},
        { Token.Type.BINARY_AMPERSAND, new Order(3, Order.Association.LEFT, false)},
        { Token.Type.BINARY_PIPE, new Order(4, Order.Association.RIGHT, false)},
        { Token.Type.BINARY_CONCAT, new Order(5, Order.Association.RIGHT, false)},
        { Token.Type.BINARY_AT, new Order(6, Order.Association.RIGHT, false)},
        { Token.Type.BINARY_PLUS, new Order(7, Order.Association.LEFT, false)},
        { Token.Type.BINARY_MINUS, new Order(7, Order.Association.LEFT, false)},
        { Token.Type.BINARY_HASH, new Order(8, Order.Association.LEFT, false)},
        { Token.Type.BINARY_SLASH, new Order(9, Order.Association.LEFT, false)},
        { Token.Type.BINARY_STAR, new Order(10, Order.Association.LEFT, false)},
        { Token.Type.BINARY_PERCENT, new Order(11, Order.Association.LEFT, false)},
        { Token.Type.BINARY_CARET, new Order(12, Order.Association.RIGHT, false)},
        { Token.Type.BINARY_DOLLAR, new Order(13, Order.Association.LEFT, false)},
        { Token.Type.BINARY_PERIOD, new Order(13, Order.Association.LEFT, false)},
        { Token.Type.BINARY_TILDE, new Order(14, Order.Association.RIGHT, false)},
        { Token.Type.UNARY_OPERATOR, new Order(15, Order.Association.RIGHT, true)},
        { Token.Type.UNARY_STAR, new Order(15, Order.Association.RIGHT, true)}
    };

    private static readonly Dictionary<Token.Type, Token.Type> _matchingParenBracket = new()
    {
        { Token.Type.R_ANGLE, Token.Type.L_ANGLE},
        { Token.Type.R_PAREN_CHOICE, Token.Type.L_PAREN_CHOICE},
        { Token.Type.R_PAREN_FUNCTION, Token.Type.L_PAREN_FUNCTION},
        { Token.Type.R_SQUARE, Token.Type.L_SQUARE}
    };

    #endregion

    #region Constructor

    internal Parser(Builder parent)
    {
        _parent = parent;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 
    /// Parse source file
    /// </summary>
    internal bool Parse()
    {
        foreach (var sourceLine in _parent.Code.SourceLines.Where(sourceLine => !sourceLine.Compiled))
            ParseLine(sourceLine);

        foreach (var expression in _parent.ExpressionList.Where(expression => !expression.Parsed))
        {
            _parent.ParseExpression.Add(ShuntYardAlgorithm(expression.ExpressionList));
            expression.Parsed = true;
        }

        return true;
    }

    /// <summary>
    /// Parse the components of a source line:
    /// Body, unconditional goto, success goto, and failure goto
    /// </summary>
    /// <param name="sourceLine">line of source code</param>
    private void ParseLine(SourceLine sourceLine)
    {
        _operatorStack.Clear();
        sourceLine.ParseBody = ShuntYardAlgorithm(sourceLine.LexBody);
        sourceLine.ParseUnconditionalGoto = ShuntYardAlgorithm(sourceLine.LexUnconditionalGoto);
        sourceLine.ParseSuccessGoto = ShuntYardAlgorithm(sourceLine.LexSuccessGoto);
        sourceLine.ParseFailureGoto = ShuntYardAlgorithm(sourceLine.LexFailureGoto);
    }

    /// <summary>
    /// Parse source code line to account for precedence and associativity.
    /// </summary>
    /// <param name="lexTokenList">List of lexemes</param>
    private List<Token> ShuntYardAlgorithm(List<Token> lexTokenList)
    {
        List<Token> outputTokenList = [];
        foreach (var token in lexTokenList)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (token.TokenType)
            {
                // Rule 1: If the token is an operand, output it.
                case Token.Type.EXPRESSION:
                case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
                case Token.Type.IDENTIFIER_FUNCTION:
                //case Token.Type.IDENTIFIER_TABLE:
                case Token.Type.IDENTIFIER:
                case Token.Type.INTEGER:
                case Token.Type.NULL:
                case Token.Type.REAL:
                case Token.Type.STRING:
                    outputTokenList.Add(token);
                    break;

                // Rule 2: If the token is an operator, pop operators from
                // the operator stack and evaluate them with values from the
                // number stack, until we reach one that has a lower
                // precedence (for a left associative operator) or equal
                // or lower (for a right associative operator.) Then push
                // the original operator on the operator stack.
                case Token.Type.BINARY_AMPERSAND:
                case Token.Type.BINARY_AT:
                case Token.Type.BINARY_CARET:
                case Token.Type.BINARY_CONCAT:
                case Token.Type.BINARY_DOLLAR:
                case Token.Type.BINARY_EQUAL:
                case Token.Type.BINARY_HASH:
                case Token.Type.BINARY_MINUS:
                case Token.Type.BINARY_PERCENT:
                case Token.Type.BINARY_PERIOD:
                case Token.Type.BINARY_PIPE:
                case Token.Type.BINARY_PLUS:
                case Token.Type.BINARY_QUESTION:
                case Token.Type.BINARY_SLASH:
                case Token.Type.BINARY_STAR:
                case Token.Type.BINARY_TILDE:
                case Token.Type.UNARY_OPERATOR:
                case Token.Type.UNARY_STAR:
                    while (PopAddCriteria(token))
                        outputTokenList.Add(_operatorStack.Pop());
                    _operatorStack.Push(token);
                    break;

                // Rule 3: If the token is left parenthesis, push it
                case Token.Type.L_ANGLE:
                case Token.Type.L_PAREN_CHOICE:
                case Token.Type.L_PAREN_FUNCTION:
                case Token.Type.L_SQUARE:
                    _operatorStack.Push(token);
                    break;

                // Rule 4: If the token is a right parenthesis/bracket,
                // output the stack until the matching left parenthesis/bracket,
                // then discard it.
                case Token.Type.R_ANGLE:
                case Token.Type.R_PAREN_FUNCTION:
                case Token.Type.R_SQUARE:
                    var match = _matchingParenBracket[token.TokenType];
                    while (_operatorStack.Peek().TokenType != match)
                        outputTokenList.Add(_operatorStack.Pop());
                    _operatorStack.Pop();
                    outputTokenList.Add(token);
                    break;

                case Token.Type.R_PAREN_CHOICE:
                    while (_operatorStack.Peek().TokenType != Token.Type.L_PAREN_CHOICE)
                        outputTokenList.Add(_operatorStack.Pop());
                    _operatorStack.Pop();
                    if (token.IntegerValue > 1)
                    {
                        outputTokenList.Add(token);
                    }
                    break;

                // Rule 5: If the token is a comma,output the
                // stack until there is a left parenthesis/bracket.
                // If a COMMA_CHOICE, output it. Otherwise, discard it.
                case Token.Type.COMMA:
                    while (_operatorStack.Peek().TokenType != Token.Type.L_ANGLE &&
                           _operatorStack.Peek().TokenType != Token.Type.L_PAREN_FUNCTION &&
                           _operatorStack.Peek().TokenType != Token.Type.L_SQUARE)
                        outputTokenList.Add(_operatorStack.Pop());
                    break;

                case Token.Type.COMMA_CHOICE:
                    while (_operatorStack.Peek().TokenType != Token.Type.L_PAREN_CHOICE)
                        outputTokenList.Add(_operatorStack.Pop());
                    outputTokenList.Add(token);
                    break;
            }
        }

        // Rule 6: After all tokens are read, the remaining operators on
        // stack are added to the output list.
        while (_operatorStack.Count > 0)
            outputTokenList.Add(_operatorStack.Pop());
        return outputTokenList;
    }

    private bool PopAddCriteria(Token x)
    {
        // Check for an operator at the top of the operator stack
        if (_operatorStack.Count == 0 || !_operators.TryGetValue(_operatorStack.Peek().TokenType, out _))
            return false;

        return _operators[_operatorStack.Peek().TokenType].Associativity switch
        {
            Order.Association.LEFT => _operators[x.TokenType].Precedence <=
                                      _operators[_operatorStack.Peek().TokenType].Precedence,
            Order.Association.RIGHT => _operators[x.TokenType].Precedence <
                                       _operators[_operatorStack.Peek().TokenType].Precedence,
            _ => false
        };
    }

    #endregion
}