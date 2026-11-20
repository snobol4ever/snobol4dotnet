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
    public void TEST_CompileErrorReport_a_missing_include_names_the_control_line_and_the_opening_quote()
    {
        // a control line carries no statement number (eight blanks), and the caret is under the opening quote; nothing runs
        var s = "        OUTPUT = 'before'\n-include 'nosuchfile.sno'\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual(
            Head + "        -include 'nosuchfile.sno'\n" + new string(' ', 17) + "!\nTest.sno(2,10) : ERROR 285 -- include file cannot be opened",
            Run(s));
    }
}
