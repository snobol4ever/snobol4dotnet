using Test.TestLexer;

namespace Test.Keywords;

/// <summary>
/// &STLIMIT as sbl keeps it: a limit set at or below the statements already counted is never reached (until it is set again), and passing a limit ends the run with error 244 whatever &ERRLIMIT allows (it ended without a word with &ERRLIMIT above 0). The lines are what sbl -b printed for the same program, the report (sbl copies it to standard error too, which the test does not read).
/// </summary>
[DoNotParallelize]
[TestClass]
public class StatementLimit
{
    [TestMethod]
    public void TEST_StatementLimit_stlimit()
    {
        var s = @"        &ERRLIMIT = 100
        A = 1
        A = 2
        A = 3
        A = 4
        A = 5
        OUTPUT = 'count ' &STCOUNT ' limit ' &STLIMIT
        &STLIMIT = 3
        OUTPUT = 'after set ' &STLIMIT ' err ' &ERRTYPE
        &STLIMIT = 100
        OUTPUT = 'after reset ' &STLIMIT ' err ' &ERRTYPE
        &STLIMIT = 20
        I = 0
L       I = I + 1
        LT(I, 100)   :S(L)
        OUTPUT = 'after loop ' I ' ' &STCOUNT ' err ' &ERRTYPE
END";
        var output = SetupTests.RunWithInput(s).Replace("\r\n", "\n");
        StringAssert.StartsWith(output, "count 7 limit 2147483647\nafter set 3 err 0\nafter reset 100 err 0\n");
        StringAssert.Contains(output, "Test.sno(14) : ERROR 244 -- statement count exceeds value of stlimit keyword");
        StringAssert.Contains(output, "stmts executed       20");
    }

    [TestMethod]
    public void TEST_StatementLimit_counts_from_the_statement_that_sets_it()
    {
        // sbl -b: the limit-th statement after the one that sets &STLIMIT is the one that ends the run (&STCOUNT, which went on
        // counting, reads 4 to 7 on the way); the report and the dump then show the count as the limit and the limit ten higher
        var output = SetupTests.RunWithInput(@"        &DUMP = 1
        X = 1
        &STLIMIT = 5
L       OUTPUT = 'x' &STCOUNT  :(L)
END").Replace("\r\n", "\n");
        StringAssert.StartsWith(output, "x4\nx5\nx6\nx7\n");
        Assert.AreEqual(4, output.Split('\n').Count(line => line.StartsWith('x')));
        StringAssert.Contains(output, "Test.sno(4) : ERROR 244 -- statement count exceeds value of stlimit keyword");
        StringAssert.Contains(output, "in statement         4\n");
        StringAssert.Contains(output, "stmts executed       5\n");
        StringAssert.Contains(output, "&STCOUNT = 5\n&STLIMIT = 15\n");
    }

    [TestMethod]
    public void TEST_StatementLimit_a_limit_equal_to_the_count_is_reached()
    {
        // sbl -b: set at the second statement to 2, the limit lets one more statement run and ends the run at the next
        var output = SetupTests.RunWithInput(@"        X = 1
        &STLIMIT = 2
L       OUTPUT = 'x' &STCOUNT  :(L)
END").Replace("\r\n", "\n");
        StringAssert.StartsWith(output, "x3\n");
        Assert.AreEqual(1, output.Split('\n').Count(line => line.StartsWith('x')));
        StringAssert.Contains(output, "Test.sno(3) : ERROR 244 -- statement count exceeds value of stlimit keyword");
        StringAssert.Contains(output, "stmts executed       2\n");
    }
}
