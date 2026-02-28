using System.Text.RegularExpressions;

namespace Snobol4.Common;

/// <summary>
/// Provides pre-compiled regular expressions for SNOBOL4 lexical analysis and parsing.
/// </summary>
/// <remarks>ctorcter
/// This class uses .NET's regular expression source generators to create high-performance,
/// compile-time validated regex patterns. Each pattern is designed to match specific
/// SNOBOL4 language constructs during the lexical analysis and parsing phases.
/// See https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-source-generators
/// </remarks>
internal partial class CompiledRegex
{
    #region Goto Statement Patterns

    /// <summary>
    /// Matches whitespace after a goto statement to detect empty goto fields.
    /// </summary>
    /// <returns>A regex that matches zero or more spaces or tabs from the start of the string.</returns>
    [GeneratedRegex(@"^[ \t]*$")]
    internal static partial Regex AfterGoToPattern();

    /// <summary>
    /// Matches an empty colon (goto with no label).
    /// </summary>
    /// <returns>A regex that matches a colon followed by optional whitespace.</returns>
    [GeneratedRegex(@"^:[ \t]*$")]
    internal static partial Regex EmptyGoToPattern();

    /// <summary>
    /// Matches the first goto statement in a SNOBOL4 line.
    /// </summary>
    /// <returns>
    /// A regex that captures:
    /// Group 1: Optional condition ("s", "S", "f", or "F").
    /// Group 2: Opening delimiter ("(" or "&lt;").
    /// </returns>
    [GeneratedRegex(@"^:[ \t]*([sSfF])?[ \t]*([(<])")]
    internal static partial Regex GoToFirstPattern();

    /// <summary>
    /// Matches the second goto statement in a SNOBOL4 line (when two gotos are present).
    /// </summary>
    /// <returns>
    /// A regex that captures:
    /// Group 1: Optional condition ("s", "S", "f", or "F").
    /// Group 2: Opening delimiter ("(" or "&lt;").
    /// </returns>
    [GeneratedRegex(@"^[ \t]*([sSfF]?)[ \t]*([(<])")]
    internal static partial Regex GoToSecondPattern();

    #endregion

    #region Operator Patterns

    /// <summary>
    /// Matches a binary operator with surrounding whitespace and a following operand.
    /// </summary>
    /// <returns>
    /// A regex that matches operators (~, ?, $, ., !, ^, %, *, /, #, +, @, |, &, =, or **)
    /// followed by whitespace and the start of a right operand.
    /// </returns>
    [GeneratedRegex(@"^([~?$.!^%*/#+@|&=-]|\*\*)[ \t]+[~?$.!^%*/#+@|&=-]*[A-Za-z0-9\""\'\(]")]
    internal static partial Regex BinaryOperatorPattern();

    /// <summary>
    /// Matches the delete operator (equals sign followed by optional whitespace and a closing delimiter).
    /// </summary>
    /// <returns>
    /// A regex that captures:
    /// Group 1: Equals sign with trailing whitespace.
    /// Group 2: Optional closing delimiter (),:,&gt;,]).
    /// </returns>
    [GeneratedRegex(@"^(\=[ \t]*)([),:>\]]?)")]
    internal static partial Regex DeleteOperatorPattern();

    /// <summary>
    /// Matches the beginning of a right operand for implicit operator detection.
    /// </summary>
    /// <returns>
    /// A regex that matches optional unary operators followed by a letter, digit, or literal delimiter.
    /// </returns>
    [GeneratedRegex(@"^[~?$.!^%*/#+@|&=-]*[A-Za-z0-9\""\'\(]")]
    internal static partial Regex RightOperandPattern();

    /// <summary>
    /// Matches a unary operator immediately followed by an operand (no whitespace).
    /// </summary>
    /// <returns>
    /// A regex that captures one or more operator characters followed immediately by
    /// a letter, digit, or opening delimiter.
    /// </returns>
    [GeneratedRegex(@"^([~?$.!^%*/#+@|&=-]+)[A-Za-z0-9\""\'\(]")]
    internal static partial Regex UnaryOperatorPattern();

    #endregion

    #region Identifier and Literal Patterns

    /// <summary>
    /// Matches a SNOBOL4 identifier with an optional following delimiter.
    /// </summary>
    /// <returns>
    /// A regex that captures:
    /// Group 1: The identifier name (letter followed by letters, digits, underscores, or periods).
    /// Group 2: Optional opening delimiter ("(", "&lt;", or "[").
    /// </returns>
    [GeneratedRegex(@"^([a-zA-Z][a-zA-Z0-9_.]*)([\(<\[])?")]
    internal static partial Regex IdentifierPattern();

    /// <summary>
    /// Matches alphabetic keywords in SNOBOL4 statements.
    /// </summary>
    /// <returns>
    /// A regex that matches one or more consecutive letters.
    /// </returns>
    [GeneratedRegex(@"[a-zA-Z]+")]
    internal static partial Regex KeywordPattern();

    /// <summary>
    /// Matches a SNOBOL4 label at the beginning of a line.
    /// </summary>
    /// <returns>
    /// A regex that captures a label starting with a letter or digit,
    /// followed by any non-whitespace characters.
    /// </returns>
    [GeneratedRegex(@"^([A-Za-z\d][^ \t]*)")]
    internal static partial Regex LabelPattern();

    /// <summary>
    /// Matches a numeric literal (integer or real number).
    /// </summary>
    /// <returns>
    /// A regex that captures:
    /// Group 1: Integer part.
    /// Group 2: Optional decimal part.
    /// Group 3: Optional exponent (with sign).
    /// </returns>
    [GeneratedRegex(@"^([0-9]+)(\.[0-9]*)?(([eE][+-]?[0-9]+))?")]
    internal static partial Regex NumericPattern();

    /// <summary>
    /// Matches a string literal enclosed in single or double quotes.
    /// </summary>
    /// <returns>
    /// A regex that captures the string content (without quotes) in named group 's'.
    /// Supports both single-quoted and double-quoted strings.
    /// </returns>
    [GeneratedRegex("^(\"(?<s>[^\"]*)\"|'(?<s>[^']*)')")]
    internal static partial Regex StringLiteralPattern();

    #endregion

    #region Prototype Patterns

    /// <summary>
    /// Matches an array prototype declaration.
    /// </summary>
    /// <returns>
    /// A regex that captures:
    /// Group 1: Array name or first dimension.
    /// Group 2: Optional second part with colon.
    /// Group 3: Second dimension.
    /// </returns>
    [GeneratedRegex("^([^,:]+)(:([^,:]+))?,?")]
    internal static partial Regex ArrayPrototypePattern();

    /// <summary>
    /// Matches a function prototype declaration.
    /// </summary>
    /// <returns>
    /// A regex that captures:
    /// Group 1: Function name.
    /// Group 2: Optional opening parenthesis.
    /// Group 3: Parameter list.
    /// Group 4: Optional closing parenthesis.
    /// Group 5: Remaining text.
    /// </returns>
    [GeneratedRegex(@"^([^(]+)(\(?)([^)]+)(\)?)(.*)$")]
    internal static partial Regex FunctionPrototypePattern();

    /// <summary>
    /// Matches a program-defined data type prototype declaration.
    /// </summary>
    /// <returns>
    /// A regex that captures:
    /// Group 1: Type name.
    /// Group 2: Optional opening parenthesis.
    /// Group 3: Field definitions.
    /// Group 4: Optional closing parenthesis.
    /// </returns>
    [GeneratedRegex(@"^([^(]+)(\(?)([^)]+)(\)?)$")]
    internal static partial Regex ProgramDefinedDataPrototypePattern();

    #endregion

    #region Statement Patterns

    /// <summary>
    /// Matches the END statement in a SNOBOL4 program.
    /// </summary>
    /// <returns>
    /// A regex that matches "END" (case-insensitive) followed by end-of-string or whitespace.
    /// </returns>
    [GeneratedRegex(@"^(END)($|[ \t]+)",RegexOptions.IgnoreCase)]
    internal static partial Regex EndPattern();

    /// <summary>
    /// Matches an entry point label declaration.
    /// </summary>
    /// <returns>
    /// A regex that captures whitespace followed by the entry label name and trailing whitespace.
    /// </returns>
    [GeneratedRegex(@"^([ \t]+)([A-Za-z\d_][^ \t]*)[ \t]*$")]
    internal static partial Regex EntryLabelPattern();

    #endregion

    #region Utility Patterns

    /// <summary>
    /// Matches a quoted string (simple quote pattern).
    /// </summary>
    /// <returns>
    /// A regex that matches text between double quotes.
    /// </returns>
    [GeneratedRegex("""
                    ".+\"
                    """)]
    internal static partial Regex QuotePattern();

    /// <summary>
    /// Matches whitespace (spaces and tabs) at the start of a string.
    /// </summary>
    /// <returns>
    /// A regex that matches one or more spaces or tabs.
    /// </returns>
    [GeneratedRegex(@"^[ \t]+")]
    internal static partial Regex WhiteSpacePattern();

    #endregion
}