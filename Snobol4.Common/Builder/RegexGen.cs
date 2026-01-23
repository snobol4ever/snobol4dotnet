using System.Text.RegularExpressions;

namespace Snobol4.Common;

internal partial class CompiledRegex
{
    // See https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-source-generators?pivots=dotnet-8-0

    [GeneratedRegex(@"^[ \t]*$")]
    internal static partial Regex AfterGoToPattern();

    [GeneratedRegex("^([^,:]+)(:([^,:]+))?,?")]
    internal static partial Regex ArrayPrototypePattern();

    [GeneratedRegex(@"^([~?$.!^%*/#+@|&=-]|\*\*)[ \t]+[~?$.!^%*/#+@|&=-]*[A-Za-z0-9\""\'\(]")]
    internal static partial Regex BinaryOperatorPattern();

    //[GeneratedRegex(@"^([a-zA-Z][a-zA-Z0-9_]*)\(([^) \t]*)\)$")]
    [GeneratedRegex(@"^([^(]+)(\(?)([^)]+)(\)?)$")]
    internal static partial Regex ProgramDefinedDataPrototypePattern();

    [GeneratedRegex(@"^(\=[ \t]*)([),:>\]]?)")]
    internal static partial Regex DeleteOperatorPattern();

    [GeneratedRegex(@"^:[ \t]*$")]
    internal static partial Regex EmptyGoToPattern();

    [GeneratedRegex(@"^(END)($|[ \t]+)",RegexOptions.IgnoreCase)]
    internal static partial Regex EndPattern();

    [GeneratedRegex(@"^([ \t]+)([A-Za-z\d_][^ \t]*)[ \t]*$")]
    internal static partial Regex EntryLabelPattern();

    [GeneratedRegex(@"^([a-zA-Z][a-zA-Z0-9_]*)\(([^)]*)\)(.*)$")]
    internal static partial Regex FunctionPrototypePattern();

    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9_]*$")]
    internal static partial Regex FunctionPrototypeIdentifierPattern();

    [GeneratedRegex(@"^:[ \t]*([sSfF])?[ \t]*([(<])")]
    internal static partial Regex GoToFirstPattern();

    [GeneratedRegex(@"^[ \t]*([sSfF]?)[ \t]*([(<])")]
    internal static partial Regex GoToSecondPattern();

    [GeneratedRegex(@"^([a-zA-Z][a-zA-Z0-9_.]*)([\(<\[])?")]
    internal static partial Regex IdentifierPattern();

    [GeneratedRegex(@"[a-zA-Z]+")]
    internal static partial Regex KeywordPattern();

    [GeneratedRegex(@"^([A-Za-z\d][^ \t]*)")]
    internal static partial Regex LabelPattern();

    [GeneratedRegex(@"^([0-9]+)(\.[0-9]*)?(([eE][+-]?[0-9]+))?")]
    internal static partial Regex NumericPattern();

    [GeneratedRegex("""
                    ".+\"
                    """)]
    internal static partial Regex QuotePattern();

    [GeneratedRegex(@"^[~?$.!^%*/#+@|&=-]*[A-Za-z0-9\""\'(]")]
    internal static partial Regex RightOperandPattern();

    [GeneratedRegex("^(\"(?<s>[^\"]*)\"|'(?<s>[^']*)')")]
    internal static partial Regex StringLiteralPattern();

    [GeneratedRegex(@"^([~?$.!^%*/#+@|&=-]+)[A-Za-z0-9\""\'\(]")]
    internal static partial Regex UnaryOperatorPattern();

    [GeneratedRegex(@"^[ \t]+")]
    internal static partial Regex WhiteSpacePattern();
}