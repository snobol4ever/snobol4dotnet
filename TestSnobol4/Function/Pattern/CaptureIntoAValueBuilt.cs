using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A capture into a call that hands back a value by RETURN (P . H(2), P $ H(2), @H(2)) is error 21 as the pattern is built, as in SPITBOL: the
/// call is made for a name when the operator's operands are evaluated. The pattern was built and the error came only when a match assigned.
/// Found by the infinite_snobol4 generator ('ab' ? LEN(1) (&amp;CASE = 1) LEN(1) . h(2)). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CaptureIntoAValueBuilt
{
    [TestMethod]
    public void TEST_CaptureIntoAValueBuilt_is_error_21_as_the_pattern_is_built()
    {
        var program = @"        &ERRLIMIT = 10
        DEFINE('h(n)')                              :(he)
h       h = n + 1                                   :(RETURN)
he      P = LEN(1) . h(2)                           :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE ' ' DATATYPE(P)    :(E1)
Y1      OUTPUT = '1 S ' DATATYPE(P)
E1      P = 'x'
        P = LEN(1) $ h(2)                           :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE ' ' DATATYPE(P)    :(E2)
Y2      OUTPUT = '2 S ' DATATYPE(P)
E2      P = 'x'
        P = @h(2)                                   :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE ' ' DATATYPE(P)    :(E3)
Y3      OUTPUT = '3 S ' DATATYPE(P)
E3      V = EVAL(""'ab' ? @h(2)"")                    :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                    :(E4)
Y4      OUTPUT = '4 S'
E4
END";
        Assert.AreEqual(@"1 F 21 STRING
2 F 21 STRING
3 F 21 STRING
4 F 21", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
