using System.Text.RegularExpressions;

namespace Snobol4.Common;

public partial class Lexer
{
    #region Members

    private struct BracketStackEntry
    {
        internal readonly string Bracket;
        internal readonly Token.Type Context;

        internal BracketStackEntry(string bracket, Token.Type context)
        {
            Bracket = bracket;
            Context = context;
        }
    }

    // Lexical analysis
    private bool _colonFound;                 // Colon before goto found
    private bool _equalFound;                 // Equal sign found
    private bool _patternMatchFound;          // Implicit pattern match
    private int _colonPosition;               // Cursor position of colon
    private int _cursorCurrent;               // Cursor position in line
    private int _failureGotoEnd;              // Cursor end of failure goto
    private int _failureGotoStart;            // Cursor end of failure goto
    private int _state;                       // Current DFA state
    private int _successGotoEnd;              // Cursor end of success goto
    private int _successGotoStart;            // Cursor start of success goto
    private int _unconditionalGotoEnd;        // Cursor end of unconditional goto
    private int _unconditionalGotoStart;      // Cursor start of unconditional
    private readonly Builder _parent;         // Builder that created this Lexer
    private readonly int _startState;         // 1 = statement; 4 = expression

    // Stack for matching parentheses, angle brackets, and square brackets
    private readonly Stack<BracketStackEntry> _bracketStack = [];

    // Stack for counting commas in choice operations
    private readonly Stack<int> _commaStack = [];

    // Dictionary of binary operators
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

    private readonly Dictionary<string, Token.Type> _parenCondition = new()
    {
        { "S", Token.Type.L_PAREN_SUCCESS },
        { "F", Token.Type.L_PAREN_FAILURE },
        { "s", Token.Type.L_PAREN_SUCCESS },
        { "f", Token.Type.L_PAREN_FAILURE },
        { "", Token.Type.L_PAREN_UNCONDITIONAL }
    };

    private readonly Dictionary<string, Token.Type> _angleCondition = new()
    {
        { "S", Token.Type.L_ANGLE_SUCCESS },
        { "F", Token.Type.L_ANGLE_FAILURE },
        { "s", Token.Type.L_ANGLE_SUCCESS },
        { "f", Token.Type.L_ANGLE_FAILURE },
        { "", Token.Type.L_PAREN_UNCONDITIONAL }
    };

    #endregion

    #region Constructor

    internal Lexer(Builder parent, int startState = 1)
    {
        _parent = parent;
        _startState = startState;
    }

    #endregion

    #region Private Methods

    internal bool Lex()
    {
        foreach (var sourceLine in _parent.Code.SourceLines.Where(line => !line.Compiled))
        {
            LexLine(sourceLine);
            ConvertUnaryStarOperatorsToDeferredExpressions(sourceLine.LexBody);
            ConvertUnaryStarOperatorsToDeferredExpressions(sourceLine.LexFailureGoto);
            ConvertUnaryStarOperatorsToDeferredExpressions(sourceLine.LexSuccessGoto);
            ConvertUnaryStarOperatorsToDeferredExpressions(sourceLine.LexUnconditionalGoto);
        }

        return _parent.ErrorCodeHistory.Count <= 0;
    }


    private void LexLine(SourceLine sourceLine)
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

        while (_cursorCurrent < sourceLine.Text.Length)
        {
            var c = sourceLine.Text[_cursorCurrent];

            // State transitions rely on ASCII characters.
            // UTF8 characters are allowed in literals,
            // labels, and comments.
            if (c > 127)
            {
                _parent.LogCompilerException(230, _cursorCurrent, sourceLine);
                return;
            }

            _state = Delta[_state, c];

            if (_state > 100)
            {
                _parent.LogCompilerException(_state, _cursorCurrent, sourceLine);
                return;
            }

            if (!FindLexeme(sourceLine, ref _state))
                return;
        }

        ExtractGotoLexemes(sourceLine);

        CheckForUnbalancedBrackets(sourceLine);
    }

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
                m = CompiledRegex.LabelPattern().Match(sourceLine.Text);

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
                if (_parent.CaseFolding)
                {
                    if (string.Equals(label.ToUpper(), "END"))
                    {
                        _cursorCurrent = sourceLine.Text.Length; // Ignore the rest of the line
                        return true;
                    }
                }
                else
                {
                    if (string.Equals(label, "end"))
                    {
                        _cursorCurrent = sourceLine.Text.Length; // Ignore the rest of the line
                        return true;
                    }
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
                m = CompiledRegex.IdentifierPattern().Match(sourceLine.Text[_cursorCurrent..]);
                _cursorCurrent += m.Groups[1].Length;
                var identifier = _parent.FoldCase(m.Groups[1].Value);

                sourceLine.LexBody.Add(new Token(GetOpenBracketToken(m.Groups[2].Value), identifier, _bracketStack.Count));
                break;

            // White space is a lexeme that separates other lexemes.
            // White space can be a space or a horizontal tab. White space
            // can also be an explicit concatenation operator or an
            // implicit pattern match operator.
            case 4: // SPACE
                m = CompiledRegex.WhiteSpacePattern().Match(sourceLine.Text[_cursorCurrent..]);
                _cursorCurrent += m.Length;

                // Ignore white space at the beginning of a line
                if (sourceLine.LexBody.Count == 0)
                    break;

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
                m = CompiledRegex.StringLiteralPattern().Match(sourceLine.Text[_cursorCurrent..]);

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
                m = CompiledRegex.NumericPattern().Match(sourceLine.Text[_cursorCurrent..]);
                _cursorCurrent += m.Length;

                if (m.Groups[2].Value != "" || m.Groups[3].Value != "")
                {
                    // See if number is a legal real number per.NET
                    if (!Var.ToReal(m.Value, out var d))
                    {
                        _parent.LogCompilerException(231, _cursorCurrent, sourceLine);
                        return false;
                    }

                    // Consider IEEE Infinity to be an error
                    if (double.IsInfinity(d))
                    {
                        _parent.LogCompilerException(231, _cursorCurrent, sourceLine);
                        return false;
                    }

                    sourceLine.LexBody.Add(new Token(Token.Type.REAL, m.Value, _bracketStack.Count, d));
                    break;
                }

                if (!Var.ToInteger(m.Value, out var i))
                {
                    _parent.LogCompilerException(231, _cursorCurrent, sourceLine);
                    return false;
                }

                // See if number is a legal integer per.NET
                sourceLine.LexBody.Add(new Token(Token.Type.INTEGER, m.Value, _bracketStack.Count, i));
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
                // Look for a unary operator or a sequence of unary
                // operators A string of unary operators is lexically
                // a single token that is broken into individual
                // operators during code generation.
                m = CompiledRegex.UnaryOperatorPattern().Match(sourceLine.Text[_cursorCurrent..]);

                if (m.Success)
                {
                    _cursorCurrent += m.Groups[1].Length;
                    for (var j = 0; j < m.Groups[1].Length; ++j)
                    {
                        var op = m.Groups[1].Value[j..(j + 1)];

                        switch (op)
                        {
                            //case "~":
                            //    sourceLine.LexBody.Add(new Token(Token.Type.UNARY_NEGATION, op, _bracketStack.Count));
                            //    break;

                            //case "?":
                            //    sourceLine.LexBody.Add(new Token(Token.Type.UNARY_INTERROGATION, op, _bracketStack.Count));
                            //    break;


                            case "*":
                                // Runs of consecutive of stars is the equivalent to one star; include once, then ignore
                                if (sourceLine.LexBody.Count > 0 && sourceLine.LexBody[^1].TokenType != Token.Type.UNARY_STAR)
                                    sourceLine.LexBody.Add(new Token(Token.Type.UNARY_STAR, op, _bracketStack.Count));
                                break;

                            default:
                                sourceLine.LexBody.Add(new Token(Token.Type.UNARY_OPERATOR, op, _bracketStack.Count));
                                break;
                        }
                    }

                    break;
                }

                // Look for binary operator (Includes = assignment or replacement
                m = CompiledRegex.BinaryOperatorPattern().Match(sourceLine.Text[_cursorCurrent..]);

                if (m.Success)
                {
                    if (!_binaryOperators.TryGetValue(m.Groups[1].Value, out var type))
                    {
                        _parent.LogCompilerException(233, _cursorCurrent, sourceLine);
                        return false;
                    }

                    if (sourceLine.LexBody.Count == 0)
                    {
                        _parent.LogCompilerException(221, _cursorCurrent, sourceLine);
                        return false;
                    }

                    _cursorCurrent += m.Groups[1].Length;

                    if (type == Token.Type.BINARY_EQUAL)
                        _equalFound = true;

                    sourceLine.LexBody.Add(new Token(type, m.Groups[1].Value, _bracketStack.Count));
                    break;
                }

                // The equals sign (=) is a binary operator that is used for
                // assignment or pattern replacement. When equal is a pattern
                // replacement operator and the right hand is blank, a null
                // string is the implicit operand. The following code identifies
                // this situation and adds an explicit null string to the right
                // hand of the assignment.
                m = CompiledRegex.DeleteOperatorPattern().Match(sourceLine.Text[_cursorCurrent..]);

                if (!m.Success)
                {
                    _parent.LogCompilerException(221, _cursorCurrent, sourceLine);
                    return false;
                }

                _equalFound = true;
                //_cursorCurrent++;
                _cursorCurrent += m.Groups[1].Length;
                sourceLine.LexBody.Add(new Token(Token.Type.BINARY_EQUAL, "=", _bracketStack.Count));
                sourceLine.LexBody.Add(new Token(Token.Type.SPACE, " ", _bracketStack.Count));
                sourceLine.LexBody.Add(new Token(Token.Type.STRING, "", _bracketStack.Count));
                break;

            // When a colon is detected, control transfers to ProcessGoto()
            // which looks ahead such that the state is changed to 10 or 11.
            // Therefore, state 8 should never appear.
            case 8: // COLON
                var remainder = sourceLine.Text[_cursorCurrent..];
                _cursorCurrent++;

                if (_colonFound)
                {
                    _parent.LogCompilerException(218, _cursorCurrent, sourceLine);
                    return false;
                }

                if (_bracketStack.Count > 0)
                {
                    _parent.LogCompilerException(_bracketStack.Peek().Bracket == "" ? 226 : 229, _cursorCurrent, sourceLine);
                    return false;
                }

                if (CompiledRegex.EmptyGoToPattern().Match(remainder).Success)
                {
                    _parent.LogCompilerException(219, _cursorCurrent, sourceLine);
                    return false;
                }

                if (!(m = CompiledRegex.GoToFirstPattern().Match(remainder)).Success)
                {
                    _parent.LogCompilerException(234, _cursorCurrent, sourceLine);
                    return false;
                }

                sourceLine.DirectGotoFirst = m.Groups[0].Value.Contains('<');
                sourceLine.SuccessFirst = m.Groups[0].Value.ToLower().Contains('s');
                _colonFound = true;
                _colonPosition = sourceLine.LexBody.Count;
                ProcessGoto(sourceLine, m);
                break;

            // A comma is used only to separate arguments in a function call,
            // array index, or choice operation. Lexically, the first two are
            // a plain comma and the choice operation is a separate lexeme.
            case 9: // COMMA
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
                _commaStack.Push(_commaStack.Pop() + 1);
                // If and function argument or array index is blank, convert arguments
                // or index to NULL
                ProcessImplicitNulls(sourceLine);
                break;

            case 10: // L_PAREN
                var tokenParen = GetOpenParenToken(sourceLine.LexBody);
                _bracketStack.Push(new BracketStackEntry("(", tokenParen));
                _cursorCurrent++;
                sourceLine.LexBody.Add(new Token(tokenParen, "(", _bracketStack.Count));
                _commaStack.Push(1);
                break;

            case 11: // L_ANGLE
                _bracketStack.Push(new BracketStackEntry("<", Token.Type.L_ANGLE));
                _cursorCurrent++;
                sourceLine.LexBody.Add(new Token(Token.Type.L_ANGLE, "<", _bracketStack.Count, 0));
                _commaStack.Push(1);
                break;

            case 12: // L_SQUARE
                _bracketStack.Push(new BracketStackEntry("[", Token.Type.L_SQUARE));
                _cursorCurrent++;
                sourceLine.LexBody.Add(new Token(Token.Type.L_SQUARE, "[", _bracketStack.Count));
                _commaStack.Push(1);
                break;

            case 13: // R_PAREN
                if (_bracketStack.Count == 0 || _bracketStack.Peek().Bracket != "(")
                {
                    _parent.LogCompilerException(224, _cursorCurrent, sourceLine);
                    return false;
                }
                _cursorCurrent++;
                if (!ProcessClosingBracket(sourceLine))
                    return false;
                break;

            case 14: // R_ANGLE
                if (_bracketStack.Count == 0 || _bracketStack.Peek().Bracket != "<")
                {
                    _parent.LogCompilerException(225, _cursorCurrent, sourceLine);
                    return false;
                }

                _cursorCurrent++;

                if (!ProcessClosingBracket(sourceLine))
                    return false;

                break;

            case 15: // R_SQUARE
                if (_bracketStack.Count == 0 || _bracketStack.Peek().Bracket != "[")
                {
                    _parent.LogCompilerException(225, _cursorCurrent, sourceLine);
                    return false;
                }

                sourceLine.LexBody.Add(new Token(Token.Type.R_SQUARE, "]", _bracketStack.Count, _commaStack.Pop()));
                _bracketStack.Pop();
                _cursorCurrent++;
                ProcessImplicitNulls(sourceLine);

                break;
        }

        return true;
    }

    private bool ProcessClosingBracket(SourceLine sourceLine)
    {
        Match m;
        var entry = _bracketStack.Pop();
        var remainder = sourceLine.Text[_cursorCurrent..];

        switch (entry.Context)
        {
            case Token.Type.L_PAREN_FAILURE:
                if (!ProcessClosingConditionalGotoBracket(sourceLine, entry, remainder))
                    return false;
                _state = 10;
                return true;

            case Token.Type.L_ANGLE_FAILURE:
                if (!ProcessClosingConditionalGotoBracket(sourceLine, entry, remainder))
                    return false;
                _state = 11;
                return true;

            case Token.Type.L_ANGLE_SUCCESS:
                if (!ProcessClosingConditionalGotoBracket(sourceLine, entry, remainder))
                    return false;
                _state = 10;
                return true;

            case Token.Type.L_PAREN_SUCCESS:
                if (!ProcessClosingConditionalGotoBracket(sourceLine, entry, remainder))
                    return false;
                _state = 11;
                return true;

            case Token.Type.L_PAREN_UNCONDITIONAL:
                sourceLine.LexBody.Add(new Token(Token.Type.R_PAREN_UNCONDITIONAL, ")", _bracketStack.Count + 1));
                _unconditionalGotoEnd = sourceLine.LexBody.Count;
                _state = 10; //L_PAREN

                // If the remaining text is whitespace, then nothing else to do
                // ReSharper disable once ExtractCommonBranchingCode
                if (CompiledRegex.AfterGoToPattern().Match(remainder).Success)
                    return true;

                // If the first goto field is unconditional, any non-white
                // space in the second field is an error 
                // Determine the error message
                m = CompiledRegex.GoToSecondPattern().Match(remainder);
                _parent.LogCompilerException(!m.Success ? 234 : 218, _cursorCurrent, sourceLine);
                break;

            case Token.Type.L_ANGLE_UNCONDITIONAL:
                sourceLine.LexBody.Add(new Token(Token.Type.R_ANGLE_UNCONDITIONAL, ">", _bracketStack.Count + 1));
                _unconditionalGotoEnd = sourceLine.LexBody.Count;
                _state = 11; // L_ANGLE: If the remaining text is whitespace, then
                // nothing else to do

                if (CompiledRegex.AfterGoToPattern().Match(remainder).Success)
                    return true;

                // If the first goto field is unconditional, any non-white
                // space in the second field is an error 
                // Determine the error message
                m = CompiledRegex.GoToSecondPattern().Match(remainder);
                _parent.LogCompilerException(!m.Success ? 234 : 218, _cursorCurrent, sourceLine);
                break;

            case Token.Type.L_PAREN_FUNCTION:
                sourceLine.LexBody.Add(new Token(Token.Type.R_PAREN_FUNCTION, ")", _bracketStack.Count + 1, _commaStack.Pop()));
                ProcessImplicitNulls(sourceLine);
                return true;

            case Token.Type.L_ANGLE:
                sourceLine.LexBody.Add(new Token(Token.Type.R_ANGLE, ">", _bracketStack.Count + 1, _commaStack.Pop()));
                ProcessImplicitNulls(sourceLine);
                return true;

            case Token.Type.L_PAREN_CHOICE:
                sourceLine.LexBody.Add(new Token(Token.Type.R_PAREN_CHOICE, ")", _bracketStack.Count + 1, _commaStack.Pop()));
                ProcessImplicitNulls(sourceLine);
                return true;

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
                throw new ArgumentOutOfRangeException();
        }
        return false;
    }

    private bool ProcessClosingConditionalGotoBracket(SourceLine sourceLine, BracketStackEntry entry, string remainder)
    {
        sourceLine.LexBody.Add(new Token(entry.Context, entry.Bracket, _bracketStack.Count + 1));
        var altToken = GetMirrorGotoBracketToken(entry.Context);
        var gotoToken = GetMirrorGotoToken(entry.Context);
        var sf = GetSfPair(entry.Context);
        SaveGotoEnd(entry.Context, sourceLine.LexBody.Count);

        // If everything after the closing bracket is whitespace, then nothing else to do
        if (CompiledRegex.AfterGoToPattern().Match(remainder).Success)
            return true;

        var m = CompiledRegex.GoToSecondPattern().Match(remainder);

        if (m.Success)
            sourceLine.DirectGotoSecond = m.Groups[0].Value.Contains('<');
        else
        {
            _parent.LogCompilerException(234, _cursorCurrent, sourceLine);
            return false;
        }

#pragma warning disable CA1862
        if (m.Groups[1].Value.ToLower() != sf)
#pragma warning restore CA1862
        {
            _parent.LogCompilerException(218, _cursorCurrent, sourceLine);
            return false;
        }

        ++_cursorCurrent;
        sourceLine.LexBody.Add(new Token(gotoToken, sf, _bracketStack.Count));
        ++_cursorCurrent;
        _bracketStack.Push(new BracketStackEntry(m.Groups[2].Value, altToken));
        sourceLine.LexBody.Add(new Token(altToken, m.Groups[2].Value, _bracketStack.Count));
        _cursorCurrent += m.Length - 2;
        SaveGotoStart(entry.Context, sourceLine.LexBody.Count - 1);
        return true;
    }

    private void ProcessImplicitOperators(SourceLine sourceLine)
    {
        switch (sourceLine.LexBody[^2].TokenType)
        {
            case Token.Type.IDENTIFIER:
            case Token.Type.STRING:
            case Token.Type.INTEGER:
            case Token.Type.REAL:
            case Token.Type.R_PAREN_FUNCTION:
            case Token.Type.R_PAREN_CHOICE:
            case Token.Type.R_SQUARE:
            case Token.Type.R_ANGLE:
                break;

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
            case Token.Type.L_ANGLE:
            case Token.Type.L_ANGLE_FAILURE:
            case Token.Type.L_ANGLE_SUCCESS:
            case Token.Type.L_ANGLE_UNCONDITIONAL:
            case Token.Type.L_PAREN_CHOICE:
            case Token.Type.L_PAREN_FAILURE:
            case Token.Type.L_PAREN_FUNCTION:
            case Token.Type.L_PAREN_SUCCESS:
            case Token.Type.L_PAREN_UNCONDITIONAL:
            case Token.Type.L_SQUARE:
            case Token.Type.NULL:
            case Token.Type.R_ANGLE_FAILURE:
            case Token.Type.R_ANGLE_SUCCESS:
            case Token.Type.R_ANGLE_UNCONDITIONAL:
            case Token.Type.R_PAREN_FAILURE:
            case Token.Type.R_PAREN_SUCCESS:
            case Token.Type.R_PAREN_UNCONDITIONAL:
            case Token.Type.SPACE:
            case Token.Type.SUCCESS_GOTO:
            case Token.Type.UNARY_OPERATOR:
            case Token.Type.UNARY_STAR:
            case Token.Type.EXPRESSION:
            default:
                return;
        }

        var m = CompiledRegex.RightOperandPattern().Match(sourceLine.Text[_cursorCurrent..]);

        if (!m.Success)
            return;

        if (_patternMatchFound || _equalFound || _bracketStack.Count > 0)
        {
            sourceLine.LexBody.Add(new Token(Token.Type.BINARY_CONCAT, "┴", _bracketStack.Count));
            sourceLine.LexBody.Add(new Token(Token.Type.SPACE, " ", _bracketStack.Count));
            return;
        }

        if (_startState != 1)
            return;

        sourceLine.LexBody.Add(new Token(Token.Type.BINARY_QUESTION, " ", _bracketStack.Count));
        sourceLine.LexBody.Add(new Token(Token.Type.SPACE, " ", _bracketStack.Count));
        _patternMatchFound = true;
    }

    private void ProcessGoto(SourceLine sourceLine, Match m)
    {
        sourceLine.LexBody.Add(new Token(Token.Type.COLON, ":", _bracketStack.Count));

        switch (m.Groups[1].Value) //S or F or null
        {
            case "S":
            case "s":
                sourceLine.LexBody.Add(new Token(Token.Type.SUCCESS_GOTO, "S", _bracketStack.Count));
                break;

            case "F":
            case "f":
                sourceLine.LexBody.Add(new Token(Token.Type.FAILURE_GOTO, "F", _bracketStack.Count));
                break;
        }

        switch (m.Groups[2].Value)
        {
            case "(":
                var parenCondition = _parenCondition[m.Groups[1].Value];
                _bracketStack.Push(new BracketStackEntry("(", parenCondition));
                sourceLine.LexBody.Add(new Token(parenCondition, "(", _bracketStack.Count));
                _state = 10; // L_PAREN
                break;

            case "<":
                var angleCondition = _angleCondition[m.Groups[1].Value];
                _bracketStack.Push(new BracketStackEntry("<", angleCondition));
                sourceLine.LexBody.Add(new Token(angleCondition, "<", _bracketStack.Count));
                _state = 11; // L_ANGLE
                break;
        }

        switch (m.Groups[1].Value) //S or F or null
        {
            case "":
                _unconditionalGotoStart = sourceLine.LexBody.Count - 1;
                break;

            case "S":
            case "s":
                _successGotoStart = sourceLine.LexBody.Count - 1;
                break;

            case "F":
            case "f":
                _failureGotoStart = sourceLine.LexBody.Count - 1;
                break;
        }

        _cursorCurrent += m.Length - 1;
    }

    private void SaveGotoEnd(Token.Type t, int pos)
    {
        if (t is Token.Type.L_PAREN_FAILURE or Token.Type.L_ANGLE_FAILURE)
            _failureGotoEnd = pos;
        else
            _successGotoEnd = pos;
    }

    private void SaveGotoStart(Token.Type t, int pos)
    {
        if (t is Token.Type.L_PAREN_FAILURE or Token.Type.L_ANGLE_FAILURE)
            _successGotoStart = pos;
        else
            _failureGotoStart = pos;
    }

    private void ExtractGotoLexemes(SourceLine sourceLine)
    {
        if (_bracketStack.Count > 0)
            return;

        if (!_colonFound)
            return;

        if (_successGotoStart > 0)
            sourceLine.LexSuccessGoto = [.. sourceLine.LexBody[(_successGotoStart + 1)..(_successGotoEnd - 1)]];

        if (_failureGotoStart > 0)
            sourceLine.LexFailureGoto = [.. sourceLine.LexBody[(_failureGotoStart + 1)..(_failureGotoEnd - 1)]];

        if (_unconditionalGotoStart > 0)
            sourceLine.LexUnconditionalGoto =
                [.. sourceLine.LexBody[(_unconditionalGotoStart + 1)..(_unconditionalGotoEnd - 1)]];

        sourceLine.LexBody.RemoveRange(_colonPosition, sourceLine.LexBody.Count - _colonPosition);
    }

    private void CheckForUnbalancedBrackets(SourceLine sourceLine)
    {
        if (_bracketStack.Count == 0)
            return;

        switch (_bracketStack.Peek().Bracket)
        {
            case "(":
                _parent.LogCompilerException(_colonFound ? 227 : 226, _cursorCurrent, sourceLine);
                return;

            case "[":
            case "<":
                _parent.LogCompilerException(_colonFound ? 228 : 229, _cursorCurrent, sourceLine);
                return;
        }
    }

    #endregion

    #region Static Helper Functiona

    private static void ProcessImplicitNulls(SourceLine sourceLine)
    {
        var index = sourceLine.LexBody[^1].Index;

        if (index == 0)
            return;

        if (IsImplicitNull(sourceLine))
            sourceLine.LexBody.Insert(sourceLine.LexBody.Count - 1, new Token(Token.Type.NULL, "", index));
    }

    private static bool IsImplicitNull(SourceLine sourceLine)
    {
        return sourceLine.LexBody[^1].TokenType switch
        {
            Token.Type.R_ANGLE => sourceLine.LexBody[^2].TokenType switch
            {
                Token.Type.COMMA or Token.Type.L_ANGLE => true,

                Token.Type.SPACE => sourceLine.LexBody[^3].TokenType switch
                {
                    Token.Type.COMMA or Token.Type.L_ANGLE => true,
                    _ => false
                },

                _ => false
            },

            Token.Type.R_PAREN_CHOICE => sourceLine.LexBody[^2].TokenType switch
            {
                Token.Type.COMMA or Token.Type.L_PAREN_CHOICE => true,

                Token.Type.SPACE => sourceLine.LexBody[^3].TokenType switch
                {
                    Token.Type.COMMA or Token.Type.L_PAREN_CHOICE => true,
                    _ => false
                },

                _ => false
            },


            Token.Type.R_PAREN_FUNCTION => sourceLine.LexBody[^2].TokenType switch
            {
                Token.Type.COMMA or Token.Type.L_PAREN_FUNCTION => true,

                Token.Type.SPACE => sourceLine.LexBody[^3].TokenType switch
                {
                    Token.Type.COMMA or Token.Type.L_PAREN_FUNCTION => true,
                    _ => false
                },

                _ => false
            },

            Token.Type.R_SQUARE => sourceLine.LexBody[^2].TokenType switch
            {
                Token.Type.COMMA or Token.Type.L_SQUARE => true,

                Token.Type.SPACE => sourceLine.LexBody[^3].TokenType switch
                {
                    Token.Type.COMMA or Token.Type.L_SQUARE => true,
                    _ => false
                },

                _ => false
            },

            Token.Type.COMMA => sourceLine.LexBody[^2].TokenType switch
            {
                Token.Type.COMMA or Token.Type.L_SQUARE or Token.Type.L_ANGLE or Token.Type.L_PAREN_CHOICE
                    or Token.Type.L_PAREN_FUNCTION => true,

                Token.Type.SPACE => sourceLine.LexBody[^3].TokenType switch
                {
                    Token.Type.COMMA or Token.Type.L_SQUARE or Token.Type.L_ANGLE or Token.Type.L_PAREN_CHOICE
                        or Token.Type.L_PAREN_FUNCTION => true,

                    _ => false
                },

                _ => false
            },
            _ => false
        };
    }

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

    private static Token.Type GetMirrorGotoBracketToken(Token.Type t)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return t switch
        {
            Token.Type.L_PAREN_FAILURE => Token.Type.L_PAREN_SUCCESS,
            Token.Type.L_PAREN_SUCCESS => Token.Type.L_PAREN_FAILURE,
            Token.Type.L_ANGLE_FAILURE => Token.Type.L_ANGLE_SUCCESS,
            Token.Type.L_ANGLE_SUCCESS => Token.Type.L_ANGLE_FAILURE,
            _ => throw new ApplicationException("GetMirrorGotoBracketToken")
        };
    }

    private static Token.Type GetMirrorGotoToken(Token.Type t)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return t switch
        {
            Token.Type.L_PAREN_FAILURE => Token.Type.SUCCESS_GOTO,
            Token.Type.L_PAREN_SUCCESS => Token.Type.FAILURE_GOTO,
            Token.Type.L_ANGLE_FAILURE => Token.Type.SUCCESS_GOTO,
            Token.Type.L_ANGLE_SUCCESS => Token.Type.FAILURE_GOTO,
            _ => throw new ApplicationException("GetMirrorGotoToken")
        };
    }

    private static string GetSfPair(Token.Type t)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return t switch
        {
            Token.Type.L_PAREN_FAILURE => "s",
            Token.Type.L_PAREN_SUCCESS => "f",
            Token.Type.L_ANGLE_FAILURE => "s",
            Token.Type.L_ANGLE_SUCCESS => "f",
            _ => throw new ApplicationException("GetSfPair")
        };
    }

    private static Token.Type GetOpenParenToken(List<Token> lexBody)
    {
        if (lexBody.Count == 0)
            return Token.Type.L_PAREN_CHOICE;

        return lexBody[^1].TokenType switch
        {
            Token.Type.SPACE => Token.Type.L_PAREN_CHOICE,
            Token.Type.L_PAREN_CHOICE => Token.Type.L_PAREN_CHOICE,
            Token.Type.L_PAREN_FUNCTION => Token.Type.L_PAREN_CHOICE,
            //            Token.Type.UNARY_NOT => Token.Type.L_PAREN_CHOICE,
            Token.Type.UNARY_OPERATOR => Token.Type.L_PAREN_CHOICE,
            Token.Type.UNARY_STAR => Token.Type.L_PAREN_CHOICE,
            _ => Token.Type.L_PAREN_FUNCTION
        };
    }

    private void ConvertUnaryStarOperatorsToDeferredExpressions(List<Token> lexLine)
    {
        if (lexLine.Count == 0)
            return;

        if (_parent.ErrorCodeHistory.Count > 0)
            return;

        List<int> unaryStars = [];
        for (var i = lexLine.Count - 1; i >= 0; --i)
            if (lexLine[i].TokenType == Token.Type.UNARY_STAR)
                unaryStars.Add(i);

        if (unaryStars.Count == 0)
            return;

        foreach (var starPos in unaryStars)
            ExtractStarExpressions(lexLine, starPos);
    }

    private void ExtractStarExpressions(List<Token> lexLine, int starPos)
    {
        var rArg = starPos + 1;

        while ( /*lexLine[rArg].TokenType == Token.Type.UNARY_NOT ||*/
               lexLine[rArg].TokenType == Token.Type.UNARY_OPERATOR ||
               lexLine[rArg].TokenType == Token.Type.UNARY_STAR)
            rArg++;

        switch (lexLine[rArg].TokenType)
        {
            case Token.Type.SPACE:
            case Token.Type.REAL:
            case Token.Type.STRING:
            case Token.Type.INTEGER:
            case Token.Type.IDENTIFIER:
                lexLine[starPos] = new Token(Token.Type.EXPRESSION, "Star" + _parent.ExpressionList.Count.ToString("D8"), -1);
                rArg++;
                _parent.ExpressionList.Add(new DeferredExpression(lexLine[(starPos + 1)..rArg]));
                lexLine.RemoveRange(starPos + 1, rArg - starPos - 1);
                return;

            case Token.Type.L_PAREN_CHOICE:
                lexLine[starPos] = new Token(Token.Type.EXPRESSION, "Star" + _parent.ExpressionList.Count.ToString("D8"), -1);
                var searchIndex3 = lexLine[rArg].Index;
                while (lexLine[rArg].Index != searchIndex3 || lexLine[rArg].TokenType != Token.Type.R_PAREN_CHOICE)
                    ++rArg;
                _parent.ExpressionList.Add(new DeferredExpression(lexLine.GetRange(starPos + 1, rArg - starPos)));
                lexLine.RemoveRange(starPos + 1, rArg - starPos);
                return;

            case Token.Type.IDENTIFIER_FUNCTION:
                lexLine[starPos] = new Token(Token.Type.EXPRESSION, "Star" + _parent.ExpressionList.Count.ToString("D8"), -1);
                rArg++;
                var searchIndex2 = lexLine[rArg].Index;
                while (lexLine[rArg].Index != searchIndex2 || lexLine[rArg].TokenType != Token.Type.R_PAREN_FUNCTION)
                    ++rArg;
                _parent.ExpressionList.Add(new DeferredExpression(lexLine.GetRange(starPos + 1, rArg - starPos)));
                lexLine.RemoveRange(starPos + 1, rArg - starPos);
                return;

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
            case Token.Type.L_ANGLE:
            case Token.Type.L_ANGLE_FAILURE:
            case Token.Type.L_ANGLE_SUCCESS:
            case Token.Type.L_ANGLE_UNCONDITIONAL:
            case Token.Type.L_PAREN_FAILURE:
            case Token.Type.L_PAREN_FUNCTION:
            case Token.Type.L_PAREN_SUCCESS:
            case Token.Type.L_PAREN_UNCONDITIONAL:
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
            case Token.Type.SUCCESS_GOTO:
            case Token.Type.UNARY_OPERATOR:
            case Token.Type.UNARY_STAR:
            case Token.Type.EXPRESSION:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion`
}
