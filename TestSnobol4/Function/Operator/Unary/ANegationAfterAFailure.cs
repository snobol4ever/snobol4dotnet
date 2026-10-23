using Test.TestLexer;

namespace Test.Operator.Unary;

/// <summary>
/// A negation negates only its own operand's failure, as in SPITBOL, where evaluation stops at the first failure: in
/// REVERSE(' 12 ') * -~1 * ~6 the statement fails at ~1, and the ~ of 6 does not turn that failure into a success, nor does
/// ~RTAB(0) in 'f' / (~7) / ~RTAB(0). dotnet remembered where the failure began as a stack position, and the operators after it, each
/// taking its operands off and leaving one failure value, moved that value below the position, so the later negation took the failure
/// for its own operand's and succeeded. The failure value now carries the position down with it (PushFailedValue). Found by the
/// infinite_snobol4 generator (seeds 250376 and 815656). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ANegationAfterAFailure
{
    [TestMethod]
    public void TEST_ANegationAfterAFailure_does_not_negate_an_earlier_failure()
    {
        var program = @"        &ERRLIMIT = 10
        V = REVERSE(' 12 ') * -~1 * ~6                  :S(Y1)
        OUTPUT = '1 F'                                  :(E1)
Y1      OUTPUT = '1 S [' V ']'
E1      V = 'f' / (~7) / ~RTAB(0)                       :S(Y2)
        OUTPUT = '2 F'                                  :(E2)
Y2      OUTPUT = '2 S ' DATATYPE(V)
E2      V = SIZE(~1) ~2                                 :S(Y3)
        OUTPUT = '3 F'                                  :(E3)
Y3      OUTPUT = '3 S [' V ']'
E3      V = ~1 + 2 ~3                                   :S(Y4)
        OUTPUT = '4 F'                                  :(E4)
Y4      OUTPUT = '4 S [' V ']'
E4      V = EVAL(""REVERSE(' 12 ') * -~1 * ~6"")         :S(Y5)
        OUTPUT = '5 F'                                  :(E5)
Y5      OUTPUT = '5 S [' V ']'
E5      V = (~1 * ~2, 'alt')                            :S(Y6)
        OUTPUT = '6 F'                                  :(E6)
Y6      OUTPUT = '6 S [' V ']'
E6      V = ~(~1 * ~2)                                  :S(Y7)
        OUTPUT = '7 F'                                  :(E7)
Y7      OUTPUT = '7 S [' V ']'
E7      V = DUPL(~1, 2) ~(~3)                           :S(Y8)
        OUTPUT = '8 F'                                  :(E8)
Y8      OUTPUT = '8 S [' V ']'
E8
END";
        Assert.AreEqual(@"1 F
2 F
3 F
4 F
5 F
6 S [alt]
7 S []
8 F", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
