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

    // sbl -b: with &ERRLIMIT above 0, EVAL or CODE text that does not compile is an error like any other: &ERRLIMIT counts it down
    // and the armed handler runs before the call fails (was eval failed lim=5, code failed lim=5, no handler)
    [TestMethod]
    public void TEST_SetExitEval_a_compile_error_in_eval_or_code_text_is_counted_and_trapped()
    {
        Assert.AreEqual("handler 1 t=221 lim=4\neval failed lim=4 t=221\nhandler 2 t=226 lim=3\ncode failed lim=3 t=226",
            SetupTests.RunWithInput(@"        &ERRLIMIT = 5
        SETEXIT(.H1)
        R = EVAL('1 +')                 :S(B)
        OUTPUT = 'eval failed lim=' &ERRLIMIT ' t=' &ERRTYPE   :(B)
H1      OUTPUT = 'handler 1 t=' &ERRTYPE ' lim=' &ERRLIMIT     :(CONTINUE)
B       SETEXIT(.H2)
        C = CODE(' X = (')              :S(END)
        OUTPUT = 'code failed lim=' &ERRLIMIT ' t=' &ERRTYPE   :(END)
H2      OUTPUT = 'handler 2 t=' &ERRTYPE ' lim=' &ERRLIMIT     :(CONTINUE)
END").Replace("\r\n", "\n"));
    }

    // sbl -b: with &ERRLIMIT at 0 a SETEXIT handler intercepts nothing; an error inside EVAL or a deferred pattern fails its statement
    // and the handler is not entered (was handler t=14 lim=0 before each failure)
    [TestMethod]
    public void TEST_SetExitEval_no_handler_runs_while_errlimit_is_zero()
    {
        Assert.AreEqual("1 failed lim=0 t=14\n2 failed t=14", SetupTests.RunWithInput(@"        SETEXIT(.H)
        X = 1
        R = EVAL('X / 0')                         :S(A1)
        OUTPUT = '1 failed lim=' &ERRLIMIT ' t=' &ERRTYPE
A1      SETEXIT(.H)
        'abc' ? *(X / 0)                          :S(END)
        OUTPUT = '2 failed t=' &ERRTYPE           :(END)
H       OUTPUT = 'handler t=' &ERRTYPE ' lim=' &ERRLIMIT   :(CONTINUE)
END").Replace("\r\n", "\n"));
    }
}
