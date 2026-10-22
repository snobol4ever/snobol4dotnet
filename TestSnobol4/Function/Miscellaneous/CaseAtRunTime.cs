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

    // A call in a program's text was resolved when that text was compiled, and a later &amp;CASE does not change it: h(1) still calls h
    // once &amp;CASE is 1, and so does h(3) in an EVAL whose own text sets &amp;CASE first, where APPLY('h', 1) looks for H (22). Setting &amp;CASE
    // had refolded the name of every call the run looked up by name, so EVAL("(&amp;CASE = 1) h(3)") was 22 (the infinite_snobol4
    // generator, BREAKX((&amp;CASE = 9)) . h(h))
    [TestMethod]
    public void TEST_CaseAtRunTime_leaves_the_compiled_calls_alone()
    {
        var program = @"        &ERRLIMIT = 10
        DEFINE('h(n)')                              :(he)
h       h = n + 1                                   :(RETURN)
he      &CASE = 1
        OUTPUT = '1 ' h(1)
        V = APPLY('h', 1)                           :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                    :(E2)
Y2      OUTPUT = '2 S ' V
E2      &CASE = 0
        V = EVAL(""(&CASE = 1) h(3)"")                :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                    :(E3)
Y3      OUTPUT = '3 S ' V
E3      &CASE = 0
        C = CODE("" &CASE = 1; OUTPUT = '4 ' h(4)"") :S<C>
END";
        Assert.AreEqual(@"1 2
2 F 22
3 S 14
4 5", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
