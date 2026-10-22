using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A comma outside every bracket and quote makes a text no expression (223), and that is a compile error like any other, as in SPITBOL:
/// EVAL('1,000') counts &amp;ERRLIMIT down and an armed SETEXIT handler takes it, and CONVERT('1,000', 'EXPRESSION') is the same error. EVAL
/// failed quietly, and CONVERT compiled *(1,000), a selection, into an expression. Found by the infinite_snobol4 generator under -b
/// ((-CONVERT('1,000', 'EXPRESSION')) ? X: ERROR 223 in sbl, 10 here). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class TopLevelCommaInAnExpressionText
{
    [TestMethod]
    public void TEST_TopLevelCommaInAnExpressionText_is_error_223()
    {
        var program = @"        &ERRLIMIT = 10
        V = CONVERT('1,000', 'EXPRESSION')          :S(Y1)
        OUTPUT = '1 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E1)
Y1      OUTPUT = '1 S ' DATATYPE(V)
E1      V = EVAL('1,000')                           :S(Y2)
        OUTPUT = '2 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E2)
Y2      OUTPUT = '2 S ' V
E2      V = EVAL('(1,000)')                         :S(Y3)
        OUTPUT = '3 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E3)
Y3      OUTPUT = '3 S ' V
E3      SETEXIT('TRAP')
        V = EVAL('a,b')                             :S(Y4)
        OUTPUT = '4 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(END)
Y4      OUTPUT = '4 S'                              :(END)
TRAP    OUTPUT = '4 TRAP t=' &ERRTYPE ' lim=' &ERRLIMIT
END";
        Assert.AreEqual(@"1 F t=223 lim=9
2 F t=223 lim=8
3 S 1
4 TRAP t=223 lim=7", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
