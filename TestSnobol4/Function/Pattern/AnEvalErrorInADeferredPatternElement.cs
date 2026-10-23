using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// An error inside an EVAL that a deferred pattern element calls fails only that EVAL, as in SPITBOL, where the error unwinds to the
/// frame of the EVAL: the element fails and the match goes on to the next alternative ('abc' ? (*EVAL("(1 / N, 'b')") | 'c') matches
/// c, so does LEN(*EVAL('1 / N')) | 'c' and *SIZE(EVAL('1 / N')) | 'c'), while an error in the element's own deferred expression
/// (*(1 / N)) still ends the whole match. dotnet ended the whole match for both, taking any error raised at the match's level for the
/// node's own. Found by the infinite_snobol4 generator under -b (change 19), reduced to these. Every expected line is what sbl -b
/// printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AnEvalErrorInADeferredPatternElement
{
    [TestMethod]
    public void TEST_AnEvalErrorInADeferredPatternElement_fails_only_that_eval()
    {
        var program = @"        &ERRLIMIT = 100
        N = 0
        'abc' ? (*EVAL(""(1 / N, 'b')"") | 'c') . R       :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                        :(E1)
Y1      OUTPUT = '1 S ' R
E1      'abc' ? (*EVAL('1 / N') | 'c') . R              :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                        :(E2)
Y2      OUTPUT = '2 S ' R
E2      'abc' ? (*(1 / N) | 'c') . R                    :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                        :(E3)
Y3      OUTPUT = '3 S ' R
E3      'abc' ? (LEN(*EVAL('1 / N')) | 'c') . R         :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                        :(E4)
Y4      OUTPUT = '4 S ' R
E4      'abc' ? (*SIZE(EVAL('1 / N')) | 'c') . R        :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE                        :(E5)
Y5      OUTPUT = '5 S ' R
E5
END";
        Assert.AreEqual(@"1 S c
2 S c
3 F 14
4 S c
5 S c", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
