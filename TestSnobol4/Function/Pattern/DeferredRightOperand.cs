using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A match whose whole pattern is a deferred expression (S ? *F(), S *F(), or P = *F() then S ? P) is a pattern of one element, *F(),
/// which the match evaluates each time it reaches it, at every cursor of an unanchored match, as in SPITBOL. It was evaluated once,
/// before the match, and before the subject was checked: a subject that is no string read the expression's error (BREAK('a') ? *$Y with
/// Y null, 239) where sbl reports 241, and a value that is no pattern read 240 where sbl reports 46. Found by the infinite_snobol4
/// generator (BREAK('a') ? (*$Y)). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class DeferredRightOperand
{
    [TestMethod]
    public void TEST_DeferredRightOperand_is_evaluated_at_every_cursor()
    {
        var program = @"        DEFINE('F()')                               :(FE)
F       N = N + 1
        F = 'z'                                     :(RETURN)
FE      DEFINE('G()')                               :(GE)
G       M = M + 1                                   :(FRETURN)
GE      N = 0
        'abc' ? *F()                                :S(Y1)
        OUTPUT = '1 F calls=' N                     :(E1)
Y1      OUTPUT = '1 S calls=' N
E1      M = 0
        'abc' ? *G()                                :S(Y2)
        OUTPUT = '2 F calls=' M                     :(E2)
Y2      OUTPUT = '2 S calls=' M
E2      N = 0
        &ANCHOR = 1
        'abc' ? *F()                                :S(Y3)
        OUTPUT = '3 F calls=' N                     :(E3)
Y3      OUTPUT = '3 S calls=' N
E3      &ANCHOR = 0
        N = 0
        'abz' ? *F()                                :S(Y4)
        OUTPUT = '4 F calls=' N                     :(E4)
Y4      OUTPUT = '4 S calls=' N
E4      N = 0
        'abc' *F()                                  :S(Y5)
        OUTPUT = '5 F calls=' N                     :(E5)
Y5      OUTPUT = '5 S calls=' N
E5      N = 0
        P = *F()
        'abc' ? P                                   :S(Y6)
        OUTPUT = '6 F calls=' N                     :(E6)
Y6      OUTPUT = '6 S calls=' N
E6
END";
        Assert.AreEqual(@"1 F calls=4
2 F calls=4
3 F calls=1
4 S calls=3
5 F calls=4
6 F calls=4", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_DeferredRightOperand_the_subject_is_checked_first_and_a_value_that_is_no_pattern_is_46()
    {
        var program = @"        &ERRLIMIT = 20
        Y =
        &ERRTYPE = 0
        V = BREAK('a') ? *$Y
        OUTPUT = '1 ' &ERRTYPE
        &ERRTYPE = 0
        V = ARRAY(2) ? *F()
        OUTPUT = '2 ' &ERRTYPE
        &ERRTYPE = 0
        V = 'abc' ? *TABLE()
        OUTPUT = '3 ' &ERRTYPE
        &ERRTYPE = 0
        'abc' *ARRAY(1)
        OUTPUT = '4 ' &ERRTYPE
        &ERRTYPE = 0
        V = 'abc' ? TABLE()
        OUTPUT = '5 ' &ERRTYPE
END";
        Assert.AreEqual(@"1 241
2 241
3 46
4 46
5 240", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
