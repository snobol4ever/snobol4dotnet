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

    [TestMethod]
    public void TEST_DeferredFunctionPattern_subscripted_call_is_deferred()
    {
        // sbl -bf: *SORT(T)[1, 2], *F(A)[1], *(A)<2> and *ARRAY(2, 'q')[1] defer the whole element of what the call or the parens
        // give, as *T[C] does, so EVAL reads it later and a match reads SORT(T)[N, 1] with N set after the star; snobol4dotnet
        // deferred the call alone and subscripted the expression, error 235 (found by the infinite_snobol4 generator at scope 9,
        // '7r' ? *SORT(u)[d, 1] | (TAN(0.)))
        var build = SetupTests.SetupScript("-bf", """
                 DEFINE('F(Y)')                  :(FE)
         F       F = Y                           :(RETURN)
         FE      T = TABLE()
                 T['k'] = 'v'
                 T['j'] = 'w'
                 A = ARRAY(2, 'a')
                 R = EVAL(*SORT(T)[1, 2]) ' ' EVAL(*F(A)[1]) ' ' EVAL(*(A)<2>) ' ' EVAL(*ARRAY(2, 'q')[1])
                 E = *SORT(T)[N, 1]
                 N = 2
                 R = R ' ' EVAL(E) ' ' DATATYPE(E)
                 'zkw' LEN(1) *SORT(T)[N, 1] @K
                 R = R ' ' K
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("w a a q k EXPRESSION 2", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }

    [TestMethod]
    public void TEST_DeferredFunctionPattern_an_expression_converts_to_a_pattern()
    {
        // sbl -bf: CONVERT(*X, 'PATTERN') is a PATTERN that evaluates X when it is matched, as *X does in a pattern, so
        // 'aqb' ? CONVERT(*X, 'PATTERN') . R matches q after X = 'q'; the console's EXPRESSION converted only to EXPRESSION,
        // and the conversion failed (found by measuring the CODE road of DESIGN-PLAN random-codegen-scope step 11,
        // CONVERT(CONVERT(a[i], 'EXPRESSION'), 'PATTERN'))
        var build = SetupTests.SetupScript("-bf", """
                 E = CONVERT('i', 'EXPRESSION')
                 R = DATATYPE(E) ' ' DATATYPE(CONVERT(E, 'PATTERN')) ' ' DATATYPE(CONVERT(*X, 'PATTERN'))
                 X = 'q'
                 'aqb' ? CONVERT(*X, 'PATTERN') . M
                 R = R ' ' M
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("EXPRESSION PATTERN PATTERN q", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}
