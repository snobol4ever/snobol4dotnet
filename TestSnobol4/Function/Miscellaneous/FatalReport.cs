using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A run-time error that ends the run prints SPITBOL's report on standard output, where sbl writes it (CEO-1858): three
/// empty lines, "file(line) : ERROR nnn -- text", five empty lines, then in file / in line / in statement / stmts executed /
/// execution time msec / REGENERATIONS / memory used (bytes) / memory left (bytes) labelled to column 21, and an empty line.
/// Every expected text below is what sbl -b printed for the same program saved as Test.sno. The run's time, regenerations and
/// memory cannot be equal between two engines, so the tests mask those four numbers as the harness's mask does (ALL.mask);
/// stmts executed is deterministic in sbl and stays exact.
/// </summary>
[DoNotParallelize]
[TestClass]
public class FatalReport
{
    private static string Run(string script) => SetupTests.MaskReport(SetupTests.RunWithInput(script));

    [TestMethod]
    public void TEST_FatalReport_runtime_error_prints_the_report_on_stdout()
    {
        var s = @"        OUTPUT = 'before'
        X = FOO(1)
        OUTPUT = 'after'
END";
        Assert.AreEqual(
            "before\n\n\n\nTest.sno(2) : ERROR 022 -- undefined function called\n\n\n\n\n\n" +
            "in file              Test.sno\nin line              2\nin statement         2\nstmts executed       2\n" +
            "execution time msec  -\nREGENERATIONS        -\nmemory used (bytes)  -\nmemory left (bytes)  -",
            Run(s));
    }

    [TestMethod]
    public void TEST_FatalReport_names_the_first_line_of_a_continued_statement()
    {
        // sbl: the comment is no statement, the blank line is one (statement 3 is the continued assignment), the error is on
        // its first physical line, and two statements have executed
        var s = "* a comment\n\n        OUTPUT = 'x'\n        Y = 1 +\n+          FOO(2)\n        OUTPUT = 'not reached'\nEND";
        Assert.AreEqual(
            "x\n\n\n\nTest.sno(4) : ERROR 022 -- undefined function called\n\n\n\n\n\n" +
            "in file              Test.sno\nin line              4\nin statement         3\nstmts executed       2\n" +
            "execution time msec  -\nREGENERATIONS        -\nmemory used (bytes)  -\nmemory left (bytes)  -",
            Run(s));
    }

    [TestMethod]
    public void TEST_FatalReport_an_error_in_a_function_body_names_that_statement()
    {
        var s = @"        DEFINE('F()')
        OUTPUT = 'a'
        F()
        OUTPUT = 'not reached'  :(END)
F       OUTPUT = 'in F'
        X = BAR()
END";
        Assert.AreEqual(
            "a\nin F\n\n\n\nTest.sno(6) : ERROR 022 -- undefined function called\n\n\n\n\n\n" +
            "in file              Test.sno\nin line              6\nin statement         6\nstmts executed       5\n" +
            "execution time msec  -\nREGENERATIONS        -\nmemory used (bytes)  -\nmemory left (bytes)  -",
            Run(s));
    }

    [TestMethod]
    public void TEST_FatalReport_an_error_the_program_survives_prints_no_report()
    {
        // &ERRLIMIT above zero lets the run continue, and sbl prints nothing for the error
        var s = @"        OUTPUT = 'before'
        &ERRLIMIT = 3
        X = FOO(1)
        OUTPUT = 'mid'
        Y = BAR(2)
        OUTPUT = 'after'
END";
        Assert.AreEqual("before\nmid\nafter", Run(s));
    }
}
