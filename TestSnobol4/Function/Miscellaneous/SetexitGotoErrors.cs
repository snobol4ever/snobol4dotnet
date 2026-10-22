using Test.TestLexer;

namespace Test.Keywords;

/// <summary>
/// The errors of a goto as sbl treats them. :(ABORT) ends the run with the error that is pending, as the fatal error it was, and with
/// none it is error 36, which a SETEXIT handler takes once; a goto to a label that is not there is error 38, which a handler takes each
/// time while &amp;ERRLIMIT lasts, and :(CONTINUE) from the handler tries the goto again. The expected lines are what sbl -bf printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SetexitGotoErrors
{
    private static string Run(string script) => SetupTests.RunWithInputDirectives("-bf", script).Replace("\r\n", "\n");

    [TestMethod]
    public void TEST_SetexitGotoErrors_abort_in_a_handler_ends_the_run_with_the_pending_error()
    {
        var output = Run("\tSETEXIT(.H)\n\t&ERRLIMIT = 100\n\tN = 0\n\tX = 1 / N\n\tOUTPUT = 'c'\n\t:(END)\nH\tOUTPUT = 'h ' &ERRTYPE\n\t:(ABORT)\nEND\n");
        StringAssert.StartsWith(output, "h 14\n");
        StringAssert.Contains(output, "Test.sno(8) : ERROR 014 -- division caused integer overflow");
        StringAssert.Contains(output, "in statement         8\n");
        StringAssert.Contains(output, "stmts executed       6\n");
    }

    [TestMethod]
    public void TEST_SetexitGotoErrors_abort_with_no_error_pending_is_error_36()
    {
        var output = Run("\tOUTPUT = 'a'\n\t:(ABORT)\n\tOUTPUT = 'b'\nEND\n");
        StringAssert.StartsWith(output, "a\n");
        StringAssert.Contains(output, "Test.sno(2) : ERROR 036 -- goto abort with no preceding error");
        Assert.IsFalse(output.Contains("\nb\n"));
        StringAssert.Contains(output, "stmts executed       2\n");
    }

    [TestMethod]
    public void TEST_SetexitGotoErrors_abort_with_no_error_pending_inside_a_function_is_error_36()
    {
        var output = Run("\tOUTPUT = 'a'\n\tDEFINE('F()')\n\tF()\n\tOUTPUT = 'b'\n\t:(END)\nF\t:(ABORT)\nEND\n");
        StringAssert.StartsWith(output, "a\n");
        StringAssert.Contains(output, "Test.sno(6) : ERROR 036 -- goto abort with no preceding error");
        Assert.IsFalse(output.Contains("\nb\n"));
        StringAssert.Contains(output, "stmts executed       4\n");
    }

    [TestMethod]
    public void TEST_SetexitGotoErrors_error_36_is_taken_by_a_handler_once()
    {
        var output = Run("\t&ERRLIMIT = 100\n\tSETEXIT(.H)\n\tOUTPUT = 'a'\n\t:(ABORT)\n\tOUTPUT = 'b'\n\t:(END)\nH\tOUTPUT = 'h ' &ERRTYPE\n\t:(CONTINUE)\nEND\n");
        StringAssert.StartsWith(output, "a\nh 36\n");
        StringAssert.Contains(output, "Test.sno(4) : ERROR 036 -- goto abort with no preceding error");
        StringAssert.Contains(output, "in statement         8\n");
        StringAssert.Contains(output, "stmts executed       6\n");
    }

    [TestMethod]
    public void TEST_SetexitGotoErrors_a_handler_takes_error_38_while_errlimit_lasts()
    {
        var output = Run("\tSETEXIT(.H)\n\t&ERRLIMIT = 5\n\tN = 0\n\tX = 1 / N :F(NOLAB)\n\tOUTPUT = 'c'\n\t:(END)\nH\tOUTPUT = 'h ' &ERRTYPE\n\tSETEXIT(.H)\n\t:(CONTINUE)\nEND\n");
        StringAssert.StartsWith(output, "h 14\nh 38\nh 38\nh 38\nh 38\n");
        Assert.AreEqual(4, output.Split('\n').Count(line => line == "h 38"));
        StringAssert.Contains(output, "Test.sno(4) : ERROR 038 -- goto undefined label");
        StringAssert.Contains(output, "in line              4\n");
        StringAssert.Contains(output, "in statement         9\n");
        StringAssert.Contains(output, "stmts executed       19\n");
    }
}
