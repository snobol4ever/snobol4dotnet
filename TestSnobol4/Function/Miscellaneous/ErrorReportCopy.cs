using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SPITBOL copies the lines of an error report to its interactive channel, standard error, as it prints them, unless its standard
/// printer is the terminal; with standard output in a file or a pipe it writes each such line twice, and a harness that merges the
/// two streams (the Budne runner's 2&gt;&amp;1) reads each twice, the pair side by side. Every expected text is what sbl -b printed for
/// the same program saved as Test.sno, with the run summary's figures and the listing head masked.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ErrorReportCopy
{
    private static string Head = "\n\n\n<identity>\n<clock>\n\n\n\n" + new string(' ', 112) + "page 1\n\n";

    // The two streams, apart and as one writer that holds both (2>&1)
    private static (string Out, string Error, string Merged) Run(string script)
    {
        var oldOut = Console.Out;
        var oldError = Console.Error;
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        using var both = new StringWriter();
        var merged = TextWriter.Synchronized(both);
        try
        {
            Console.SetOut(stdout);
            Console.SetError(stderr);
            SetupTests.SetupScript("-b", script);
            Console.SetOut(merged);
            Console.SetError(merged);
            SetupTests.SetupScript("-b", script);
        }
        finally
        {
            Console.SetOut(oldOut);
            Console.SetError(oldError);
        }

        string Mask(string text) => SetupTests.MaskOracle(text).TrimEnd();
        return (Mask(stdout.ToString()), Mask(stderr.ToString()), Mask(both.ToString()));
    }

    [TestMethod]
    public void TEST_ErrorReportCopy_a_fatal_report_is_read_twice_when_the_streams_are_one()
    {
        var (_, _, merged) = Run("        OUTPUT = 'before'\n        A = 'a'\n        X = A + 1\n        OUTPUT = 'after'\nEND");
        Assert.AreEqual(
            "before\n\n\n\n" +
            "Test.sno(3) : ERROR 001 -- addition left operand is not numeric\nTest.sno(3) : ERROR 001 -- addition left operand is not numeric\n\n\n\n\n\n\n\n" +
            "in file              Test.sno\nin file              Test.sno\nin line              3\nin line              3\n" +
            "in statement         3\nin statement         3\nstmts executed       3\nstmts executed       3\n" +
            "execution time msec  -\nexecution time msec  -\nREGENERATIONS        -\nREGENERATIONS        -\n" +
            "memory used (bytes)  -\nmemory left (bytes)  -",
            merged);
    }

    [TestMethod]
    public void TEST_ErrorReportCopy_standard_error_holds_the_report_without_the_memory_figures()
    {
        var (_, error, _) = Run("        OUTPUT = 'before'\n        A = 'a'\n        X = A + 1\n        OUTPUT = 'after'\nEND");
        Assert.AreEqual(
            "Test.sno(3) : ERROR 001 -- addition left operand is not numeric\n\n\n" +
            "in file              Test.sno\nin line              3\nin statement         3\nstmts executed       3\n" +
            "execution time msec  -\nREGENERATIONS        -",
            error);
    }

    [TestMethod]
    public void TEST_ErrorReportCopy_a_statement_in_error_is_read_twice_when_the_streams_are_one()
    {
        var (_, _, merged) = Run("        OUTPUT = 'before'\n        X = (1 +\nEND");
        Assert.AreEqual(
            Head + "2               X = (1 +\n2               X = (1 +\n" + new string(' ', 23) + "!\n" + new string(' ', 23) + "!\n" +
            "Test.sno(2,16) : ERROR 221 -- syntax error: missing operand\nTest.sno(2,16) : ERROR 221 -- syntax error: missing operand",
            merged);
    }

    [TestMethod]
    public void TEST_ErrorReportCopy_standard_error_holds_the_page_of_a_statement_in_error_without_the_head()
    {
        var (output, error, _) = Run("        OUTPUT = 'before'\n        X = (1 +\nEND");
        Assert.AreEqual(
            "2               X = (1 +\n" + new string(' ', 23) + "!\nTest.sno(2,16) : ERROR 221 -- syntax error: missing operand",
            error);
        Assert.AreEqual(
            Head + "2               X = (1 +\n" + new string(' ', 23) + "!\nTest.sno(2,16) : ERROR 221 -- syntax error: missing operand",
            output);
    }
}
