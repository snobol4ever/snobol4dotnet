using Test.TestLexer;

namespace Test.TestCommandLine;

/// <summary>
/// A program with compilation errors does not run (-NOERRORS, the default); -k or the -ERRORS control runs
/// it, every error is reported, the statements after one in error compile with their labels, and reaching
/// the statement in error is error 7. Expected results are sbl's for the same programs.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CompileErrors
{
    private const string SkipsTheError = """
         OUTPUT = 'first'  :(E)
         X = 'a' (
        E OUTPUT = 'last'
        F OUTPUT = 'f'
        END
        """;

    // The report sbl prints on standard output for each statement in error (CEO-1858): the listing page, with its head's identity and
    // clock masked. The -k switch prints the page number flush left; the -ERRORS control does not.
    private static readonly string Head = "\n\n\n<identity>\n<clock>\n\n\n\n" + new string(' ', 112) + "page 1\n\n";
    private const string HeadK = "\n\n\n<identity>\n<clock>\n\n\n\npage 1\n\n";

    // the page for the error in SkipsTheError: statement 2, found at `line`, column 10 (the end of " X = 'a' (")
    private static string SkipsPage(int line) =>
        "2        X = 'a' (\n" + new string(' ', 17) + $"!\nTest.sno({line},10) : ERROR 226 -- syntax error: missing right paren";

    [TestMethod]
    public void TEST_CompileErrors_k_and_ERRORS_run()
    {
        // sbl -bk and -ERRORS print "first last f" after the report: the label E after the statement in error is defined; the
        // control is line 1, so the error is on line 3
        Assert.AreEqual(HeadK + SkipsPage(2) + "\n\nfirst\nlast\nf",
            SetupTests.MaskListingHead(SetupTests.RunWithInputDirectives("-b -k", SkipsTheError)));
        Assert.AreEqual(Head + SkipsPage(3) + "\n\nfirst\nlast\nf",
            SetupTests.MaskListingHead(SetupTests.RunWithInput("-ERRORS\n" + SkipsTheError)));
    }

    [TestMethod]
    public void TEST_CompileErrors_reaching_the_statement_is_error_7()
    {
        // sbl -bk: reaching the statement in error is error 7, which SETEXIT catches like any error
        Assert.AreEqual(HeadK + "4               X = 'a' (\n" + new string(' ', 24) +
            "!\nTest.sno(4,17) : ERROR 226 -- syntax error: missing right paren\n\nfirst\ncaught 7\nafter",
            SetupTests.MaskListingHead(SetupTests.RunWithInputDirectives("-b -k", """
                 &ERRLIMIT = 5
                 SETEXIT(.H)
                 OUTPUT = 'first'
                 X = 'a' (
                 OUTPUT = 'after'  :(END)
         H       OUTPUT = 'caught ' &ERRTYPE  :(CONTINUE)
         END
         """)));
    }
}
