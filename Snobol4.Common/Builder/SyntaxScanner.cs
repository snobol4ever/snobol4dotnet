using System.Globalization;
using System.Text.RegularExpressions;

namespace Snobol4.Common;

/// <summary>
/// The syntax errors of a statement, found as SPITBOL finds them: by its scanner (scane) and its expression analyzer (expan), read
/// from the elements of the statement one after the other. The error of a statement is the first one SPITBOL meets, and its column
/// is where the element it was meeting began (scnse), so that a missing operand is reported at the operator that came where an
/// operand should have, not at the one before it, a misplaced operator under itself, and an unbalanced paren at the terminator.
/// <para>
/// The scanner takes a blank to be part of what an operator means: a binary operator (one followed by a blank, a semicolon, a colon or
/// a closing bracket, or by the end of the statement) must have a blank before it; a unary one (anything else) must have a blank
/// before it unless it follows an operator, an opening bracket, a comma or a function call. Names and numbers end at a blank, a
/// bracket, a quote or an operator, but + and - cannot end a name (A+B is error 233 where A starts), and make a number of what
/// follows (1+2 is not a number, error 231).
/// </para>
/// The expression analyzer is a machine with three states (nothing scanned at this level, an operand expected, an operator
/// expected) and a stack of levels (outer, goto, direct goto, array brackets, grouping, function arguments); the errors are the
/// pairs of an element with a state that have no meaning (220 to 229), and a goto field that is neither ( ) nor [ ] (234). Building the
/// tree is left to the lexer and parser, which read what this has passed.
/// </summary>
internal sealed partial class SyntaxScanner
{
    // The element types of scane, in the order of its t_xxx codes: a unary operator is allowed without a blank before it after any
    // element up to a function call
    private const int Unary = 0, LeftParen = 3, LeftBracket = 6, Comma = 9, Function = 12, Variable = 15, Constant = 18, Binary = 21,
        RightParen = 24, RightBracket = 27, Colon = 30, Semicolon = 33, FailureGoto = 34, SuccessGoto = 35;

    private const int UnaryOk = Function;

    // The levels of the analyzer
    private const int Outer = 0, NormalGoto = 1, DirectGoto = 2, Subscripts = 3, Grouping = 4, Arguments = 5;

    // SwallowsNext: the element at fault was the semicolon that ends the statement, so SPITBOL, skipping to the end of the statement
    // after the error, takes the next statement of the line for the rest of this one and compiles nothing of it
    // AtEnd: the element at fault was the end of the text, which SPITBOL finds by reading on to the next card
    internal readonly record struct Error(int Code, int Cursor, bool SwallowsNext = false, bool AtEnd = false);

    private sealed class SyntaxFault(int code) : Exception
    {
        internal int Code { get; } = code;
    }

    private readonly string _text;
    private int _position;                // scnpt: where the next element is read from
    private int _start;                   // scnse: where the element read last began
    private bool _blank;                  // scnbl: blanks were skipped before the element read last
    private int _previous = Semicolon;    // scntp: the type of the element read before it
    private bool _gotoField;              // scngo: S and F are goto types, not letters
    private bool _rescan;                 // scnrs: the element read last is read again
    private int _type = Semicolon;        // its type
    private bool _semicolon;              // and it is a semicolon in the text, not the end of it
    private bool _atEnd;                  // and it is the end of the text
    private char _operator;               // and its character, for an operator

    private Func<string, bool>? _defined;

    // Where the physical lines of the statement end in the text, each at the blank that stands for the continuation character: a string
    // does not run over one
    private int[] _lineEnds = [];

    private SyntaxScanner(string text) => _text = text;

    /// <summary>
    /// The first syntax error of a statement as it is written (label, body, gotos), or null for none. A statement whose label is not a
    /// name SPITBOL could read is left to the lexer. The END statement may carry the name of the label the run starts at, which must
    /// be one the program has defined (`defined` says so): anything else after END is error 215.
    /// </summary>
    internal static Error? FirstError(string text, Func<string, bool>? defined = null, int[]? lineEnds = null)
    {
        if (text.Length == 0)
            return null;

        var scanner = new SyntaxScanner(text) { _defined = defined, _lineEnds = lineEnds ?? [] };
        try
        {
            return scanner.Statement();
        }
        catch (SyntaxFault fault)
        {
            return new Error(fault.Code, scanner._start, scanner._type == Semicolon && scanner._semicolon, scanner._atEnd);
        }
    }

    private Error? Statement()
    {
        // a label runs to the first blank, tab or semicolon
        if (_text[0] is not (' ' or '\t' or ';'))
        {
            if (!char.IsAsciiLetterOrDigit(_text[0]))
                return null;
            while (_position < _text.Length && _text[_position] is not (' ' or '\t' or ';'))
                _position++;
            if (_text[.._position].Equals("END", StringComparison.OrdinalIgnoreCase))
                return EndStatement();
        }

        _start = _position;
        Expand(Outer);

        if (Read() != Colon)
            return null;

        // the goto fields
        var success = false;
        var failure = false;
        while (true)
        {
            _gotoField = true;
            var type = Read();
            if (type == Semicolon)
            {
                if (!success && !failure)
                    throw new SyntaxFault(219);
                return null;
            }

            switch (type)
            {
                case SuccessGoto:
                    GotoField();
                    if (success)
                        throw new SyntaxFault(218);
                    success = true;
                    break;
                case FailureGoto:
                    GotoField();
                    if (failure)
                        throw new SyntaxFault(218);
                    failure = true;
                    break;
                default:
                    // a goto that is neither: it sets both, and so fails if either is set
                    _rescan = true;
                    GotoField();
                    if (failure || success)
                        throw new SyntaxFault(218);
                    failure = success = true;
                    break;
            }
        }
    }

    // cmp08: after END comes nothing or the name of the label to start at, and nothing after that
    private Error? EndStatement()
    {
        _start = _position;
        var type = Read();
        if (type == Semicolon)
            return null;
        if (type != Variable)
            throw new SyntaxFault(215);
        if (_defined is { } defined && !defined(_text[_start.._position]))
            throw new SyntaxFault(215);
        if (Read() != Semicolon)
            throw new SyntaxFault(215);
        return null;
    }

    // scngf: a goto field is ( ) or [ ], an expression between them
    private void GotoField()
    {
        switch (Read())
        {
            case LeftParen:
                Expand(NormalGoto);
                break;
            case LeftBracket:
                Expand(DirectGoto);
                break;
            default:
                throw new SyntaxFault(234);
        }
    }

    // expan: the elements of an expression, until its terminator (a colon or semicolon of the outer level, which is left to be read
    // again; the closing paren or bracket of a goto, which is not)
    private void Expand(int level)
    {
        // 0 nothing scanned at this level, 1 an operand expected, 2 an operator expected
        var state = 0;
        var outer = new Stack<int>();
        while (true)
        {
            var type = Read();
            switch (type)
            {
                case Variable or Constant:
                    if (state == 2)
                    {
                        Concatenation();
                        state = 1;
                        break;
                    }

                    state = 2;
                    break;

                case LeftParen:
                case Function:
                    if (state == 2)
                    {
                        Concatenation();
                        state = 1;
                        break;
                    }

                    outer.Push(level);
                    level = type == LeftParen ? Grouping : Arguments;
                    state = 0;
                    break;

                case RightParen:
                    if (state == 0)
                    {
                        _rescan = true;
                        state = 2;
                        break;
                    }

                    if (state == 1)
                        throw new SyntaxFault(221);
                    switch (level)
                    {
                        case NormalGoto:
                            return;
                        case Arguments:
                        case Grouping:
                            level = outer.Pop();
                            state = 2;
                            break;
                        default:
                            throw new SyntaxFault(224);
                    }

                    break;

                case LeftBracket:
                    if (state != 2)
                        throw new SyntaxFault(222);
                    outer.Push(level);
                    level = Subscripts;
                    state = 0;
                    break;

                case RightBracket:
                    if (state == 0)
                    {
                        _rescan = true;
                        state = 2;
                        break;
                    }

                    if (state == 1)
                        throw new SyntaxFault(221);
                    switch (level)
                    {
                        case Subscripts:
                            level = outer.Pop();
                            state = 2;
                            break;
                        case DirectGoto:
                            return;
                        default:
                            throw new SyntaxFault(225);
                    }

                    break;

                case Unary:
                    if (state == 2)
                        Concatenation();
                    state = 1;
                    break;   // a unary operator after an operand is read again, in state 1, as what follows a concatenation

                case Binary:
                    if (state != 2)
                        throw new SyntaxFault(221);
                    // the right argument of = may be left out, so a terminator after it supplies a null
                    state = _operator == '=' ? 0 : 1;
                    break;

                case Comma:
                    if (state == 0)
                    {
                        _rescan = true;
                        state = 2;
                        break;
                    }

                    if (state == 1)
                        throw new SyntaxFault(221);
                    if (level <= DirectGoto)
                        throw new SyntaxFault(223);
                    state = 0;
                    break;

                default:   // Colon, Semicolon
                    if (state == 0)
                    {
                        _rescan = true;
                        state = 2;
                        break;
                    }

                    if (state == 1)
                        throw new SyntaxFault(221);
                    _rescan = true;
                    switch (level)
                    {
                        case Outer:
                            return;
                        case NormalGoto:
                            throw new SyntaxFault(227);
                        case DirectGoto:
                            throw new SyntaxFault(228);
                        case Subscripts:
                            throw new SyntaxFault(229);
                        default:
                            throw new SyntaxFault(226);
                    }
            }
        }
    }

    // exp04: an operand, a paren, a call or a unary operator where an operator is expected is a concatenation if blanks come before
    // it, else an operator is missing; it is read again as what it is, in state 1 (an operand expected).
    private void Concatenation()
    {
        if (!_blank)
            throw new SyntaxFault(220);
        _rescan = true;
    }

    // scane: the next element. The code of an error is thrown; _start tells where
    private int Read()
    {
        _blank = false;
        if (_rescan)
        {
            _rescan = false;
            _gotoField = false;
            return _type;
        }

        _atEnd = false;
        var text = _text;
        var position = _position;
        if (position == text.Length)
            return Exit(Semicolon);

        _start = position;
        var atStart = true;    // nothing of a name or number is read yet (wb)
        var name = true;       // it is not a number (wc)
        while (true)
        {
            var c = text[position++];
            _position = position;

            if (c is ' ' or '\t' or '\v')
            {
                if (!atStart)
                    return Finish(position - 1, name);
                _start++;
                if (position == text.Length)
                    return Exit(Semicolon);
                _blank = true;
                continue;
            }

            var part = false;    // c goes on a name or number
            if (c is >= '0' and <= '9')
            {
                if (atStart)
                    name = false;
                part = true;
            }
            else if (char.IsAsciiLetter(c))
            {
                if (_gotoField && c is 'F' or 'f' or 'S' or 's')
                    return atStart ? Exit(c is 'F' or 'f' ? FailureGoto : SuccessGoto) : Finish(position - 1, name);
                part = true;
            }
            else if (c == '_')
            {
                if (atStart)
                    throw new SyntaxFault(230);
                part = true;
            }
            else if (c == '.')
            {
                if (!atStart)
                    part = true;
                else
                    return Operator(c);
            }
            else if (c is '+' or '-')
            {
                if (!atStart)
                {
                    if (!name)
                        part = true;
                    else
                        throw new SyntaxFault(233);
                }
                else
                    return Operator(c);
            }

            if (part)
            {
                if (position == text.Length)
                    return Finish(position, name);
                atStart = false;
                continue;
            }

            // everything else ends a name or number
            if (!atStart)
                return Finish(position - 1, name);

            switch (c)
            {
                case '\'' or '"':
                {
                    var close = text.IndexOf(c, position);
                    // the string is closed on the line it opens on
                    var lineEnd = _lineEnds.FirstOrDefault(end => end >= position, -1);
                    if (close < 0 || (lineEnd >= 0 && close >= lineEnd))
                    {
                        _position = lineEnd >= 0 && lineEnd < text.Length ? lineEnd : text.Length;
                        throw new SyntaxFault(232);
                    }

                    _position = close + 1;
                    return Exit(Constant);
                }
                case '(':
                    return Exit(LeftParen);
                case ')':
                    return Exit(RightParen);
                case '[' or '<':
                    return Exit(LeftBracket);
                case ']' or '>':
                    return Exit(RightBracket);
                case ':':
                    return Exit(Colon);
                case ';':
                    Exit(Semicolon);
                    _semicolon = true;
                    _atEnd = false;
                    return Semicolon;
                case ',':
                    return Exit(Comma);
                case '*':
                    // ** followed by a blank or the end is the operator !
                    if (position < text.Length && text[position] == '*' && (position + 1 == text.Length || text[position + 1] is ' ' or '\t' or '\v'))
                        _position = position += 1;
                    return Operator(c);
                case '=' or '?' or '&' or '|' or '@' or '#' or '/' or '%' or '!' or '$' or '~' or '^':
                    return Operator(c);
                default:
                    throw new SyntaxFault(230);
            }
        }
    }

    // scn46: an operator is binary if a blank (or a semicolon, a colon, a closing paren or bracket, or the end) follows it, else it is
    // unary; a binary one needs blanks before it, a unary one needs them unless it follows what lets an operand begin
    private int Operator(char c)
    {
        var position = _position;
        var text = _text;
        var binary = position == text.Length || text[position] is ' ' or '\t' or '\v' or ';' or ':' or ')' or ']' or '>';
        if (!binary && _previous <= UnaryOk)
            return Exit(Unary, c);
        if (!_blank)
            throw new SyntaxFault(233);
        return Exit(binary ? Binary : Unary, c);
    }

    // scn10, scn11: a name or number ends at `end`
    private int Finish(int end, bool name)
    {
        _position = end;
        var token = _text[_start..end];
        if (name)
        {
            // a name that is followed by a left paren is a call of a function
            if (end < _text.Length && _text[end] == '(')
            {
                _position = end + 1;
                return Exit(Function);
            }

            return Exit(Variable);
        }

        if (!IsNumber(token))
            throw new SyntaxFault(231);
        return Exit(Constant);
    }

    private int Exit(int type, char op = '\0')
    {
        _type = type;
        _semicolon = false;
        _atEnd = type == Semicolon;
        _operator = op;
        _previous = type;
        _gotoField = false;
        return type;
    }

    // gtnum: an integer in the 64 bits, or a real in the doubles
    private static bool IsNumber(string token)
    {
        var match = NumberPattern().Match(token);
        if (!match.Success)
            return false;
        if (match.Groups[2].Length == 0 && match.Groups[3].Length == 0)
            return Var.ToInteger(token, out _);
        return Var.ToReal(token, out var real) && !double.IsInfinity(real);
    }

    [GeneratedRegex(@"^(\d+)(\.\d*)?([eEdD][+-]?\d+)?$")]
    private static partial Regex NumberPattern();
}
