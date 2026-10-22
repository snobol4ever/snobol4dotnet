using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// &amp;CASE set at run time decides how the names the run reads from strings fold, as in SPITBOL: under -f, once &amp;CASE is 1, APPLY('f', 3)
/// calls F (undefined, 22), $'abc' reads ABC, and DEFINE('g()') defines G, so APPLY('g') is 286; with &amp;CASE back at 0, APPLY('f', 3) calls f
/// again. Only EVAL and CODE followed &amp;CASE; the rest followed the command line. Found by the infinite_snobol4 generator with keyword
/// assignments in its closed world ((&amp;CASE = 1) then APPLY('f', 3): FAIL in sbl, 3 here). Every expected line is what sbl -bf printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CaseAtRunTime
{
    [TestMethod]
    public void TEST_CaseAtRunTime_folds_the_names_read_from_strings()
    {
        var program = @"        &ERRLIMIT = 20
        DEFINE('f(n)')                              :(fe)
f       f = n * 10                                  :(RETURN)
fe      abc = 'lower'
        ABC = 'upper'
        &CASE = 1
        &ERRTYPE = 0
        V = APPLY('f', 3)                           :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                    :(E1)
Y1      OUTPUT = '1 S ' V
E1      &ERRTYPE = 0
        V = $'abc'
        OUTPUT = '2 ' V ' ' &ERRTYPE
        &ERRTYPE = 0
        V = EVAL('abc')
        OUTPUT = '3 ' V ' ' &ERRTYPE
        &ERRTYPE = 0
        V = EVAL(""f(4)"")                            :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                    :(E4)
Y4      OUTPUT = '4 S ' V
E4      &ERRTYPE = 0
        V = APPLY('F', 3)                           :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE                    :(E5)
Y5      OUTPUT = '5 S ' V
E5      &ERRTYPE = 0
        DEFINE('g()')                               :(ge)
g       g = 'gee'                                   :(RETURN)
ge      V = APPLY('g')                              :S(Y6)
        OUTPUT = '6 F ' &ERRTYPE                    :(E6)
Y6      OUTPUT = '6 S ' V
E6      &CASE = 0
        &ERRTYPE = 0
        V = APPLY('f', 3)                           :S(Y7)
        OUTPUT = '7 F ' &ERRTYPE                    :(E7)
Y7      OUTPUT = '7 S ' V
E7
END";
        Assert.AreEqual(@"1 F 22
2 upper 0
3 upper 0
4 F 22
5 F 22
6 F 286
7 S 30", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
