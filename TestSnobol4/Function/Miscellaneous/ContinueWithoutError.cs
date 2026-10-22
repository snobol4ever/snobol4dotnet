using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// :(CONTINUE) with no error pending is error 37, and with &amp;ERRLIMIT above 0 and no handler armed it still ends the run with the report,
/// as sbl -b prints it; the console ended the run in silence. A handler armed takes the error as it takes any other.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ContinueWithoutError
{
    [TestMethod]
    public void TEST_ContinueWithoutError_ends_the_run_with_the_report_under_an_error_limit()
    {
        var output = SetupTests.MaskReport(SetupTests.RunWithInput("        &ERRLIMIT = 10\n        OUTPUT = 'a'\n        :(CONTINUE)\n        OUTPUT = 'b'\nEND"));
        StringAssert.StartsWith(output, "a\n");
        StringAssert.Contains(output, "Test.sno(3) : ERROR 037 -- goto continue with no preceding error");
        StringAssert.Contains(output, "in statement         3\n");
        Assert.IsFalse(output.Contains('b') && output.Contains("\nb"), output);
    }

    [TestMethod]
    public void TEST_ContinueWithoutError_is_taken_by_an_armed_handler()
    {
        // sbl -b: the handler runs once; its :(CONTINUE) goes back to the goto that failed, which is error 37 again, with the handler spent
        var output = SetupTests.RunWithInput("        &ERRLIMIT = 10\n        SETEXIT(.H)\n        :(CONTINUE)\n        OUTPUT = 'b'\n        :(END)\nH       OUTPUT = 'handler ' &ERRTYPE  :(CONTINUE)\nEND").Replace("\r\n", "\n");
        StringAssert.StartsWith(output, "handler 37\n");
        // (sbl's report names the line of the goto, 3; the run here names the handler's :(CONTINUE), the left item of the cursor)
        StringAssert.Contains(output, "ERROR 037 -- goto continue with no preceding error");
    }
}
