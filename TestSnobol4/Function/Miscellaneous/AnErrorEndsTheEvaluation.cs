using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// An error inside an evaluated expression (EVAL of a string or of an expression) ends that evaluation, which fails as a whole, as in
/// SPITBOL: a negation or a choice inside the expression does not go on from it (EVAL('~(1 / N)') and EVAL("(1 / N, 'd')") fail with
/// &amp;ERRTYPE 14), at &amp;ERRLIMIT 0 too. The failed EVAL is an ordinary failure to the code that called it ((EVAL('1 / N'), 'f') is
/// f, nested EVALs too), and a statement of a function the expression calls goes on from an error as any statement does (NEG). In a
/// statement an error with no handler armed is the failure of its operand, so ~(1 / N) succeeds and (1 / N, 'b') is b. dotnet went on
/// inside the expression as in a statement. Found by the infinite_snobol4 generator, whose evaluator EVALs every test. Every expected
/// line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AnErrorEndsTheEvaluation
{
    [TestMethod]
    public void TEST_AnErrorEndsTheEvaluation_negation_and_choice_do_not_go_on_from_it()
    {
        var program = @"        &ERRLIMIT = 20
        N = 0
        DEFINE('NEG()')                             :(NEG_END)
NEG     NEG = ~(1 / N)                              :S(RETURN)F(FRETURN)
NEG_END
        V = ~(1 / N)                                :S(Y1)
        OUTPUT = '1 F'                              :(E1)
Y1      OUTPUT = '1 S [' V ']'
E1      V = (1 / N, 'b')                            :S(Y2)
        OUTPUT = '2 F'                              :(E2)
Y2      OUTPUT = '2 S [' V ']'
E2      V = EVAL('~(1 / N)')                        :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                    :(E3)
Y3      OUTPUT = '3 S [' V ']'
E3      V = EVAL(""(1 / N, 'd')"")                    :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                    :(E4)
Y4      OUTPUT = '4 S [' V ']'
E4      V = EVAL(*((1 / N), 'e'))                   :S(Y5)
        OUTPUT = '5 F'                              :(E5)
Y5      OUTPUT = '5 S [' V ']'
E5      V = (EVAL('1 / N'), 'f')                    :S(Y6)
        OUTPUT = '6 F'                              :(E6)
Y6      OUTPUT = '6 S [' V ']'
E6      V = EVAL(""(EVAL('1 / N'), 'g')"")            :S(Y7)
        OUTPUT = '7 F'                              :(E7)
Y7      OUTPUT = '7 S [' V ']'
E7      V = EVAL('NEG()')                           :S(Y8)
        OUTPUT = '8 F'                              :(E8)
Y8      OUTPUT = '8 S [' V ']'
E8      &ERRLIMIT = 0
        V = EVAL('~(1 / N)')                        :S(Y9)
        OUTPUT = '9 F &ERRLIMIT=' &ERRLIMIT         :(E9)
Y9      OUTPUT = '9 S [' V ']'
E9
END";
        Assert.AreEqual(@"1 S []
2 S [b]
3 F 14
4 F 14
5 F
6 S [f]
7 S [g]
8 S []
9 F &ERRLIMIT=0", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
