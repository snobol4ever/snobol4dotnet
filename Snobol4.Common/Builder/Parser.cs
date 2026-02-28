namespace Snobol4.Common;

/// <summary>
/// Implements a parser for SNOBOL4 expressions using Dijkstra's shunting yard algorithm.
/// Converts infix notation expressions into postfix (Reverse Polish Notation) for evaluation.
/// </summary>
/// <remarks>
/// The shunting yard algorithm is a method for parsing mathematical expressions specified in infix notation.
/// It can produce either a postfix notation string (Reverse Polish notation) or an abstract syntax tree.
/// The algorithm was invented by Edsger Dijkstra and named the "shunting yard" algorithm because its operation
/// resembles that of a railroad shunting yard.
/// </remarks>
public class Parser
{
    #region Embedded class

    /// <summary>
    /// Defines the precedence and associativity properties for operators in the shunting yard algorithm.
    /// </summary>
    internal class Order
    {
        /// <summary>
        /// Gets the associativity of the operator (left-to-right or right-to-left).
        /// </summary>
        internal Association Associativity { get; }
        
        /// <summary>
        /// Gets the precedence level of the operator. Higher values indicate higher precedence.
        /// </summary>
        internal int Precedence { get; }
        
        /// <summary>
        /// Gets a value indicating whether the operator is unary.
        /// </summary>
        internal bool Unary { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Order"/> class.
        /// </summary>
        /// <param name="precedence">The precedence level of the operator.</param>
        /// <param name="associativity">The associativity direction of the operator.</param>
        /// <param name="unary">Indicates whether the operator is unary.</param>
        internal Order(int precedence, Association associativity, bool unary)
        {
            Precedence = precedence;
            Associativity = associativity;
            Unary = unary;
        }

        /// <summary>
        /// Defines the associativity direction for operators in expression parsing.
        /// </summary>
        internal enum Association
        {
            /// <summary>
            /// Left-to-right associativity. Operators with the same precedence are evaluated from left to right.
            /// </summary>
            LEFT = 1,
            
            /// <summary>
            /// Right-to-left associativity. Operators with the same precedence are evaluated from right to left.
            /// </summary>
            RIGHT = 2
        }

    }

    #endregion

    #region Members

    /// <summary>
    /// Reference to the parent builder that owns this parser.
    /// </summary>
    private readonly Builder _parent;

    /// <summary>
    /// The operator stack used by the shunting yard algorithm to hold operators and delimiters
    /// while converting from infix to postfix notation.
    /// </summary>
    private readonly Stack<Token> _operatorStack = [];

    /// <summary>
    /// Defines the precedence and associativity for all binary and unary operators.
    /// Used by the shunting yard algorithm to determine operator ordering.
    /// </summary>
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

    /// <summary>
    /// Maps right delimiters (closing brackets/parentheses) to their corresponding left delimiters.
    /// Used to validate balanced delimiters during parsing.
    /// </summary>
    private static readonly Dictionary<Token.Type, Token.Type> _matchingParenBracket = new()
    {
        { Token.Type.R_ANGLE, Token.Type.L_ANGLE},
        { Token.Type.R_PAREN_CHOICE, Token.Type.L_PAREN_CHOICE},
        { Token.Type.R_PAREN_FUNCTION, Token.Type.L_PAREN_FUNCTION},
        { Token.Type.R_SQUARE, Token.Type.L_SQUARE}
    };

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Parser"/> class.
    /// </summary>
    /// <param name="parent">The builder instance that owns this parser.</param>
    internal Parser(Builder parent)
    {
        _parent = parent;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Parses all uncompiled source lines and unparsed expressions in the parent builder.
    /// Applies Dijkstra's shunting yard algorithm to convert infix expressions to postfix notation.
    /// </summary>
    /// <returns>
    /// <c>true</c> if parsing completed successfully; <c>false</c> if a malformed expression was encountered.
    /// </returns>
    internal bool Parse()
    {
        try
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
        catch (InvalidOperationException)
        {
            // Stack operation failed - indicates malformed expression
            return false;
        }
    }

    /// <summary>
    /// Parses a single source line by applying the shunting yard algorithm to all its components
    /// (body, unconditional goto, success goto, and failure goto).
    /// </summary>
    /// <param name="sourceLine">The source line to parse.</param>
    private void ParseLine(SourceLine sourceLine)
    {
        _operatorStack.Clear();
        sourceLine.ParseBody = ShuntYardAlgorithm(sourceLine.LexBody);
        sourceLine.ParseUnconditionalGoto = ShuntYardAlgorithm(sourceLine.LexUnconditionalGoto);
        sourceLine.ParseSuccessGoto = ShuntYardAlgorithm(sourceLine.LexSuccessGoto);
        sourceLine.ParseFailureGoto = ShuntYardAlgorithm(sourceLine.LexFailureGoto);
    }

    /// <summary>
    /// Implements Dijkstra's shunting yard algorithm to convert infix token notation to postfix (RPN).
    /// </summary>
    /// <param name="lexTokenList">The list of tokens in infix notation.</param>
    /// <returns>A list of tokens in postfix notation (Reverse Polish Notation).</returns>
    /// <remarks>
    /// The algorithm processes tokens sequentially, using an operator stack to maintain operators
    /// and delimiters according to their precedence and associativity rules. The resulting postfix
    /// notation eliminates the need for parentheses and makes evaluation straightforward using a stack.
    /// </remarks>
    private List<Token> ShuntYardAlgorithm(List<Token> lexTokenList)
    {
        List<Token> outputTokenList = [];
        foreach (var token in lexTokenList)
        {
            switch (token.TokenType)
            {
                case var type when IsOperand(type):
                    // Rule 1: If the token is an operand, output it.
                    outputTokenList.Add(token);
                    break;

                case var type when IsOperator(type):
                    // Rule 2: Pop and output operators based on precedence and associativity
                    HandleOperator(token, outputTokenList);
                    break;

                case var type when IsLeftDelimiter(type):
                    // Rule 3: If the token is left parenthesis/bracket, push it
                    _operatorStack.Push(token);
                    break;

                case Token.Type.R_PAREN_CHOICE:
                    HandleRightParenChoice(token, outputTokenList);
                    break;

                case var type when IsRightDelimiter(type):
                    // Rule 4: Handle right parenthesis/bracket
                    HandleRightDelimiter(token, outputTokenList);
                    break;

                case Token.Type.COMMA:
                    // Rule 5: Output stack until left parenthesis/bracket
                    HandleComma(outputTokenList);
                    break;

                case Token.Type.COMMA_CHOICE:
                    HandleCommaChoice(outputTokenList);
                    break;
            }
        }

        // Rule 6: After all tokens are read, output remaining operators
        while (_operatorStack.Count > 0)
            outputTokenList.Add(_operatorStack.Pop());
            
        return outputTokenList;
    }

    /// <summary>
    /// Determines whether the specified token type represents an operand (value or identifier).
    /// </summary>
    /// <param name="type">The token type to check.</param>
    /// <returns><c>true</c> if the token type is an operand; otherwise, <c>false</c>.</returns>
    private static bool IsOperand(Token.Type type) => type switch
    {
        Token.Type.EXPRESSION or
        Token.Type.IDENTIFIER_ARRAY_OR_TABLE or
        Token.Type.IDENTIFIER_FUNCTION or
        Token.Type.IDENTIFIER or
        Token.Type.INTEGER or
        Token.Type.NULL or
        Token.Type.REAL or
        Token.Type.STRING => true,
        _ => false
    };

    /// <summary>
    /// Determines whether the specified token type represents a binary or unary operator.
    /// </summary>
    /// <param name="type">The token type to check.</param>
    /// <returns><c>true</c> if the token type is an operator; otherwise, <c>false</c>.</returns>
    private static bool IsOperator(Token.Type type) => _operators.ContainsKey(type);

    /// <summary>
    /// Determines whether the specified token type represents a left delimiter (opening bracket/parenthesis).
    /// </summary>
    /// <param name="type">The token type to check.</param>
    /// <returns><c>true</c> if the token type is a left delimiter; otherwise, <c>false</c>.</returns>
    private static bool IsLeftDelimiter(Token.Type type) => type switch
    {
        Token.Type.L_ANGLE or
        Token.Type.L_PAREN_CHOICE or
        Token.Type.L_PAREN_FUNCTION or
        Token.Type.L_SQUARE => true,
        _ => false
    };

    /// <summary>
    /// Determines whether the specified token type represents a right delimiter (closing bracket/parenthesis).
    /// </summary>
    /// <param name="type">The token type to check.</param>
    /// <returns><c>true</c> if the token type is a right delimiter; otherwise, <c>false</c>.</returns>
    private static bool IsRightDelimiter(Token.Type type) => type switch
    {
        Token.Type.R_ANGLE or
        Token.Type.R_PAREN_FUNCTION or
        Token.Type.R_SQUARE => true,
        _ => false
    };

    /// <summary>
    /// Handles an operator token by popping higher or equal precedence operators from the stack
    /// (according to associativity rules) before pushing the current operator.
    /// </summary>
    /// <param name="token">The operator token to handle.</param>
    /// <param name="outputTokenList">The output list where tokens are added in postfix order.</param>
    private void HandleOperator(Token token, List<Token> outputTokenList)
    {
        while (ShouldPopOperator(token))
            outputTokenList.Add(_operatorStack.Pop());
        _operatorStack.Push(token);
    }

    /// <summary>
    /// Handles a right delimiter by popping operators from the stack until the matching left delimiter is found.
    /// </summary>
    /// <param name="token">The right delimiter token to handle.</param>
    /// <param name="outputTokenList">The output list where tokens are added in postfix order.</param>
    /// <exception cref="InvalidOperationException">Thrown when a matching left delimiter is not found (mismatched delimiters).</exception>
    private void HandleRightDelimiter(Token token, List<Token> outputTokenList)
    {
        var matchingLeft = _matchingParenBracket[token.TokenType];
        while (_operatorStack.Count > 0 && _operatorStack.Peek().TokenType != matchingLeft)
            outputTokenList.Add(_operatorStack.Pop());
            
        if (_operatorStack.Count == 0)
            throw new InvalidOperationException($"Mismatched delimiter: {token.TokenType}");
            
        _operatorStack.Pop();
        outputTokenList.Add(token);
    }

    /// <summary>
    /// Handles a right parenthesis for choice expressions by popping operators until the matching left parenthesis.
    /// </summary>
    /// <param name="token">The right parenthesis choice token to handle.</param>
    /// <param name="outputTokenList">The output list where tokens are added in postfix order.</param>
    /// <exception cref="InvalidOperationException">Thrown when a matching left parenthesis is not found.</exception>
    private void HandleRightParenChoice(Token token, List<Token> outputTokenList)
    {
        while (_operatorStack.Count > 0 && _operatorStack.Peek().TokenType != Token.Type.L_PAREN_CHOICE)
            outputTokenList.Add(_operatorStack.Pop());
            
        if (_operatorStack.Count == 0)
            throw new InvalidOperationException($"Mismatched parenthesis: {token.TokenType}");
            
        _operatorStack.Pop();
        if (token.IntegerValue > 1)
            outputTokenList.Add(token);
    }

    /// <summary>
    /// Handles a comma token by popping operators from the stack until a left delimiter is encountered.
    /// </summary>
    /// <param name="outputTokenList">The output list where tokens are added in postfix order.</param>
    private void HandleComma(List<Token> outputTokenList)
    {
        while (_operatorStack.Count > 0 &&
               _operatorStack.Peek().TokenType is not (Token.Type.L_ANGLE or Token.Type.L_PAREN_FUNCTION or Token.Type.L_SQUARE))
            outputTokenList.Add(_operatorStack.Pop());
    }

    /// <summary>
    /// Handles a comma in a choice expression by popping operators until a left parenthesis choice is found,
    /// then outputs the comma choice token.
    /// </summary>
    /// <param name="outputTokenList">The output list where tokens are added in postfix order.</param>
    private void HandleCommaChoice(List<Token> outputTokenList)
    {
        while (_operatorStack.Count > 0 && _operatorStack.Peek().TokenType != Token.Type.L_PAREN_CHOICE)
            outputTokenList.Add(_operatorStack.Pop());
        outputTokenList.Add(new Token { TokenType = Token.Type.COMMA_CHOICE });
    }

    /// <summary>
    /// Determines whether an operator should be popped from the operator stack based on
    /// precedence and associativity rules of the shunting yard algorithm.
    /// </summary>
    /// <param name="currentOperator">The operator token being processed.</param>
    /// <returns>
    /// <c>true</c> if the operator at the top of the stack should be popped; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// For left-associative operators, pops if the current operator has less than or equal precedence.
    /// For right-associative operators, pops if the current operator has strictly less precedence.
    /// </remarks>
    private bool ShouldPopOperator(Token currentOperator)
    {
        if (_operatorStack.Count == 0)
            return false;

        var topToken = _operatorStack.Peek().TokenType;
        if (!_operators.TryGetValue(topToken, out var topOrder))
            return false;

        if (!_operators.TryGetValue(currentOperator.TokenType, out var currentOrder))
            return false;

        return currentOrder.Associativity switch
        {
            Order.Association.LEFT => currentOrder.Precedence <= topOrder.Precedence,
            Order.Association.RIGHT => currentOrder.Precedence < topOrder.Precedence,
            _ => false
        };
    }

    #endregion
}