using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// An ARBNO repetition that matches null still goes on to what follows the ARBNO, as in SPITBOL, and only another repetition from it is
/// not tried. Expected text: sbl -b.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ArbNoNullRepetition
{
    // A repetition that matches null still goes on to what follows the ARBNO, and only another repetition from it is not tried: so *K()
    // after the ARBNO runs once more for each null repetition, and $ Z after a null repetition holding @Z writes Z again. Here the null
    // repetition failed outright. Found by the infinite_snobol4 generator ('4' ? ARBNO(@Z) $ Z ARBNO(...) FAIL left Z = 1 where sbl leaves
    // the null string). Expected text: sbl -b
    [TestMethod]
    public void TEST_ArbNoNullRepetition_goes_on_to_what_follows()
    {
        var program = @"        DEFINE('G()')                               :(GE)
G       N = N + 1
        G = ''                                      :(RETURN)
GE      DEFINE('K()')                               :(KE)
K       M = M + 1
        K = ''                                      :(RETURN)
KE      N = 0
        M = 0
        '' ? ARBNO(*G()) *K() FAIL
        OUTPUT = '1 G=' N ' K=' M
        N = 0
        M = 0
        '4' ? ARBNO(*G()) *K() FAIL
        OUTPUT = '2 G=' N ' K=' M
        N = 0
        M = 0
        '4' ? POS(0) ARBNO('' | *G()) *K() FAIL
        OUTPUT = '3 G=' N ' K=' M
        N = 0
        M = 0
        'ab' ? POS(0) ARBNO(LEN(1) | *G()) *K() FAIL
        OUTPUT = '4 G=' N ' K=' M
        '4' ? ARBNO(@Z) $ Z FAIL
        OUTPUT = '5 Z=' Z
        'abc' ? POS(0) ARBNO('' | LEN(1)) $ Y RPOS(0)
        OUTPUT = '6 Y=' Y
END";
        Assert.AreEqual("1 G=1 K=2\n2 G=2 K=4\n3 G=1 K=3\n4 G=3 K=6\n5 Z=\n6 Y=abc",
            SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
