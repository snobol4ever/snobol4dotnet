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
}
