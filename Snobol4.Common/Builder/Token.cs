using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugToken()}")]
internal class Token
{
    #region Members

    internal Type TokenType { get; set; } = Type.NULL;
    internal string MatchedString { get; set; }
    internal double DoubleValue { get; set; }
    internal long IntegerValue { get; set; }
    internal int Index { get; set; }

    #endregion

    #region Enumerations

    public enum Type
    {
        BINARY_AMPERSAND = 1,
        BINARY_AT = 2,
        BINARY_CARET = 3,
        BINARY_CONCAT = 4,
        BINARY_DOLLAR = 5,
        BINARY_EQUAL = 6,
        BINARY_HASH = 7,
        BINARY_MINUS = 8,
        BINARY_PERCENT = 9,
        BINARY_PERIOD = 10,
        BINARY_PIPE = 11,
        BINARY_PLUS = 12,
        BINARY_QUESTION = 13,
        BINARY_SLASH = 14,
        BINARY_STAR = 15,
        BINARY_TILDE = 16,
        COLON = 17,
        COMMA = 18,
        COMMA_CHOICE = 19,
        FAILURE_GOTO = 20,
        IDENTIFIER_ARRAY_OR_TABLE = 21,
        IDENTIFIER_FUNCTION = 22,
        //IDENTIFIER_TABLE = 23,
        IDENTIFIER = 23,
        INTEGER = 24,
        L_ANGLE = 25,
        L_ANGLE_FAILURE = 26,
        L_ANGLE_SUCCESS = 27,
        L_ANGLE_UNCONDITIONAL = 28,
        L_PAREN_CHOICE = 29,
        L_PAREN_FAILURE = 30,
        L_PAREN_FUNCTION = 31,
        L_PAREN_SUCCESS = 32,
        L_PAREN_UNCONDITIONAL = 33,
        L_SQUARE = 34,
        NULL = 35,
        R_ANGLE = 36,
        R_ANGLE_FAILURE = 37,
        R_ANGLE_SUCCESS = 38,
        R_ANGLE_UNCONDITIONAL = 39,
        R_PAREN_CHOICE = 40,
        R_PAREN_FAILURE = 41,
        R_PAREN_FUNCTION = 42,
        R_PAREN_SUCCESS = 43,
        R_PAREN_UNCONDITIONAL = 44,
        R_SQUARE = 45,
        REAL = 46,
        SPACE = 47,
        STRING = 48,
        SUCCESS_GOTO = 49,
        //       UNARY_NOT  = 50,
        UNARY_OPERATOR = 51,
        UNARY_STAR = 52,
        EXPRESSION = 53
    }

    #endregion

    #region Constructors

                internal Token()
    {
        MatchedString = "";
    }

                            internal Token(Type type, string match, int index)
    {
        TokenType = type;
        MatchedString = match;
        Index = index;
    }

                                internal Token(Type type, string match, int index, long value)
    {
        TokenType = type;
        MatchedString = match;
        Index = index;
        IntegerValue = value;
    }

                                internal Token(Type type, string match, int index, double value)
    {
        TokenType = type;
        MatchedString = match;
        Index = index;
        DoubleValue = value;
    }

    #endregion

    #region Methods

    //internal string ToString(bool detailed = false)
    //{
    //    if (detailed)
    //        return "Type: " + TokenType + " <" + MatchedString;
    //    return TokenType + new string(' ', 32 - TokenType.ToString().Length) +
    //           "<" + MatchedString + ">" +
    //           " (" + IntegerValue + ") (" + DoubleValue + ") [" + Index + "]";
    //}

    #endregion

    #region Debugging

    public string DebugToken()
    {
        return $"{TokenType} '{MatchedString}' {DoubleValue}D {IntegerValue}L [{Index}]";
    }

    #endregion
}