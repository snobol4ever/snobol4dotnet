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
    public void TEST_DirectGoto_in_error_is_taken_again_by_continue()
    {
        // sbl -b answers ERROR 24 / ERROR 332 / ERROR 332 and then ends with 332, fatal, at Test.sno(3), in statement 6, 9 statements
        // executed: :(CONTINUE) resumes at the failure exit of the statement in error, and a direct goto is its own failure exit, so
        // it is made again, and C, which holds no CODE, is error 332, "goto continue with error in failure goto", a fatal-class error
        // the third strike of which ends the run. The console resumed at the next statement and printed 'after' (found by measuring
        // the CODE road of DESIGN-PLAN random-codegen-scope step 11)
        var output = SetupTests.RunWithInput(@"        &ERRLIMIT = 10
        SETEXIT('ERR')
        X = 1                :<Z>
        OUTPUT = 'after'     :(END)
ERR     OUTPUT = 'ERROR ' &ERRTYPE
        SETEXIT('ERR')       :(CONTINUE)
END").Replace("\r\n", "\n");
        StringAssert.StartsWith(output, "ERROR 24\nERROR 332\nERROR 332\n");
        StringAssert.Contains(output, "Test.sno(3) : ERROR 332 -- goto continue with error in failure goto");
        StringAssert.Contains(output, "in line              3\nin statement         6\nstmts executed       9\n");
        Assert.IsFalse(output.Contains("after"));
    }

    [TestMethod]
    public void TEST_DirectGoto_in_error_ends_where_the_handler_ends_the_run()
    {
        // sbl -bf answers ERROR 24 / ERROR 332 / ERROR 332 and ends at the handler's :S(END) before the third strike: a handler run at
        // the goto's error that leaves makes its transfer. The console went on to a third 332, fatal, after the handler's END
        // (snobol4dotnet 2705048; the witness ladder__rung16_dotnet_continue_retakes_a_direct_goto_in_error)
        var output = SetupTests.RunWithInputDirectives("-bf", @"        &ERRLIMIT = 10
        SETEXIT('ERR')
        X = 1                :<Z>
        OUTPUT = 'after'     :(END)
ERR     N = N + 1
        OUTPUT = 'ERROR ' &ERRTYPE
        GE(N, 3)             :S(END)
        SETEXIT('ERR')       :(CONTINUE)
END").Replace("\r\n", "\n");
        Assert.AreEqual("ERROR 24\nERROR 332\nERROR 332", output.TrimEnd('\n'));
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
