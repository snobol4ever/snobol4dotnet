using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A match whose pattern is a deferred expression calling a program-defined function runs that function at
/// match time, as in SPITBOL (sbl -bf; Gimpel's ONCE builds its pattern so).
/// </summary>
[TestClass]
public class DeferredFunctionPattern
{
    [TestMethod]
    public void TEST_DeferredFunctionPattern_once()
    {
        // sbl -bf: S P with P = *ONCE() matches the first time and fails after, since ONCE then answers FAIL;
        // snobol4dotnet crashed (the function's calls cleared the match's argument list)
        var build = SetupTests.SetupScript("-bf", """
                 DEFINE('ONCE()')               :(ONCE_END)
         ONCE    ONCE = DIFFER(DONE) FAIL       :S(RETURN)
                 DONE = 1
                 ONCE = ''                      :(RETURN)
         ONCE_END
                 P = *ONCE()
                 'xyz' P                        :F(T2)
                 R = R 'a'
         T2      'xyz' P                        :F(T3)
                 R = R 'b'
         T3      'xyz' P                        :F(T4)
                 R = R 'c'
         T4
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("a", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }

    [TestMethod]
    public void TEST_DeferredFunctionPattern_subscripted_element_is_deferred()
    {
        // sbl -bf: *T[C], *A<1,2> and *T['a']['b'] defer the whole element, so EVAL reads it later and a match
        // reads T[C] after $ C has set C ('abycz' LEN(1) $ C *T[C] matches at 'b', with T['b'] = 'y'; Gimpel's
        // hyphenat matches *DIGRAM_TBL[C] so); snobol4dotnet dropped the star and read the element at once
        var build = SetupTests.SetupScript("-bf", """
                 T = TABLE()
                 T['a'] = 'x'
                 T['b'] = 'y'
                 T['n'] = TABLE()
                 T['n']['m'] = 'deep'
                 A = ARRAY('2,2')
                 A<1,2> = 'el'
                 E = *T['a']
                 R = EVAL(E) ' ' EVAL(*A<1,2>) ' ' EVAL(*T['n']['m'])
                 'abycz' LEN(1) $ C *T[C] @K
                 R = R ' ' C K
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("x el deep b3", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}
