using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// CONVERT of a text that does not compile to EXPRESSION or CODE is the compile error of EVAL or CODE, as in SPITBOL: &amp;ERRLIMIT counts it
/// down and an armed SETEXIT handler takes it (CONVERT('(', 'EXPRESSION') is 226 in the handler), where it failed quietly; and the null
/// string converts to the expression whose value is the null string, where it failed. Found by the infinite_snobol4 generator
/// (... ? ITEM($a[0], 9) (+CONVERT('[]', 'EXPRESSION') $m ? '4p'): ERROR 222 in sbl, FAIL here). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ConvertCompileError
{
    [TestMethod]
    public void TEST_ConvertCompileError_is_the_error_of_eval_and_code()
    {
        var program = @"        &ERRLIMIT = 10
        V = CONVERT('[]', 'EXPRESSION')             :S(Y1)
        OUTPUT = '1 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E1)
Y1      OUTPUT = '1 S ' DATATYPE(V)
E1      V = EVAL('[]')                              :S(Y2)
        OUTPUT = '2 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E2)
Y2      OUTPUT = '2 S ' DATATYPE(V)
E2      V = CONVERT('', 'EXPRESSION')               :S(Y3)
        OUTPUT = '3 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E3)
Y3      OUTPUT = '3 S ' DATATYPE(V) ' [' EVAL(V) ']'
E3      V = CONVERT('1 +', 'EXPRESSION')            :S(Y4)
        OUTPUT = '4 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E4)
Y4      OUTPUT = '4 S ' DATATYPE(V)
E4      SETEXIT('TRAP')
        V = CONVERT('(', 'EXPRESSION')              :S(Y5)
        OUTPUT = '5 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E5)
Y5      OUTPUT = '5 S ' DATATYPE(V)                 :(E5)
TRAP    OUTPUT = '5 TRAP t=' &ERRTYPE ' lim=' &ERRLIMIT :(E5)
E5      V = CODE(' X = [')                          :S(Y6)
        OUTPUT = '6 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E6)
Y6      OUTPUT = '6 S'
E6      V = CONVERT(' X = [', 'CODE')               :S(Y7)
        OUTPUT = '7 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(END)
Y7      OUTPUT = '7 S'
END";
        Assert.AreEqual(@"1 F t=222 lim=9
2 F t=222 lim=8
3 S EXPRESSION []
4 F t=221 lim=7
5 TRAP t=226 lim=6
6 F t=222 lim=5
7 F t=222 lim=4", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
