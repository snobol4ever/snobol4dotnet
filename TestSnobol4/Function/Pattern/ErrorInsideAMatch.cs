using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// An error a pattern node raises while the match scans (SPAN(*Y) with Y null is 56, LEN(*'z') is 47) ends the whole match, as in SPITBOL: the
/// statement fails once, &ERRLIMIT drops by one, and an immediate assignment made before the error stands. The node failed alone and the scan
/// went on to the next cursor, raising the error again at each until &ERRLIMIT ran out. An error inside a function the scan called fails only
/// that function's statement. Found by the infinite_snobol4 generator ('ykm' ? *v SPAN(*Y)). Every expected line is what sbl -bf printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ErrorInsideAMatch
{
    [TestMethod]
    public void TEST_ErrorInsideAMatch_the_error_ends_the_match_once()
    {
        var program = @"        &ERRLIMIT = 10
        'ykm' ? (LEN(1) $ X) SPAN(*Y)               :S(Y1)
        OUTPUT = '1 F X=' X ' lim=' &ERRLIMIT ' t=' &ERRTYPE :(E1)
Y1      OUTPUT = '1 S X=' X
E1      X =
        'ykm' ? (LEN(1) $ X) LEN(*'z')              :S(Y2)
        OUTPUT = '2 F X=' X ' lim=' &ERRLIMIT ' t=' &ERRTYPE :(E2)
Y2      OUTPUT = '2 S X=' X
E2      X =
        'ykm' ? (LEN(1) . X) SPAN(*Y)               :S(Y3)
        OUTPUT = '3 F X=' X ' lim=' &ERRLIMIT ' t=' &ERRTYPE :(E3)
Y3      OUTPUT = '3 S X=' X
E3      X =
        'ykm' ? (LEN(1) $ X) ('q' | SPAN(*Y))       :S(Y4)
        OUTPUT = '4 F X=' X ' lim=' &ERRLIMIT ' t=' &ERRTYPE :(E4)
Y4      OUTPUT = '4 S X=' X
E4      R = EVAL(""'ykm' ? (LEN(1) $ X) SPAN(*Y)"")    :S(Y5)
        OUTPUT = '5 F X=' X ' lim=' &ERRLIMIT ' t=' &ERRTYPE :(END)
Y5      OUTPUT = '5 S X=' X
END";
        Assert.AreEqual(@"1 F X=y lim=9 t=56
2 F X=y lim=8 t=47
3 F X= lim=7 t=56
4 F X=y lim=6 t=56
5 F X=y lim=5 t=56", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_ErrorInsideAMatch_an_error_in_a_called_function_fails_only_its_statement()
    {
        var program = @"        &ERRLIMIT = 10
        A = 'a'
        DEFINE('G()')                               :(GE)
G       G = 1 + A
        G = 'k'                                     :(RETURN)
GE      'ykm' ? (LEN(1) $ X) *G()                   :S(Y1)
        OUTPUT = '1 F X=' X ' lim=' &ERRLIMIT ' t=' &ERRTYPE :(E1)
Y1      OUTPUT = '1 S X=' X ' lim=' &ERRLIMIT ' t=' &ERRTYPE
E1      'ykm' ? (LEN(1) $ X) 'm' *G()               :S(Y2)
        OUTPUT = '2 F X=' X ' lim=' &ERRLIMIT ' t=' &ERRTYPE :(END)
Y2      OUTPUT = '2 S X=' X ' lim=' &ERRLIMIT ' t=' &ERRTYPE
END";
        Assert.AreEqual(@"1 S X=y lim=9 t=2
2 F X=m lim=8 t=2", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_ErrorInsideAMatch_under_a_negation_leaves_the_statement_whole()
    {
        // sbl -bf: an error that ends a match inside the operand of a negation is that operand's failure, which the negation takes, and
        // the assignment made before it stands (a, b); in the code of an EVAL the error ends the evaluation, which fails (c); without the
        // negation the statement fails (d). The console left the error's value on the stack under the match's failure, so the statement's
        // own assignment took E for its target and E was null (a, b), and the error was a plain failure, which the negation took (cs)
        // (found by the infinite_snobol4 campaign at scope 12, seed0 20261106, batch 6 of kind=match, line 421)
        var build = SetupTests.SetupScript("-bf", @"        &ERRLIMIT = 20
        N = -1
        X = ~((E = 'xxi') ('abc' ? LEN(*N)))    :F(F1)
        S = 'a' E
F1      E =
        X = ~((E = 'xxi') (E ? LEN(*N)))        :F(F2)
        S = S ',b' E
F2      E =
        X = EVAL(""~((E = 'xxi') (E ? LEN(*N)))"")    :S(S3)
        S = S ',c' E                            :(F3)
S3      S = S ',cs' E
F3      E =
        X = ((E = 'xxi') ('abc' ? LEN(*N)))     :S(S4)
        S = S ',d' E                            :(F4)
S4      S = S ',ds' E
F4
END");
        Assert.AreEqual("axxi,bxxi,cxxi,dxxi", ((StringVar)build.Execute!.IdentifierTable["S"]).Data);
    }
}
