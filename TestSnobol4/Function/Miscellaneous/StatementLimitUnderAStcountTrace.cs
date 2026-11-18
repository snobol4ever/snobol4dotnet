using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// With &amp;STCOUNT traced, the statement limit is checked at every statement against what is left of it, &amp;STLIMIT less &amp;STCOUNT, as
/// in SPITBOL: stgcc breaks out of stmgo on each statement when stcount is traced (or profiling is on), and stgo4 takes one from kvstc,
/// which an assignment to &amp;STLIMIT sets to the new limit less the count. Untraced, the breakout count starts at the limit itself, so the
/// limit-th statement after the assignment is error 244, as before. snobol4dotnet counted from the assignment in both cases, so spitbol_88's
/// GOTOS demo stopped one statement late. The expected answers are sbl -bf's.
/// </summary>
[DoNotParallelize]
[TestClass]
public class StatementLimitUnderAStcountTrace
{
    private static string Run(string program) => SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd();

    // the lines the trace function prints, then the statement the limit is passed at
    [TestMethod]
    public void TEST_StatementLimitUnderAStcountTrace_the_limit_is_what_is_left()
    {
        var output = Run(@"        DEFINE('T()')   :(TE)
T       OUTPUT = 'T ' &STCOUNT ' ' &LASTNO
        EQ(1,2)   :S(RETURN)
        N = N + 1   :(RETURN)
TE      TRACE('STCOUNT', 'KEYWORD', , 'T')
        &TRACE = 1000
        &STLIMIT = 12
A       :(B)
C       :(D)
B       :(C)
D       :(A)
END").Split('\n');
        Assert.AreEqual("T 5 7|T 9 8|in statement         10",
            string.Join("|", output.Where(l => l.StartsWith("T ") || l.StartsWith("in statement"))));
    }

    [TestMethod]
    public void TEST_StatementLimitUnderAStcountTrace_untraced_it_counts_from_the_assignment()
    {
        Assert.AreEqual("j 23 type 244", Run(@"        &ERRLIMIT = 1
        I = 0
L       I = I + 1
        LT(I,10)   :S(L)
        &STLIMIT = 25
        J = 0
M       J = J + 1   :S(M)F(X)
X       OUTPUT = 'j ' J ' type ' &ERRTYPE
END"));
    }
}
