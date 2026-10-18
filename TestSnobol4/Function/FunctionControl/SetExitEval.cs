using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// An error inside EVAL is trapped by SETEXIT like any other, and the trap wins over the statement's
/// failure goto, as SPITBOL does (sbl -b; spitbol_x64_tests' math_chks.inc).
/// </summary>
[DoNotParallelize]
[TestClass]
public class SetExitEval
{
    [TestMethod]
    public void TEST_SetExitEval_an_error_inside_eval_is_trapped()
    {
        Assert.AreEqual("trap 1\ntrap 2", SetupTests.RunWithInput(@"        &ERRLIMIT = 100
        M = -4
        SETEXIT(.T1)
        O = SQRT(M)  :F(OOPS1)
        OUTPUT = 'no trap 1'  :(N2)
OOPS1   OUTPUT = 'oops 1'  :(N2)
T1      OUTPUT = 'trap 1'
N2      SETEXIT(.T2)
        O = EVAL('SQRT(M)')  :F(OOPS2)
        OUTPUT = 'no trap 2'  :(END)
OOPS2   OUTPUT = 'oops 2'  :(END)
T2      OUTPUT = 'trap 2'
END").Replace("\r\n", "\n"));
    }
}
