using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A deferred capture target is evaluated for a name when the match assigns, as in SPITBOL: a call that gives a value (a RETURN, SIZE('a'))
/// is error 21 and an expression that can only give one (*('a' 'b'), *'abc') error 249, counted while &amp;ERRLIMIT is above 0 and failing the
/// statement even at 0; an NRETURN gives the name and assigns. The assignment failed with no error. Found beside CaptureIntoAValueBuilt.
/// Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CaptureIntoAValueDeferred
{
    [TestMethod]
    public void TEST_CaptureIntoAValueDeferred_is_error_21_or_249()
    {
        var program = @"        &ERRLIMIT = 10
        DEFINE('h(n)')                              :(he)
h       h = n + 1                                   :(RETURN)
he      DEFINE('k()')                               :(ke)
k       k = .Q                                      :(NRETURN)
ke      'ab' ? LEN(1) . *h(2)                       :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE ' ' &ERRLIMIT      :(E1)
Y1      OUTPUT = '1 S'
E1      'ab' ? LEN(1) $ *SIZE('a')                  :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE ' ' &ERRLIMIT      :(E2)
Y2      OUTPUT = '2 S'
E2      'ab' ? LEN(1) . *('a' 'b')                  :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE ' ' &ERRLIMIT      :(E3)
Y3      OUTPUT = '3 S'
E3      'ab' ? @*h(2)                               :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE ' ' &ERRLIMIT      :(E4)
Y4      OUTPUT = '4 S'
E4      'ab' ? LEN(1) . *k()                        :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE                    :(E5)
Y5      OUTPUT = '5 S ' Q
E5      &ERRTYPE = 0
        &ERRLIMIT = 0
        'ab' ? LEN(1) . *'abc'                      :S(Y6)
        OUTPUT = '6 F ' &ERRTYPE ' ' &ERRLIMIT      :(END)
Y6      OUTPUT = '6 S'
END";
        Assert.AreEqual(@"1 F 21 9
2 F 21 8
3 F 249 7
4 F 21 6
5 S a
6 F 249 0", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
