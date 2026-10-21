using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A statement in error is reported on standard output as sbl reports it (CEO-1858), on the listing page: the head, then for
/// each statement the physical line it was detected on with the number of its statement in an eight-column field and its tabs
/// kept, a line that keeps the tabs and puts ! under the error column (empty when the error has none), the line
/// "file(line,col) : ERROR nnn -- text", and an empty line. Execution is suppressed. Every expected text below is what sbl -b
/// printed for the same program saved as Test.sno, with the head's identity and clock lines masked.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CompileErrorReport
{
    private static readonly string Head = "\n\n\n<identity>\n<clock>\n\n\n\n" + new string(' ', 112) + "page 1\n\n";

    private static string Run(string script) => SetupTests.MaskListingHead(SetupTests.RunWithInput(script));

    [TestMethod]
    public void TEST_CompileErrorReport_a_syntax_error_prints_the_listing_page_on_stdout()
    {
        var s = "        OUTPUT = 'before'\n        X = (1 +\nEND";
        Assert.AreEqual(
            Head + "2               X = (1 +\n" + new string(' ', 23) + "!\nTest.sno(2,16) : ERROR 221 -- syntax error: missing operand",
            Run(s));
    }

    [TestMethod]
    public void TEST_CompileErrorReport_keeps_tabs_and_numbers_each_erring_statement()
    {
        // statement 2 compiles and is not listed; the column counts a tab as one character
        var s = "\tL\tA\t=  \\INTEGER(I)\t:(RETURN)\n        OUTPUT = 'ok'\n\tV\tB\t=  \\INTEGER(V)\nEND";
        Assert.AreEqual(
            Head +
            "1       \tL\tA\t=  \\INTEGER(I)\t:(RETURN)\n        \t \t \t   !\nTest.sno(1,9) : ERROR 230 -- syntax error: illegal character\n\n" +
            "3       \tV\tB\t=  \\INTEGER(V)\n        \t \t \t   !\nTest.sno(3,9) : ERROR 230 -- syntax error: illegal character",
            Run(s));
    }

    [TestMethod]
    public void TEST_CompileErrorReport_a_second_statement_on_a_line_reports_the_column_in_the_line()
    {
        // the listing line carries the number of the line's first statement, and the column is in the physical line
        var s = "        A = 1; B = (2 +\n        OUTPUT = 'x'\nEND";
        Assert.AreEqual(
            Head + "1               A = 1; B = (2 +\n" + new string(' ', 30) + "!\nTest.sno(1,23) : ERROR 221 -- syntax error: missing operand",
            Run(s));
    }

    [TestMethod]
    public void TEST_CompileErrorReport_a_continued_statement_reports_the_line_where_it_was_detected()
    {
        // missing right paren is found at the end of the statement: its last physical line, the last character
        var s = "        OUTPUT = 'a'\n        X = 1 +\n+       (2 +\n+       3\nEND";
        Assert.AreEqual(
            Head + "2       +       3\n" + new string(' ', 16) + "!\nTest.sno(4,9) : ERROR 226 -- syntax error: missing right paren",
            Run(s));
    }

    [TestMethod]
    public void TEST_CompileErrorReport_a_missing_include_names_the_control_line_and_the_opening_quote()
    {
        // a control line carries no statement number (eight blanks), and the caret is under the opening quote; nothing runs
        var s = "        OUTPUT = 'before'\n-include 'nosuchfile.sno'\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual(
            Head + "        -include 'nosuchfile.sno'\n" + new string(' ', 17) + "!\nTest.sno(2,10) : ERROR 285 -- include file cannot be opened",
            Run(s));
    }

    [TestMethod]
    public void TEST_CompileErrorReport_an_error_without_a_column_has_an_empty_caret_line()
    {
        // a continuation line after a comment continues nothing: error 214, named by its line only
        var s = "* a comment\n.       OUTPUT = 'x'\n        OUTPUT = 'y'\nEND";
        Assert.AreEqual(
            Head + "1       .       OUTPUT = 'x'\n\nTest.sno(2) : ERROR 214 -- bad label or misplaced continuation line",
            Run(s));
    }
}
