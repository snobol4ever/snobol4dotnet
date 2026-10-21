using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// :(SCONTINUE) resumes only after a user interrupt (error 320), which a run here never has: with an error pending it is error 331, with
/// none 321, and both are fatal, so a SETEXIT handler still takes them but the third fatal error ends the run whatever the handler
/// says. And after a call returns, the call's last statement is the last executed: the rest of the calling statement, and a statement
/// a STCOUNT trace interrupts, reads it as &amp;LASTNO. Every expected text is what sbl -b printed for the same program saved as Test.sno,
/// with the run summary's figures masked.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ScontinueAndLastno
{
    [TestMethod]
    public void TEST_ScontinueAndLastno_scontinue_after_an_error_is_331_and_the_third_fatal_error_ends_the_run()
    {
        var s = "        SETEXIT(.F)\n        &ERRLIMIT = 10\n        &Z = 1\n        OUTPUT = 'after'\n        :(END)\nF       OUTPUT = 'err ' &ERRTYPE ' ' &ERRTEXT\n        SETEXIT(.F)\n        &ERRLIMIT = 10\n        :(SCONTINUE)\nEND";
        Assert.AreEqual("err 251 keyword operand is not name of defined keyword\nerr 331 goto scontinue with no user interrupt\nerr 331 goto scontinue with no user interrupt\n\n\n\nTest.sno(9) : ERROR 331 -- goto scontinue with no user interrupt\n\n\n\n\n\nin file              Test.sno\nin line              9\nin statement         9\nstmts executed       15\nexecution time msec  -\nREGENERATIONS        -\nmemory used (bytes)  -\nmemory left (bytes)  -", SetupTests.MaskReport(SetupTests.RunWithInput(s)));
    }

    [TestMethod]
    public void TEST_ScontinueAndLastno_scontinue_with_no_error_pending_is_321()
    {
        var s = "        SETEXIT(.F)\n        &ERRLIMIT = 10\n        :(SCONTINUE)\n        OUTPUT = 'after'\n        :(END)\nF       OUTPUT = 'err ' &ERRTYPE\n        :(CONTINUE)\nEND";
        // the handler is taken once; the retaken SCONTINUE is fatal again, and the third fatal error ends the run with its report
        // (sbl's report names the line of the goto, which the run here names by the handler's :(CONTINUE): only its number is held)
        var output = SetupTests.RunWithInput(s);
        StringAssert.StartsWith(output, "err 321\n");
        StringAssert.Contains(output, "ERROR 321 -- goto scontinue with no preceding error");
    }

    [TestMethod]
    public void TEST_ScontinueAndLastno_a_trace_function_s_last_statement_is_the_last_one_executed()
    {
        var s = "        DEFINE('S()')\n        TRACE('STCOUNT','KEYWORD',,.S)\n        &TRACE = 1000\n        OUTPUT = 'a ' &LASTNO\n        OUTPUT = 'b ' &LASTNO\n        :(END)\nS       OUTPUT = 'S ' &LASTNO ' ' &STNO\n        :(RETURN)\nEND";
        Assert.AreEqual("S 4 7\na 8\nS 5 7\nb 8\nS 6 7\nS 9 7", SetupTests.MaskReport(SetupTests.RunWithInput(s)));
    }

    [TestMethod]
    public void TEST_ScontinueAndLastno_a_one_statement_trace_function()
    {
        var s = "        DEFINE('S()')\n        TRACE('STCOUNT','KEYWORD',,.S)\n        &TRACE = 1000\n        OUTPUT = 'a ' &LASTNO\n        OUTPUT = 'b ' &LASTNO\n        :(END)\nS       OUTPUT = 'S ' &LASTNO ' ' &STNO     :(RETURN)\nEND";
        Assert.AreEqual("S 4 7\na 7\nS 5 7\nb 7\nS 6 7\nS 8 7", SetupTests.MaskReport(SetupTests.RunWithInput(s)));
    }

    [TestMethod]
    public void TEST_ScontinueAndLastno_the_rest_of_a_calling_statement_reads_the_calls_last_statement()
    {
        var s = "        DEFINE('G()')\n        X = G() ' ' &LASTNO\n        OUTPUT = X\n        OUTPUT = 'next ' &LASTNO\n        :(END)\nG       Z = 5\n        G = 'g'\n        :(RETURN)\nEND";
        Assert.AreEqual("g 8\nnext 3", SetupTests.MaskReport(SetupTests.RunWithInput(s)));
    }
}
