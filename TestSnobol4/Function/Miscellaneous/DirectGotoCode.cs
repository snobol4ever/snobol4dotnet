using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A direct goto takes CODE as its operand's value, as SPITBOL does: :<CODE('...')> with no variable
/// behind it transfers like :<C> with C holding the CODE.
/// </summary>
[DoNotParallelize]
[TestClass]
public class DirectGotoCode
{
    [TestMethod]
    public void TEST_DirectGoto_to_a_code_value()
    {
        // sbl -b answers in code / back / LP=lp / direct (Gimpel's LPROG returns through such a goto)
        Assert.AreEqual("in code\nback\nLP=lp\ndirect", SetupTests.RunWithInput(@"        C = CODE(' OUTPUT = ""in code"" :(BACK)')
        :<C>
BACK    OUTPUT = 'back'
        DEFINE('LP()')                 :(LP_END)
LP      :<CODE(' LP = ""lp"" :(RETURN)')>
LP_END  OUTPUT = 'LP=' LP()
        :<CODE(' OUTPUT = ""direct""')>
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_DirectGoto_is_not_made_after_a_trapped_error()
    {
        // sbl -b answers ERROR 148 / ERROR2 148 / ERROR3 148: an error SETEXIT traps takes the statement's place, so its direct goto
        // (:<C>, :F<C>, :S(E3)F<C>) is not made, as a label goto is not. The console made it after the trap, and C, which holds no CODE,
        // was error 24 with no handler left, then 332 (found by measuring the CODE road of DESIGN-PLAN random-codegen-scope step 11)
        Assert.AreEqual("ERROR 148\nERROR2 148\nERROR3 148", SetupTests.RunWithInput(@"        &ERRLIMIT = 10
        SETEXIT('E1')
        X = LT(1, 'a')      :<C>
        OUTPUT = 'next'     :(END)
E1      OUTPUT = 'ERROR ' &ERRTYPE
        SETEXIT('E2')
        X = LT(1, 'b')      :F<C>
        OUTPUT = 'next 2'   :(END)
E2      OUTPUT = 'ERROR2 ' &ERRTYPE
        SETEXIT('E3')
        X = LT(1, 'c')      :S(E3)F<C>
        OUTPUT = 'next 3'
E3      OUTPUT = 'ERROR3 ' &ERRTYPE
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
