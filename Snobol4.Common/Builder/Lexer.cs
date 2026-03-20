using System.Text.RegularExpressions;
using static Snobol4.Common.CompiledRegex;

namespace Snobol4.Common;

/// <summary>
/// Implements a lexical analyzer for SNOBOL4 source code.
/// Tokenizes source lines into lexemes that can be parsed and compiled.
/// </summary>
/// <remarks>
/// The lexer uses a deterministic finite automaton (DFA) to scan source text and identify tokens
/// such as identifiers, literals, operators, and delimiters. It handles SNOBOL4-specific features
/// including implicit operators, goto statements, and deferred expression evaluation.
/// </remarks>
public partial class Lexer
{
    #region Members

    /// <summary>
    /// Represents an entry in the bracket stack used to track nested delimiters during lexical analysis.
    /// </summary>
    /// <param name="Bracket">The bracket character (e.g., "(", "<", "[").</param>
    /// <param name="Context">The token type context for this bracket.</param>
    private readonly record struct BracketStackEntry(string Bracket, Token.Type Context);

    // Lexical analysis state variables
    
    /// <summary>
    /// Indicates whether a colon has been found in the current line (marking the start of goto statements).
    /// </summary>
    private bool _colonFound;
    
    /// <summary>
    /// Indicates whether an equal sign has been found (for assignment or pattern replacement).
    /// </summary>
    private bool _equalFound;
    
    /// <summary>
    /// Indicates whether an implicit pattern match operator has been inserted.
    /// </summary>
    private bool _patternMatchFound;
    
    /// <summary>
    /// The position in the lexeme list where the colon was found.
    /// </summary>
    private int _colonPosition;
    
    /// <summary>
    /// The current cursor position in the source line being analyzed.
    /// </summary>
    private int _cursorCurrent;
    
    /// <summary>
    /// The end position of the failure goto expression in the lexeme list.
    /// </summary>
    private int _failureGotoEnd;
    
    /// <summary>
    /// The start position of the failure goto expression in the lexeme list.
    /// </summary>
    private int _failureGotoStart;
    
    /// <summary>
    /// The current state of the deterministic finite automaton (DFA) used for tokenization.
    /// </summary>
    private int _state;
    
    /// <summary>
    /// The end position of the success goto expression in the lexeme list.
    /// </summary>
    private int _successGotoEnd;
    
    /// <summary>
    /// The start position of the success goto expression in the lexeme list.
    /// </summary>
    private int _successGotoStart;
    
    /// <summary>
    /// The end position of the unconditional goto expression in the lexeme list.
    /// </summary>
    private int _unconditionalGotoEnd;
    
    /// <summary>
    /// The start position of the unconditional goto expression in the lexeme list.
    /// </summary>
    private int _unconditionalGotoStart;
    
    /// <summary>
    /// Reference to the parent builder that owns this lexer.
    /// </summary>
    private readonly Builder _parent;
    
    /// <summary>
    /// The initial DFA state for lexical analysis (1 = statement; 4 = expression).
    /// </summary>
    private readonly int _startState;

    /// <summary>
    /// Stack for tracking nested parentheses, angle brackets, and square brackets to ensure proper matching.
    /// </summary>
    private readonly Stack<BracketStackEntry> _bracketStack = [];

    /// <summary>
    /// Stack for counting comma-separated arguments in function calls, array indices, and choice operations.
    /// </summary>
    private readonly Stack<int> _commaStack = [];

    /// <summary>
    /// Maps operator symbols to their corresponding binary operator token types.
    /// </summary>
    private static readonly Dictionary<string, Token.Type> _binaryOperators = new()
    {
        { "~", Token.Type.BINARY_TILDE },
        { "?", Token.Type.BINARY_QUESTION },
        { "$", Token.Type.BINARY_DOLLAR },
        { ".", Token.Type.BINARY_PERIOD },
        { "!", Token.Type.BINARY_CARET },
        { "**", Token.Type.BINARY_CARET },
        { "^", Token.Type.BINARY_CARET },
        { "%", Token.Type.BINARY_PERCENT },
        { "*", Token.Type.BINARY_STAR },
        { "/", Token.Type.BINARY_SLASH },
        { "#", Token.Type.BINARY_HASH },
        { "+", Token.Type.BINARY_PLUS },
        { "-", Token.Type.BINARY_MINUS },
        { "@", Token.Type.BINARY_AT },
        { "|", Token.Type.BINARY_PIPE },
        { "&", Token.Type.BINARY_AMPERSAND },
        { "=", Token.Type.BINARY_EQUAL }
    };

    /// <summary>
    /// Maps goto condition prefixes ("S", "F", or empty) to parenthesis-based goto token types.
    /// </summary>
    private static readonly Dictionary<string, Token.Type> _parenCondition = new()
    {
        { "S", Token.Type.L_PAREN_SUCCESS },
        { "F", Token.Type.L_PAREN_FAILURE },
        { "s", Token.Type.L_PAREN_SUCCESS },
        { "f", Token.Type.L_PAREN_FAILURE },
        { "", Token.Type.L_PAREN_UNCONDITIONAL }
    };

    /// <summary>
    /// Maps goto condition prefixes ("S", "F", or empty) to angle bracket-based goto token types.
    /// </summary>
    private static readonly Dictionary<string, Token.Type> _angleCondition = new()
    {
        { "S", Token.Type.L_ANGLE_SUCCESS },
        { "F", Token.Type.L_ANGLE_FAILURE },
        { "s", Token.Type.L_ANGLE_SUCCESS },
        { "f", Token.Type.L_ANGLE_FAILURE },
        { "", Token.Type.L_PAREN_UNCONDITIONAL }
    };

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Lexer"/> class.
    /// </summary>
    /// <param name="parent">The builder instance that owns this lexer.</param>
    /// <param name="startState">The initial DFA state (1 for statement, 4 for expression). Default is 1.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parent"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startState"/> is less than or equal to zero.</exception>
    internal Lexer(Builder parent, int startState = 1)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(startState);
        
        _parent = parent;
        _startState = startState;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initializes or resets the lexer state before processing a new source line.
    /// </summary>
    /// <param name="sourceLine">The source line to prepare for lexical analysis.</param>
    private void InitializeLexState(SourceLine sourceLine)
    {
        _bracketStack.Clear();
        _colonFound = false;
        _colonPosition = 0;
        _commaStack.Clear();
        _cursorCurrent = 0;
        _equalFound = false;
        _failureGotoEnd = 0;
        _failureGotoStart = 0;
        _patternMatchFound = false;
        sourceLine.LexFailureGoto.Clear();
        sourceLine.LexSuccessGoto.Clear();
        sourceLine.LexUnconditionalGoto.Clear();
        _state = _startState;
        _successGotoEnd = 0;
        _successGotoStart = 0;
        _unconditionalGotoStart = 0;
    }

    /// <summary>
    /// Validates whether a character is valid for DFA state transitions.
    /// </summary>
    /// <param name="c">The character to validate.</param>
    /// <param name="sourceLine">The source line containing the character.</param>
    /// <returns><c>true</c> if the character is ASCII (0-127); otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// State transitions rely on ASCII characters. UTF-8 characters are allowed in literals, labels, and comments
    /// but not for state transitions.
    /// </remarks>
    private bool IsValidCharacter(char c, SourceLine sourceLine)
    {
        // State transitions rely on ASCII characters.
        // UTF8 characters are allowed in literals, labels, and comments.
        if (c > 127)
        {
            _parent.LogCompilerException(230, _cursorCurrent, sourceLine);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Performs lexical analysis on all uncompiled source lines in the parent builder.
    /// </summary>
    /// <returns><c>true</c> if lexical analysis completed successfully; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method processes each source line, tokenizing the text and handling special constructs
    /// like unary star operators (deferred expressions) and goto statements.
    /// </remarks>
    internal bool Lex()
    {
        foreach (var sourceLine in _parent.Code.SourceLines.Where(line => !line.Compiled))
        {
            if (!LexLine(sourceLine))
                return false;
            ConvertUnaryStarOperatorsToDeferredExpressions(sourceLine.LexBody);
            ConvertUnaryStarOperatorsToDeferredExpressions(sourceLine.LexFailureGoto);
            ConvertUnaryStarOperatorsToDeferredExpressions(sourceLine.LexSuccessGoto);
            ConvertUnaryStarOperatorsToDeferredExpressions(sourceLine.LexUnconditionalGoto);
        }

        return true;
    }


    /// <summary>
    /// Performs lexical analysis on a single source line using a DFA-based tokenizer.
    /// </summary>
    /// <param name="sourceLine">The source line to tokenize.</param>
    /// <returns><c>true</c> if the line was successfully tokenized; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceLine"/> or its text is null.</exception>
    private bool LexLine(SourceLine sourceLine)
    {
        ArgumentNullException.ThrowIfNull(sourceLine);
        ArgumentNullException.ThrowIfNull(sourceLine.Text);
        
        InitializeLexState(sourceLine);

        while (_cursorCurrent < sourceLine.Text.Length)
        {
            var c = sourceLine.Text[_cursorCurrent];

            if (!IsValidCharacter(c, sourceLine))
            {
                return false;
            }

            _state = Delta[_state, c];

            if (_state > 100)
            {
                _parent.LogCompilerException(_state, _cursorCurrent, sourceLine);
                return false;
            }

            if (!FindLexeme(sourceLine, ref _state))
            {
                return false;
            }
        }

        ExtractGotoLexemes(sourceLine);

        return CheckForUnbalancedBrackets(sourceLine);
    }

    /// <summary>
    /// Identifies and processes a lexeme based on the current DFA state.
    /// </summary>
    /// <param name="sourceLine">The source line being analyzed.</param>
    /// <param name="state">The current DFA state that determines which lexeme type to process.</param>
    /// <returns><c>true</c> if the lexeme was successfully identified and processed; otherwise, <c>false</c>.</returns>
    private bool FindLexeme(SourceLine sourceLine, ref int state)
    {
        Match m;
        switch (state)
        {
            // If present, a label must begin in the first character
            // position of a statement, and must start with a
            // letter or number. Additional characters may be
            // anything except blank or tab.
            // [Emmer & Quillen 2000, pg. 29]
            case 2: // LABEL
                // Sub-lines produced by semicolon splitting (LineCountSubLine > 1)
                // cannot carry a label — column 1 is not the start of a physical line.
                // Skip label extraction entirely; transition to IDENTIFIER state (3).
                if (sourceLine.LineCountSubLine > 1)
                {
                    state = 3;
                    return FindLexeme(sourceLine, ref state);
                }

                m = LabelPattern().Match(sourceLine.Text);

                if (!m.Success)
                {
                    _parent.LogCompilerException(214, _cursorCurrent, sourceLine);
                    return false;
                }

                var label = _parent.FoldCase(m.Value);

                if (_parent.Code.Labels.ContainsKey(label))
                {
                    _parent.LogCompilerException(217, _cursorCurrent, sourceLine);
                    return false;
                }

                sourceLine.Label = label;

                // Stop when the end label is found
                var comparisonType = _parent.BuildOptions.CaseFolding 
                    ? StringComparison.OrdinalIgnoreCase 
                    : StringComparison.Ordinal;
                
                if (string.Equals(label, "end", comparisonType))
                {
                    _cursorCurrent = sourceLine.Text.Length; // Ignore the rest of the line
                    return true;
                }

                _parent.Code.Labels[label] = sourceLine.LineCountTotal;
                _cursorCurrent += m.Length;
                break;

            // Identifier names must start with a letter, either uppercase or
            // lowercase. If longer than one character, subsequent characters
            // can include letters, digits, periods, or underscores.
            // [Emmer & Quillen 2000, pg. 23]
            // An identifier followed by:
            //     A left parenthesis is lexically a function name.
            //     A left angle bracket is lexically an array name.
            //     A left square bracket is lexically a table name.
            //     Otherwise, it is a variable name.
            case 3: // IDENTIFIER
                if (!ProcessIdentifier(sourceLine))
                {
                    return false;
                }
                break;

            // White space is a lexeme that separates other lexemes.
            // White space can be a space or a horizontal tab. White space
            // can also be an explicit concatenation operator or an
            // implicit pattern match operator.
            case 4: // SPACE
                m = WhiteSpacePattern().Match(sourceLine.Text[_cursorCurrent..]);
                _cursorCurrent += m.Length;

                // Ignore white space at the beginning of a line
                if (sourceLine.LexBody.Count == 0)
                {
                    break;
                }

                sourceLine.LexBody.Add(new Token(Token.Type.SPACE, " ", _bracketStack.Count));
                ProcessImplicitOperators(sourceLine);
                break;

            // Literals are strings of UTF8 characters enclosed either in
            // a single quotation marks ('') or double quotation marks (""). Either
            // may be used, but the beginning and ending marks must be the
            // same. The string itself may contain one type of mark if the
            // other is used to enclose the string. Internally, strings are
            // stored following UTC16 UTF8 characters. UTF8 characters can be inserted
            // using the CHAR(n) function.
            // [Emmer & Quillen 2000, pg. 17]
            case 5: // STRING
                m = StringLiteralPattern().Match(sourceLine.Text[_cursorCurrent..]);

                if (!m.Success)
                {
                    _parent.LogCompilerException(232, _cursorCurrent, sourceLine);
                    return false;
                }

                _cursorCurrent += m.Length;
                sourceLine.LexBody.Add(new Token(Token.Type.STRING, m.Groups[2].Value, _bracketStack.Count));
                break;

            // A numeric constant is a sequence of digits, optionally
            // followed by a decimal point and another sequence of
            // digits, and optionally followed by an exponent. The
            // exponent is the letter e or E followed by an optional
            // sign and a sequence of digits. The exponent is a power
            // of 10 by which the number is multiplied. The number
            // may be an integer or a real number. If the number is a
            // real number, it is stored in 64-bit floating point number.
            // If the number is an integer, it is stored in 64-bit
            // signed integer. Integers and floating numbers must be 
            // represented by a valid .NET 64-bit number of the same type.
            // [Emmer and Quillen 2000, 196-197]
            case 6: // NUMERIC
                if (!ProcessNumericLiteral(sourceLine))
                {
                    return false;
                }
                break;

            // An operator is a symbol that represents an operation. The
            // operation may be unary or binary. A unary operator is an
            // operator that operates on one operand. Unary operators are
            // placed immediately to the left of their operand and no
            // blank or tab character may appear between operator and
            // operand. Binary operators have one or more blank or tab
            // characters on each side. A binary operator is an operator
            // that operates on two operands. The operands are the values
            // that the operator operates on.Emmer & Quillen 2000, pg. 18
            case 7: // OPERATOR
                if (!ProcessOperator(sourceLine))
                {
                    return false;
                }
                break;

            // When a colon is detected, control transfers to ProcessGoto()
            // which looks ahead such that the state is changed to 10 or 11.
            // Therefore, state 8 should never appear.
            case 8: // COLON
                if (!ProcessColon(sourceLine))
                {
                    return false;
                }
                break;

            // A comma is used only to separate arguments in a function call,
            // array index, or choice operation. Lexically, the first two are
            // a plain comma and the choice operation is a separate lexeme.
            case 9: // COMMA
                if (!ProcessComma(sourceLine))
                {
                    return false;
                }
                break;

            case 10: // L_PAREN
                ProcessOpenBracket(sourceLine, "(", GetOpenParenToken(sourceLine.LexBody));
                break;

            case 11: // L_ANGLE
                ProcessOpenBracket(sourceLine, "<", Token.Type.L_ANGLE);
                break;

            case 12: // L_SQUARE
                ProcessOpenBracket(sourceLine, "[", Token.Type.L_SQUARE);
                break;

            case 13: // R_PAREN
                if (!ValidateClosingBracket("(", 224, sourceLine))
                {
                    return false;
                }
                _cursorCurrent++;
                if (!ProcessClosingBracket(sourceLine))
                {
                    return false;
                }
                break;

            case 14: // R_ANGLE
                if (!ValidateClosingBracket("<", 225, sourceLine))
                {
                    return false;
                }
                _cursorCurrent++;
                if (!ProcessClosingBracket(sourceLine))
                {
                    return false;
                }
                break;

            case 15: // R_SQUARE
                if (!ValidateClosingBracket("[", 225, sourceLine))
                {
                    return false;
                }
                
                if (_commaStack.Count > 0)
                {
                    sourceLine.LexBody.Add(new Token(Token.Type.R_SQUARE, "]", _bracketStack.Count, _commaStack.Pop()));
                    _bracketStack.Pop();
                }
                
                _cursorCurrent++;
                ProcessImplicitNulls(sourceLine);
                break;
        }

        return true;
    }

    /// <summary>
    /// Processes a closing bracket (parenthesis or angle bracket) based on its context.
    /// </summary>
    /// <param name="sourceLine">The source line containing the closing bracket.</param>
    /// <returns><c>true</c> if the closing bracket was processed successfully; otherwise, <c>false</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unexpected bracket context is encountered.</exception>
    private bool ProcessClosingBracket(SourceLine sourceLine)
    {
        var entry = _bracketStack.Pop();
        var remainder = sourceLine.Text[_cursorCurrent..];

        switch (entry.Context)
        {
            case Token.Type.L_PAREN_FAILURE:
            case Token.Type.L_ANGLE_FAILURE:
            case Token.Type.L_ANGLE_SUCCESS:
            case Token.Type.L_PAREN_SUCCESS:
                if (!ProcessClosingConditionalGotoBracket(sourceLine, entry, remainder))
                {
                    return false;
                }
                _state = entry.Context is Token.Type.L_PAREN_FAILURE or Token.Type.L_ANGLE_SUCCESS ? 10 : 11;
                return true;

            case Token.Type.L_PAREN_UNCONDITIONAL:
                return ProcessUnconditionalGotoClosing(sourceLine, remainder, Token.Type.R_PAREN_UNCONDITIONAL, ")", 10);

            case Token.Type.L_ANGLE_UNCONDITIONAL:
                return ProcessUnconditionalGotoClosing(sourceLine, remainder, Token.Type.R_ANGLE_UNCONDITIONAL, ">", 11);

            case Token.Type.L_PAREN_FUNCTION:
                return ProcessRegularClosingBracket(sourceLine, Token.Type.R_PAREN_FUNCTION, ")");

            case Token.Type.L_ANGLE:
                return ProcessRegularClosingBracket(sourceLine, Token.Type.R_ANGLE, ">");

            case Token.Type.L_PAREN_CHOICE:
                return ProcessRegularClosingBracket(sourceLine, Token.Type.R_PAREN_CHOICE, ")");

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
            case Token.Type.COLON:
            case Token.Type.COMMA:
            case Token.Type.COMMA_CHOICE:
            case Token.Type.FAILURE_GOTO:
            case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
            case Token.Type.IDENTIFIER_FUNCTION:
            case Token.Type.IDENTIFIER:
            case Token.Type.INTEGER:
            case Token.Type.L_SQUARE:
            case Token.Type.NULL:
            case Token.Type.R_ANGLE:
            case Token.Type.R_ANGLE_FAILURE:
            case Token.Type.R_ANGLE_SUCCESS:
            case Token.Type.R_ANGLE_UNCONDITIONAL:
            case Token.Type.R_PAREN_CHOICE:
            case Token.Type.R_PAREN_FAILURE:
            case Token.Type.R_PAREN_FUNCTION:
            case Token.Type.R_PAREN_SUCCESS:
            case Token.Type.R_PAREN_UNCONDITIONAL:
            case Token.Type.R_SQUARE:
            case Token.Type.REAL:
            case Token.Type.SPACE:
            case Token.Type.STRING:
            case Token.Type.SUCCESS_GOTO:
            case Token.Type.UNARY_OPERATOR:
            case Token.Type.UNARY_STAR:
            case Token.Type.EXPRESSION:
            default:
                throw new InvalidOperationException($"Unexpected bracket context in ProcessClosingBracket: {entry.Context}");
        }
    }

    /// <summary>
    /// Processes the closing bracket of a conditional goto statement (success or failure).
    /// </summary>
    /// <param name="sourceLine">The source line containing the closing bracket.</param>
    /// <param name="entry">The bracket stack entry for the opening bracket.</param>
    /// <param name="remainder">The remaining text after the closing bracket.</param>
    /// <returns><c>true</c> if the closing bracket was processed successfully; otherwise, <c>false</c>.</returns>
    private bool ProcessClosingConditionalGotoBracket(SourceLine sourceLine, BracketStackEntry entry, string remainder)
    {
        sourceLine.LexBody.Add(new Token(entry.Context, entry.Bracket, _bracketStack.Count + 1));
        var altToken = GetMirrorGotoBracketToken(entry.Context);
        var gotoToken = GetMirrorGotoToken(entry.Context);
        var expectedSf = GetSfPair(entry.Context);
        SaveGotoEnd(entry.Context, sourceLine.LexBody.Count);

        // If everything after the closing bracket is whitespace, then nothing else to do
        if (AfterGoToPattern().IsMatch(remainder))
        {
            return true;
        }

        var m = GoToSecondPattern().Match(remainder);
        if (!m.Success)
        {
            _parent.LogCompilerException(234, _cursorCurrent, sourceLine);
            return false;
        }
        
        sourceLine.DirectGotoSecond = m.Groups[0].Value.Contains('<');

        if (!string.Equals(m.Groups[1].Value, expectedSf, StringComparison.OrdinalIgnoreCase))
        {
            _parent.LogCompilerException(218, _cursorCurrent, sourceLine);
            return false;
        }

        _cursorCurrent++;
        sourceLine.LexBody.Add(new Token(gotoToken, expectedSf, _bracketStack.Count));
        _cursorCurrent++;
        _bracketStack.Push(new BracketStackEntry(m.Groups[2].Value, altToken));
        sourceLine.LexBody.Add(new Token(altToken, m.Groups[2].Value, _bracketStack.Count));
        _cursorCurrent += m.Length - 2;
        SaveGotoStart(entry.Context, sourceLine.LexBody.Count - 1);
        return true;
    }

    /// <summary>
    /// Inserts implicit operators (concatenation or pattern match) when whitespace appears between operands.
    /// </summary>
    /// <param name="sourceLine">The source line being analyzed.</param>
    /// <remarks>
    /// In SNOBOL4, whitespace between operands can represent either implicit concatenation or
    /// implicit pattern matching, depending on context.
    /// </remarks>
    private void ProcessImplicitOperators(SourceLine sourceLine)
    {
        if (sourceLine.LexBody.Count < 2)
        {
            return;
        }
        
        var previousTokenType = sourceLine.LexBody[^2].TokenType;
        
        if (!CanHaveImplicitOperator(previousTokenType))
        {
            return;
        }

        var m = RightOperandPattern().Match(sourceLine.Text[_cursorCurrent..]);
        if (!m.Success)
        {
            return;
        }

        if (_patternMatchFound || _equalFound || _bracketStack.Count > 0)
        {
            AddImplicitConcatenation(sourceLine);
            return;
        }

        if (_startState == 1)
        {
            AddImplicitPatternMatch(sourceLine);
        }
    }

    /// <summary>
    /// Determines whether a token type can be followed by an implicit operator.
    /// </summary>
    /// <param name="tokenType">The token type to check.</param>
    /// <returns><c>true</c> if the token type can be followed by an implicit operator; otherwise, <c>false</c>.</returns>
    private static bool CanHaveImplicitOperator(Token.Type tokenType)
    {
        return tokenType is Token.Type.IDENTIFIER
            or Token.Type.STRING
            or Token.Type.INTEGER
            or Token.Type.REAL
            or Token.Type.R_PAREN_FUNCTION
            or Token.Type.R_PAREN_CHOICE
            or Token.Type.R_SQUARE
            or Token.Type.R_ANGLE;
    }

    /// <summary>
    /// Adds an implicit concatenation operator to the token list.
    /// </summary>
    /// <param name="sourceLine">The source line to add the operator to.</param>
    private void AddImplicitConcatenation(SourceLine sourceLine)
    {
        sourceLine.LexBody.Add(new Token(Token.Type.BINARY_CONCAT, "┴", _bracketStack.Count));
        sourceLine.LexBody.Add(new Token(Token.Type.SPACE, " ", _bracketStack.Count));
    }

    /// <summary>
    /// Adds an implicit pattern match operator to the token list.
    /// </summary>
    /// <param name="sourceLine">The source line to add the operator to.</param>
    private void AddImplicitPatternMatch(SourceLine sourceLine)
    {
        sourceLine.LexBody.Add(new Token(Token.Type.BINARY_QUESTION, " ", _bracketStack.Count));
        sourceLine.LexBody.Add(new Token(Token.Type.SPACE, " ", _bracketStack.Count));
        _patternMatchFound = true;
    }

    /// <summary>
    /// Processes a goto statement, including its condition (S/F) and delimiter.
    /// </summary>
    /// <param name="sourceLine">The source line containing the goto statement.</param>
    /// <param name="gotoMatch">The regex match containing goto components.</param>
    private void ProcessGoto(SourceLine sourceLine, Match gotoMatch)
    {
        sourceLine.LexBody.Add(new Token(Token.Type.COLON, ":", _bracketStack.Count));

        var gotoCondition = gotoMatch.Groups[1].Value;
        var gotoBracket = gotoMatch.Groups[2].Value;
        
        // Add goto type token (S or F)
        AddGotoTypeToken(sourceLine, gotoCondition);
        
        // Add bracket token and update state
        AddGotoBracketToken(sourceLine, gotoCondition, gotoBracket);
        
        // Save goto start position
        SaveGotoStartPosition(gotoCondition, sourceLine.LexBody.Count - 1);

        _cursorCurrent += gotoMatch.Length - 1;
    }

    /// <summary>
    /// Adds a goto type token (SUCCESS_GOTO or FAILURE_GOTO) based on the condition.
    /// </summary>
    /// <param name="sourceLine">The source line to add the token to.</param>
    /// <param name="condition">The goto condition ("s", "f", or empty string).</param>
    private void AddGotoTypeToken(SourceLine sourceLine, string condition)
    {
        var gotoToken = condition.ToLowerInvariant() switch
        {
            "s" => Token.Type.SUCCESS_GOTO,
            "f" => Token.Type.FAILURE_GOTO,
            _ => (Token.Type?)null
        };
        
        if (gotoToken.HasValue)
        {
            sourceLine.LexBody.Add(new Token(gotoToken.Value, condition.ToUpperInvariant(), _bracketStack.Count));
        }
    }

    /// <summary>
    /// Adds the opening bracket token for a goto statement and updates the DFA state.
    /// </summary>
    /// <param name="sourceLine">The source line to add the token to.</param>
    /// <param name="condition">The goto condition ("s", "f", or empty string).</param>
    /// <param name="bracket">The bracket character ("(" or "<").</param>
    private void AddGotoBracketToken(SourceLine sourceLine, string condition, string bracket)
    {
        var (bracketType, newState) = bracket switch
        {
            "(" => (_parenCondition[condition], 10),
            "<" => (_angleCondition[condition], 11),
            _ => ((Token.Type?)null, 0)
        };

        if (bracketType.HasValue)
        {
            _bracketStack.Push(new BracketStackEntry(bracket, bracketType.Value));
            sourceLine.LexBody.Add(new Token(bracketType.Value, bracket, _bracketStack.Count));
            _state = newState;
        }
    }

    /// <summary>
    /// Saves the starting position of a goto expression based on its condition.
    /// </summary>
    /// <param name="condition">The goto condition ("s", "f", or empty string).</param>
    /// <param name="position">The position in the token list.</param>
    private void SaveGotoStartPosition(string condition, int position)
    {
        var conditionLower = condition.ToLowerInvariant();
        
        switch (conditionLower)
        {
            case "":
                _unconditionalGotoStart = position;
                break;
            case "s":
                _successGotoStart = position;
                break;
            case "f":
                _failureGotoStart = position;
                break;
        }
    }

    /// <summary>
    /// Saves the ending position of a goto expression based on its token type.
    /// </summary>
    /// <param name="tokenType">The goto bracket token type.</param>
    /// <param name="position">The position in the token list.</param>
    private void SaveGotoEnd(Token.Type tokenType, int position)
    {
        if (tokenType is Token.Type.L_PAREN_FAILURE or Token.Type.L_ANGLE_FAILURE)
        {
            _failureGotoEnd = position;
        }
        else
        {
            _successGotoEnd = position;
        }
    }

    /// <summary>
    /// Saves the starting position of the second goto expression based on the first goto's token type.
    /// </summary>
    /// <param name="tokenType">The first goto bracket token type.</param>
    /// <param name="position">The position in the token list.</param>
    private void SaveGotoStart(Token.Type tokenType, int position)
    {
        if (tokenType is Token.Type.L_PAREN_FAILURE or Token.Type.L_ANGLE_FAILURE)
        {
            _successGotoStart = position;
        }
        else
        {
            _failureGotoStart = position;
        }
    }

    /// <summary>
    /// Extracts goto expressions from the main token list into separate lists for success, failure, and unconditional gotos.
    /// </summary>
    /// <param name="sourceLine">The source line containing goto expressions.</param>
    private void ExtractGotoLexemes(SourceLine sourceLine)
    {
        if (_bracketStack.Count > 0 || !_colonFound)
        {
            return;
        }

        if (_successGotoStart > 0)
        {
            sourceLine.LexSuccessGoto = [.. sourceLine.LexBody[(_successGotoStart + 1)..(_successGotoEnd - 1)]];
        }

        if (_failureGotoStart > 0)
        {
            sourceLine.LexFailureGoto = [.. sourceLine.LexBody[(_failureGotoStart + 1)..(_failureGotoEnd - 1)]];
        }

        if (_unconditionalGotoStart > 0)
        {
            sourceLine.LexUnconditionalGoto = [.. sourceLine.LexBody[(_unconditionalGotoStart + 1)..(_unconditionalGotoEnd - 1)]];
        }

        sourceLine.LexBody.RemoveRange(_colonPosition, sourceLine.LexBody.Count - _colonPosition);
    }

    /// <summary>
    /// Validates that a closing bracket matches the expected opening bracket.
    /// </summary>
    /// <param name="expectedBracket">The expected opening bracket character.</param>
    /// <param name="errorCode">The error code to log if validation fails.</param>
    /// <param name="sourceLine">The source line containing the bracket.</param>
    /// <returns><c>true</c> if the bracket is valid; otherwise, <c>false</c>.</returns>
    private bool ValidateClosingBracket(string expectedBracket, int errorCode, SourceLine sourceLine)
    {
        if (_bracketStack.Count == 0 || _bracketStack.Peek().Bracket != expectedBracket)
        {
            _parent.LogCompilerException(errorCode, _cursorCurrent, sourceLine);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Processes an opening bracket by pushing it onto the bracket stack and adding it to the token list.
    /// </summary>
    /// <param name="sourceLine">The source line containing the bracket.</param>
    /// <param name="bracket">The bracket character.</param>
    /// <param name="tokenType">The token type for this bracket.</param>
    private void ProcessOpenBracket(SourceLine sourceLine, string bracket, Token.Type tokenType)
    {
        _bracketStack.Push(new BracketStackEntry(bracket, tokenType));
        _cursorCurrent++;
        sourceLine.LexBody.Add(new Token(tokenType, bracket, _bracketStack.Count));
        _commaStack.Push(1);
    }

    /// <summary>
    /// Processes a comma token, determining whether it's a regular comma or a choice comma.
    /// </summary>
    /// <param name="sourceLine">The source line containing the comma.</param>
    /// <returns><c>true</c> if the comma was processed successfully; otherwise, <c>false</c>.</returns>
    private bool ProcessComma(SourceLine sourceLine)
    {
        _cursorCurrent++;

        if (_bracketStack.Count == 0)
        {
            _parent.LogCompilerException(223, _cursorCurrent, sourceLine);
            return false;
        }

        var tokenComma = _bracketStack.Peek().Context == Token.Type.L_PAREN_CHOICE
            ? Token.Type.COMMA_CHOICE
            : Token.Type.COMMA;
        
        sourceLine.LexBody.Add(new Token(tokenComma, ",", _bracketStack.Count, 0));
        
        if (_commaStack.Count > 0)
        {
            _commaStack.Push(_commaStack.Pop() + 1);
        }
        
        // If a function argument or array index is blank, convert to NULL
        ProcessImplicitNulls(sourceLine);
        return true;
    }

    /// <summary>
    /// Checks for unbalanced brackets at the end of a source line.
    /// </summary>
    /// <param name="sourceLine">The source line to check.</param>
    /// <returns><c>true</c> if all brackets are balanced; otherwise, <c>false</c>.</returns>
    private bool CheckForUnbalancedBrackets(SourceLine sourceLine)
    {
        if (_bracketStack.Count == 0)
        {
            return true;
        }

        var errorCode = _bracketStack.Peek().Bracket switch
        {
            "(" => _colonFound ? 227 : 226,
            "[" or "<" => _colonFound ? 228 : 229,
            _ => 226 // Default to parenthesis error
        };
        
        _parent.LogCompilerException(errorCode, _cursorCurrent, sourceLine);
        return false;
    }

    /// <summary>
    /// Processes an identifier token, determining whether it's a function, array/table, or simple identifier.
    /// </summary>
    /// <param name="sourceLine">The source line containing the identifier.</param>
    /// <returns><c>true</c> if the identifier was processed successfully; otherwise, <c>false</c>.</returns>
    private bool ProcessIdentifier(SourceLine sourceLine)
    {
        var m = IdentifierPattern().Match(sourceLine.Text[_cursorCurrent..]);
        if (!m.Success || m.Groups.Count < 3)
        {
            return false;
        }
        
        _cursorCurrent += m.Groups[1].Length;
        var identifier = _parent.FoldCase(m.Groups[1].Value);
        var tokenType = GetOpenBracketToken(m.Groups[2].Value);
        
        sourceLine.LexBody.Add(new Token(tokenType, identifier, _bracketStack.Count));
        return true;
    }

    /// <summary>
    /// Processes a numeric literal, determining whether it's an integer or real number.
    /// </summary>
    /// <param name="sourceLine">The source line containing the numeric literal.</param>
    /// <returns><c>true</c> if the numeric literal was processed successfully; otherwise, <c>false</c>.</returns>
    private bool ProcessNumericLiteral(SourceLine sourceLine)
    {
        var m = NumericPattern().Match(sourceLine.Text[_cursorCurrent..]);
        if (!m.Success)
        {
            return false;
        }
        
        _cursorCurrent += m.Length;
        var hasDecimalOrExponent = m.Groups[2].Value != "" || m.Groups[3].Value != "";

        if (hasDecimalOrExponent)
        {
            return ProcessRealNumber(sourceLine, m.Value);
        }

        return ProcessInteger(sourceLine, m.Value);
    }

    /// <summary>
    /// Processes a real (floating-point) number literal.
    /// </summary>
    /// <param name="sourceLine">The source line containing the number.</param>
    /// <param name="value">The string representation of the number.</param>
    /// <returns><c>true</c> if the number is valid; otherwise, <c>false</c>.</returns>
    private bool ProcessRealNumber(SourceLine sourceLine, string value)
    {
        if (!Var.ToReal(value, out var realValue))
        {
            _parent.LogCompilerException(231, _cursorCurrent, sourceLine);
            return false;
        }

        // Consider IEEE Infinity to be an error
        if (double.IsInfinity(realValue))
        {
            _parent.LogCompilerException(231, _cursorCurrent, sourceLine);
            return false;
        }

        sourceLine.LexBody.Add(new Token(Token.Type.REAL, value, _bracketStack.Count, realValue));
        return true;
    }

    /// <summary>
    /// Processes an integer literal.
    /// </summary>
    /// <param name="sourceLine">The source line containing the integer.</param>
    /// <param name="value">The string representation of the integer.</param>
    /// <returns><c>true</c> if the integer is valid; otherwise, <c>false</c>.</returns>
    private bool ProcessInteger(SourceLine sourceLine, string value)
    {
        if (!Var.ToInteger(value, out var intValue))
        {
            _parent.LogCompilerException(231, _cursorCurrent, sourceLine);
            return false;
        }

        sourceLine.LexBody.Add(new Token(Token.Type.INTEGER, value, _bracketStack.Count, intValue));
        return true;
    }

    /// <summary>
    /// Processes the closing bracket of an unconditional goto statement.
    /// </summary>
    /// <param name="sourceLine">The source line containing the closing bracket.</param>
    /// <param name="remainder">The remaining text after the closing bracket.</param>
    /// <param name="tokenType">The token type for the closing bracket.</param>
    /// <param name="bracket">The bracket character.</param>
    /// <param name="newState">The new DFA state after processing.</param>
    /// <returns><c>true</c> if the closing was processed successfully; otherwise, <c>false</c>.</returns>
    private bool ProcessUnconditionalGotoClosing(SourceLine sourceLine, string remainder, Token.Type tokenType, string bracket, int newState)
    {
        sourceLine.LexBody.Add(new Token(tokenType, bracket, _bracketStack.Count + 1));
        _unconditionalGotoEnd = sourceLine.LexBody.Count;
        _state = newState;

        // If the remaining text is whitespace, then nothing else to do
        if (AfterGoToPattern().IsMatch(remainder))
        {
            return true;
        }

        // If the first goto field is unconditional, any non-whitespace in the second field is an error
        var isSecondGotoValid = GoToSecondPattern().IsMatch(remainder);
        var errorCode = isSecondGotoValid ? 218 : 234;
        _parent.LogCompilerException(errorCode, _cursorCurrent, sourceLine);
        return false;
    }

    /// <summary>
    /// Processes a regular closing bracket (not part of a goto statement).
    /// </summary>
    /// <param name="sourceLine">The source line containing the closing bracket.</param>
    /// <param name="tokenType">The token type for the closing bracket.</param>
    /// <param name="bracket">The bracket character.</param>
    /// <returns><c>true</c> if processing was successful; otherwise, <c>false</c>.</returns>
    private bool ProcessRegularClosingBracket(SourceLine sourceLine, Token.Type tokenType, string bracket)
    {
        if (_commaStack.Count > 0)
        {
            sourceLine.LexBody.Add(new Token(tokenType, bracket, _bracketStack.Count + 1, _commaStack.Pop()));
        }
        else
        {
            sourceLine.LexBody.Add(new Token(tokenType, bracket, _bracketStack.Count + 1));
        }
        
        ProcessImplicitNulls(sourceLine);
        return true;
    }

    /// <summary>
    /// Processes an operator token, determining whether it's unary, binary, or a delete operator.
    /// </summary>
    /// <param name="sourceLine">The source line containing the operator.</param>
    /// <returns><c>true</c> if the operator was processed successfully; otherwise, <c>false</c>.</returns>
    private bool ProcessOperator(SourceLine sourceLine)
    {
        // Look for a unary operator or a sequence of unary operators
        // A string of unary operators is lexically a single token that is
        // broken into individual operators during code generation.
        var m = UnaryOperatorPattern().Match(sourceLine.Text[_cursorCurrent..]);

        if (m.Success)
        {
            return ProcessUnaryOperators(sourceLine, m);
        }

        // Look for binary operator (includes = assignment or replacement)
        m = BinaryOperatorPattern().Match(sourceLine.Text[_cursorCurrent..]);

        if (m.Success)
        {
            return ProcessBinaryOperator(sourceLine, m);
        }

        // The equals sign (=) is a binary operator that is used for
        // assignment or pattern replacement. When equal is a pattern
        // replacement operator and the right hand is blank, a null
        // string is the implicit operand. The following code identifies
        // this situation and adds an explicit null string to the right
        // hand of the assignment.
        return ProcessDeleteOperator(sourceLine);
    }

    /// <summary>
    /// Processes one or more consecutive unary operators.
    /// </summary>
    /// <param name="sourceLine">The source line containing the operators.</param>
    /// <param name="match">The regex match containing the operator sequence.</param>
    /// <returns><c>true</c> if processing was successful; otherwise, <c>false</c>.</returns>
    private bool ProcessUnaryOperators(SourceLine sourceLine, Match match)
    {
        _cursorCurrent += match.Groups[1].Length;
        
        for (var j = 0; j < match.Groups[1].Length; ++j)
        {
            var op = match.Groups[1].Value.Substring(j, 1);

            if (op == "*")
            {
                // Runs of consecutive stars is equivalent to one star; include once, then ignore
                if (sourceLine.LexBody.Count == 0 || sourceLine.LexBody[^1].TokenType != Token.Type.UNARY_STAR)
                {
                    sourceLine.LexBody.Add(new Token(Token.Type.UNARY_STAR, op, _bracketStack.Count));
                }
            }
            else
            {
                sourceLine.LexBody.Add(new Token(Token.Type.UNARY_OPERATOR, op, _bracketStack.Count));
            }
        }

        return true;
    }

    /// <summary>
    /// Processes a binary operator token.
    /// </summary>
    /// <param name="sourceLine">The source line containing the operator.</param>
    /// <param name="match">The regex match containing the operator.</param>
    /// <returns><c>true</c> if processing was successful; otherwise, <c>false</c>.</returns>
    private bool ProcessBinaryOperator(SourceLine sourceLine, Match match)
    {
        if (!_binaryOperators.TryGetValue(match.Groups[1].Value, out var tokenType))
        {
            _parent.LogCompilerException(233, _cursorCurrent, sourceLine);
            return false;
        }

        if (sourceLine.LexBody.Count == 0)
        {
            _parent.LogCompilerException(221, _cursorCurrent, sourceLine);
            return false;
        }

        _cursorCurrent += match.Groups[1].Length;

        if (tokenType == Token.Type.BINARY_EQUAL)
        {
            _equalFound = true;
        }

        sourceLine.LexBody.Add(new Token(tokenType, match.Groups[1].Value, _bracketStack.Count));
        return true;
    }

    /// <summary>
    /// Processes a delete operator (assignment to null string when right-hand side is blank).
    /// </summary>
    /// <param name="sourceLine">The source line containing the operator.</param>
    /// <returns><c>true</c> if processing was successful; otherwise, <c>false</c>.</returns>
    private bool ProcessDeleteOperator(SourceLine sourceLine)
    {
        var m = DeleteOperatorPattern().Match(sourceLine.Text[_cursorCurrent..]);

        if (!m.Success)
        {
            _parent.LogCompilerException(221, _cursorCurrent, sourceLine);
            return false;
        }

        _equalFound = true;
        _cursorCurrent += m.Groups[1].Length;
        sourceLine.LexBody.Add(new Token(Token.Type.BINARY_EQUAL, "=", _bracketStack.Count));
        sourceLine.LexBody.Add(new Token(Token.Type.SPACE, " ", _bracketStack.Count));
        sourceLine.LexBody.Add(new Token(Token.Type.STRING, "", _bracketStack.Count));
        return true;
    }

    /// <summary>
    /// Processes a colon token that marks the beginning of goto statements.
    /// </summary>
    /// <param name="sourceLine">The source line containing the colon.</param>
    /// <returns><c>true</c> if processing was successful; otherwise, <c>false</c>.</returns>
    private bool ProcessColon(SourceLine sourceLine)
    {
        var remainder = sourceLine.Text[_cursorCurrent..];
        _cursorCurrent++;

        if (_colonFound)
        {
            _parent.LogCompilerException(218, _cursorCurrent, sourceLine);
            return false;
        }

        if (_bracketStack.Count > 0)
        {
            var errorCode = _bracketStack.Peek().Bracket == "" ? 226 : 229;
            _parent.LogCompilerException(errorCode, _cursorCurrent, sourceLine);
            return false;
        }

        if (EmptyGoToPattern().IsMatch(remainder))
        {
            _parent.LogCompilerException(219, _cursorCurrent, sourceLine);
            return false;
        }

        var m = GoToFirstPattern().Match(remainder);
        if (!m.Success)
        {
            _parent.LogCompilerException(234, _cursorCurrent, sourceLine);
            return false;
        }

        sourceLine.DirectGotoFirst = m.Groups[0].Value.Contains('<');
        sourceLine.SuccessFirst = m.Groups[0].Value.Contains('s', StringComparison.OrdinalIgnoreCase);
        _colonFound = true;
        _colonPosition = sourceLine.LexBody.Count;
        ProcessGoto(sourceLine, m);
        return true;
    }

    #endregion

    #region Static Helper Functions

    /// <summary>
    /// Inserts implicit null tokens for empty function arguments, array indices, or choice alternatives.
    /// </summary>
    /// <param name="sourceLine">The source line to process.</param>
    private static void ProcessImplicitNulls(SourceLine sourceLine)
    {
        if (sourceLine.LexBody.Count == 0)
        {
            return;
        }
        
        var index = sourceLine.LexBody[^1].Index;

        if (index == 0)
        {
            return;
        }

        if (IsImplicitNull(sourceLine))
        {
            sourceLine.LexBody.Insert(sourceLine.LexBody.Count - 1, new Token(Token.Type.NULL, "", index));
        }
    }

    /// <summary>
    /// Determines whether an implicit null should be inserted at the current position.
    /// </summary>
    /// <param name="sourceLine">The source line to check.</param>
    /// <returns><c>true</c> if an implicit null is needed; otherwise, <c>false</c>.</returns>
    private static bool IsImplicitNull(SourceLine sourceLine)
    {
        if (sourceLine.LexBody.Count < 2)
        {
            return false;
        }
        
        return sourceLine.LexBody[^1].TokenType switch
        {
            Token.Type.R_ANGLE => CheckImplicitNull(sourceLine, Token.Type.L_ANGLE),
            Token.Type.R_PAREN_CHOICE => CheckImplicitNull(sourceLine, Token.Type.L_PAREN_CHOICE),
            Token.Type.R_PAREN_FUNCTION => CheckImplicitNull(sourceLine, Token.Type.L_PAREN_FUNCTION),
            Token.Type.R_SQUARE => CheckImplicitNull(sourceLine, Token.Type.L_SQUARE),
            Token.Type.COMMA => CheckCommaImplicitNull(sourceLine),
            _ => false
        };
    }

    /// <summary>
    /// Checks if an implicit null is needed for a specific closing bracket type.
    /// </summary>
    /// <param name="sourceLine">The source line to check.</param>
    /// <param name="openingType">The matching opening bracket token type.</param>
    /// <returns><c>true</c> if an implicit null is needed; otherwise, <c>false</c>.</returns>
    private static bool CheckImplicitNull(SourceLine sourceLine, Token.Type openingType)
    {
        var secondLast = sourceLine.LexBody[^2].TokenType;
        
        if (secondLast == Token.Type.COMMA || secondLast == openingType)
        {
            return true;
        }

        if (secondLast == Token.Type.SPACE && sourceLine.LexBody.Count >= 3)
        {
            var thirdLast = sourceLine.LexBody[^3].TokenType;
            return thirdLast == Token.Type.COMMA || thirdLast == openingType;
        }

        return false;
    }

    /// <summary>
    /// Checks if an implicit null is needed after a comma.
    /// </summary>
    /// <param name="sourceLine">The source line to check.</param>
    /// <returns><c>true</c> if an implicit null is needed; otherwise, <c>false</c>.</returns>
    private static bool CheckCommaImplicitNull(SourceLine sourceLine)
    {
        var secondLast = sourceLine.LexBody[^2].TokenType;
        
        if (secondLast is Token.Type.COMMA or Token.Type.L_SQUARE or Token.Type.L_ANGLE 
            or Token.Type.L_PAREN_CHOICE or Token.Type.L_PAREN_FUNCTION)
        {
            return true;
        }

        if (secondLast == Token.Type.SPACE && sourceLine.LexBody.Count >= 3)
        {
            var thirdLast = sourceLine.LexBody[^3].TokenType;
            return thirdLast is Token.Type.COMMA or Token.Type.L_SQUARE or Token.Type.L_ANGLE 
                or Token.Type.L_PAREN_CHOICE or Token.Type.L_PAREN_FUNCTION;
        }

        return false;
    }

    /// <summary>
    /// Determines the appropriate identifier token type based on the following bracket character.
    /// </summary>
    /// <param name="s">The bracket character following the identifier ("(", "<", "[", or empty).</param>
    /// <returns>The token type for the identifier.</returns>
    private static Token.Type GetOpenBracketToken(string s)
    {
        return s switch
        {
            "(" => Token.Type.IDENTIFIER_FUNCTION,
            "<" => Token.Type.IDENTIFIER_ARRAY_OR_TABLE,
            //"[" => Token.Type.IDENTIFIER_TABLE,
            "[" => Token.Type.IDENTIFIER_ARRAY_OR_TABLE,
            _ => Token.Type.IDENTIFIER
        };
    }

    /// <summary>
    /// Gets the mirror goto bracket token type for conditional gotos (success ↔ failure).
    /// </summary>
    /// <param name="tokenType">The first goto bracket token type.</param>
    /// <returns>The corresponding mirror token type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the token type is not a conditional goto bracket.</exception>
    private static Token.Type GetMirrorGotoBracketToken(Token.Type tokenType)
    {
        return tokenType switch
        {
            Token.Type.L_PAREN_FAILURE => Token.Type.L_PAREN_SUCCESS,
            Token.Type.L_PAREN_SUCCESS => Token.Type.L_PAREN_FAILURE,
            Token.Type.L_ANGLE_FAILURE => Token.Type.L_ANGLE_SUCCESS,
            Token.Type.L_ANGLE_SUCCESS => Token.Type.L_ANGLE_FAILURE,
            _ => throw new InvalidOperationException($"Unexpected token type in GetMirrorGotoBracketToken: {tokenType}")
        };
    }

    /// <summary>
    /// Gets the mirror goto type token (success ↔ failure).
    /// </summary>
    /// <param name="tokenType">The first goto bracket token type.</param>
    /// <returns>The corresponding mirror goto type token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the token type is not a conditional goto bracket.</exception>
    private static Token.Type GetMirrorGotoToken(Token.Type tokenType)
    {
        return tokenType switch
        {
            Token.Type.L_PAREN_FAILURE or Token.Type.L_ANGLE_FAILURE => Token.Type.SUCCESS_GOTO,
            Token.Type.L_PAREN_SUCCESS or Token.Type.L_ANGLE_SUCCESS => Token.Type.FAILURE_GOTO,
            _ => throw new InvalidOperationException($"Unexpected token type in GetMirrorGotoToken: {tokenType}")
        };
    }

    /// <summary>
    /// Gets the expected success/failure pair letter for a conditional goto.
    /// </summary>
    /// <param name="tokenType">The first goto bracket token type.</param>
    /// <returns>"s" if the first goto is failure (expecting success next), or "f" if success (expecting failure next).</returns>
    /// <exception cref="InvalidOperationException">Thrown when the token type is not a conditional goto bracket.</exception>
    private static string GetSfPair(Token.Type tokenType)
    {
        return tokenType switch
        {
            Token.Type.L_PAREN_FAILURE or Token.Type.L_ANGLE_FAILURE => "s",
            Token.Type.L_PAREN_SUCCESS or Token.Type.L_ANGLE_SUCCESS => "f",
            _ => throw new InvalidOperationException($"Unexpected token type in GetSfPair: {tokenType}")
        };
    }

    /// <summary>
    /// Determines whether an opening parenthesis is for a function call or a choice operation
    /// based on the preceding token.
    /// </summary>
    /// <param name="lexBody">The current token list.</param>
    /// <returns>The appropriate left parenthesis token type.</returns>
    private static Token.Type GetOpenParenToken(List<Token> lexBody)
    {
        if (lexBody.Count == 0)
        {
            return Token.Type.L_PAREN_CHOICE;
        }

        return lexBody[^1].TokenType switch
        {
            Token.Type.SPACE or
            Token.Type.L_PAREN_CHOICE or
            Token.Type.L_PAREN_FUNCTION or
            Token.Type.UNARY_OPERATOR or
            Token.Type.UNARY_STAR => Token.Type.L_PAREN_CHOICE,
            _ => Token.Type.L_PAREN_FUNCTION
        };
    }

    /// <summary>
    /// Finds the matching closing bracket for deferred expression processing.
    /// </summary>
    /// <param name="lexLine">The token list to search.</param>
    /// <param name="position">The starting position; updated to the closing bracket position if found.</param>
    /// <param name="closingType">The type of closing bracket to find.</param>
    /// <returns><c>true</c> if a matching bracket was found; otherwise, <c>false</c>.</returns>
    private bool FindMatchingBracket(List<Token> lexLine, ref int position, Token.Type closingType)
    {
        if (position >= lexLine.Count)
        {
            return false;
        }
        
        var searchIndex = lexLine[position].Index;
        
        while (position < lexLine.Count && 
               (lexLine[position].Index != searchIndex || lexLine[position].TokenType != closingType))
        {
            position++;
        }
        
        return position < lexLine.Count;
    }

    /// <summary>
    /// Creates a deferred expression (star expression) from a range of tokens.
    /// </summary>
    /// <param name="lexLine">The token list containing the expression.</param>
    /// <param name="starPos">The position of the unary star operator.</param>
    /// <param name="endPos">The ending position of the expression.</param>
    private void CreateStarExpression(List<Token> lexLine, int starPos, int endPos)
    {
        if (starPos < 0 || endPos <= starPos || starPos >= lexLine.Count)
        {
            return;
        }
        
        var expressionName = $"Star{_parent.ExpressionList.Count:D8}";
        lexLine[starPos] = new Token(Token.Type.EXPRESSION, expressionName, -1);
        
        var rangeStart = starPos + 1;
        var rangeLength = endPos - starPos;
        
        if (rangeLength > 0 && rangeStart < lexLine.Count)
        {
            _parent.ExpressionList.Add(new DeferredExpression(lexLine.GetRange(rangeStart, rangeLength)));
            lexLine.RemoveRange(rangeStart, rangeLength);
        }
    }

    /// <summary>
    /// Converts all unary star operators in a token list to deferred expressions.
    /// </summary>
    /// <param name="lexLine">The token list to process.</param>
    /// <remarks>
    /// The unary star operator in SNOBOL4 defers evaluation of its operand until runtime.
    /// This method extracts starred expressions and stores them for later evaluation.
    /// </remarks>
    private void ConvertUnaryStarOperatorsToDeferredExpressions(List<Token> lexLine)
    {
        if (lexLine.Count == 0)
        {
            return;
        }

        List<int> unaryStars = [];
        for (var i = lexLine.Count - 1; i >= 0; --i)
        {
            if (lexLine[i].TokenType == Token.Type.UNARY_STAR)
            {
                unaryStars.Add(i);
            }
        }

        if (unaryStars.Count == 0)
        {
            return;
        }

        foreach (var starPos in unaryStars)
        {
            ExtractStarExpressions(lexLine, starPos);
        }
    }

    /// <summary>
    /// Extracts a single star expression starting at the specified position.
    /// </summary>
    /// <param name="lexLine">The token list containing the expression.</param>
    /// <param name="starPos">The position of the unary star operator.</param>
    private void ExtractStarExpressions(List<Token> lexLine, int starPos)
    {
        if (starPos < 0 || starPos >= lexLine.Count || starPos + 1 >= lexLine.Count)
        {
            return;
        }
        
        var currentPos = starPos + 1;

        // Skip over any unary operators following the star
        while (currentPos < lexLine.Count && 
               lexLine[currentPos].TokenType is Token.Type.UNARY_OPERATOR or Token.Type.UNARY_STAR)
        {
            currentPos++;
        }
        
        if (currentPos >= lexLine.Count)
        {
            return;
        }

        switch (lexLine[currentPos].TokenType)
        {
            case Token.Type.SPACE:
            case Token.Type.REAL:
            case Token.Type.STRING:
            case Token.Type.INTEGER:
            case Token.Type.IDENTIFIER:
                CreateSimpleStarExpression(lexLine, starPos, currentPos + 1);
                return;

            case Token.Type.L_PAREN_CHOICE:
                if (FindMatchingBracket(lexLine, ref currentPos, Token.Type.R_PAREN_CHOICE))
                {
                    CreateStarExpression(lexLine, starPos, currentPos);
                }
                return;

            case Token.Type.IDENTIFIER_FUNCTION:
                currentPos++;
                if (FindMatchingBracket(lexLine, ref currentPos, Token.Type.R_PAREN_FUNCTION))
                {
                    CreateStarExpression(lexLine, starPos, currentPos);
                }
                return;

            default:
                // For any other token type, we can't create a star expression
                return;
        }
    }

    /// <summary>
    /// Creates a simple deferred expression (star expression) for a single token operand.
    /// </summary>
    /// <param name="lexLine">The token list containing the expression.</param>
    /// <param name="starPos">The position of the unary star operator.</param>
    /// <param name="endPos">The ending position of the expression.</param>
    private void CreateSimpleStarExpression(List<Token> lexLine, int starPos, int endPos)
    {
        var expressionName = $"Star{_parent.ExpressionList.Count:D8}";
        lexLine[starPos] = new Token(Token.Type.EXPRESSION, expressionName, -1);
        
        var rangeLength = endPos - starPos - 1;
        if (rangeLength > 0 && starPos + 1 < lexLine.Count)
        {
            _parent.ExpressionList.Add(new DeferredExpression(lexLine[(starPos + 1)..endPos]));
            lexLine.RemoveRange(starPos + 1, rangeLength);
        }
    }

    #endregion`
}
