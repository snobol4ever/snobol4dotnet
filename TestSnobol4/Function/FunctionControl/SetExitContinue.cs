using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// A SETEXIT handler's :(CONTINUE) resumes at the failure exit of the statement in error, as in SPITBOL:
/// its :F label, else its unconditional goto, else the next statement.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SetExitContinue
{
    private static string Run(string erring) => SetupTests.RunWithInput($@"        SETEXIT(.ERRTRACE)
        &ERRLIMIT = 100
        N1 = 1.0
        N0 = 0.0
        {erring}
        OUTPUT = 'next'
OOPS    OUTPUT = 'oops'                :(END)
ERRTRACE OUTPUT = 'trace ' &ERRTYPE
        SETEXIT(.ERRTRACE)             :(CONTINUE)
END").Replace("\r\n", "\n");

    [TestMethod]
    public void TEST_SetExit_continue_takes_the_failure_exit()
    {
        // sbl -b answers trace 262 then oops for the first, second and fourth, trace 262 / next / oops for
        // the third
        Assert.AreEqual("trace 262\noops", Run("R = N1 / N0 :S(END)F(OOPS)"));
        Assert.AreEqual("trace 262\noops", Run("R = EVAL('N1 / N0') :S(END)F(OOPS)"));
        Assert.AreEqual("trace 262\nnext\noops", Run("R = N1 / N0"));
        Assert.AreEqual("trace 262\noops", Run("R = N1 / N0 :(OOPS)"));
    }

    [TestMethod]
    public void TEST_SetExit_continue_without_an_error_is_error_37()
    {
        // sbl -b: :(CONTINUE) with no trapped error is error 37, which the SETEXIT handler traps
        // (snobol4dotnet's own Test.sno); without a handler it ends the run with 37
        Assert.AreEqual("2\nError trapped", SetupTests.RunWithInput(@"        SETEXIT(.ERR)
        &ERRLIMIT = 100
        A = 2
        OUTPUT = A
        A = 1 / 2                      :(CONTINUE)
        OUTPUT = 'RETURN'              :(END)
ERR     OUTPUT = 'Error trapped'
END").Replace("\r\n", "\n"));

        var bare = SetupTests.SetupScript("-b", @"
        A = 1                          :(CONTINUE)
end");
        Assert.AreEqual(37, bare.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_SetExit_an_error_no_handler_takes_uses_the_continuation_up()
    {
        // sbl -bf: an error no handler takes (the handler did not arm SETEXIT again) goes on at once from where it is, so a later
        // :(CONTINUE) has no error to go on from and is error 37; the console went on from the error the handler had taken (fail)
        var output = SetupTests.RunWithInputDirectives("-bf", @"        &ERRLIMIT = 5
        N = 0
        SETEXIT('ERR')
        Y = ~(1 / N)                        :S(S1)F(F1)
S1      OUTPUT = 'succ [' Y ']'             :(END)
F1      OUTPUT = 'fail'                     :(END)
ERR     OUTPUT = 'ERR ' &ERRTYPE            :(ERR2)
ERR2    Z = 2 / N
        OUTPUT = 'after untrapped ' &ERRTYPE  :(CONTINUE)
END").Replace("\r\n", "\n");
        StringAssert.StartsWith(output, "ERR 14\nafter untrapped 14\n");
        StringAssert.Contains(output, "Test.sno(9) : ERROR 037 -- goto continue with no preceding error");
        Assert.IsFalse(output.Contains("fail"));
    }

    [TestMethod]
    public void TEST_SetExit_firing_disarms_the_trap()
    {
        // sbl -bf: taking a SETEXIT trap resets its stored label to null, so SETEXIT('') in the handler answers
        // the null string (the rungs' keyword_replace_1); snobol4dotnet answered H
        Assert.AreEqual("PREV=[]", SetupTests.RunWithInputDirectives("-bf", @"        SETEXIT(.H)
        &ERRLIMIT = 10
        D = 0
        X = 1 / D
        :(END)
H       OUTPUT = 'PREV=[' SETEXIT('') ']'
        :(END)
END"));
    }

    // sbl -b: a failure exit that is an expression is evaluated when :(CONTINUE) resumes there: $'RETURN' returns F with its null value
    // and $L goes to the label L names (was F [normal] G [normal]: the console resumed at the next statement)
    [TestMethod]
    public void TEST_SetExitContinue_a_failure_exit_that_is_an_expression_is_evaluated()
    {
        Assert.AreEqual("handler t=14\nhandler t=14\nF [] G [there]",
            SetupTests.RunWithInput(@"        &ERRLIMIT = 5
        DEFINE('F()')
        DEFINE('G()')                              :(FE)
F       SETEXIT(.H)
        X = Z / 0                                  :F($'RETURN')
        F = 'normal'                               :(RETURN)
G       SETEXIT(.H)
        L = 'THERE'
        X = Z / 0                                  :F($L)
        G = 'normal'                               :(RETURN)
THERE   G = 'there'                                :(RETURN)
H       OUTPUT = 'handler t=' &ERRTYPE             :(CONTINUE)
FE      Z = 1
        OUTPUT = 'F [' F() '] G [' G() ']'
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_SetExit_continue_resumes_at_the_innermost_failure_point()
    {
        // sbl -b: the handler runs where the error is, and :(CONTINUE) resumes there by failing the operation in error, so a negation
        // (1) or a choice (2) inside the statement takes that failure and the statement goes on, and a handler of two statements (7)
        // does the same; an interrogation (3), a pattern match the error ended (4) and a plain statement (5) fail, and a handler that
        // leaves elsewhere (6) is entered with the statement's assignment not made. The console ran the handler when the statement
        // ended and took the statement's failure exit (1, 2 and 7 failed; found by measuring the CODE road of DESIGN-PLAN step 11)
        Assert.AreEqual("ERROR 239\n1 succ []\nERROR 239\n2 succ [b]\nERROR 239\n3 fail\nERROR 239\n4 fail\nERROR 239\n5 fail\n" +
                        "ERR2 239 Y=[y]\nERR3 239\nERR3 two\n7 succ []", SetupTests.RunWithInput(@"        &ERRLIMIT = 20
        DEFINE('H()')                       :(HE)
H       H = .E
        DIFFER(H)                           :S(NRETURN)F(FRETURN)
HE      SETEXIT('ERR')
        Y = ~$H()                           :S(S1)F(F1)
S1      OUTPUT = '1 succ [' Y ']'           :(N2)
F1      OUTPUT = '1 fail'                   :(N2)
ERR     OUTPUT = 'ERROR ' &ERRTYPE
        SETEXIT('ERR')                      :(CONTINUE)
N2      Y = ($H(), 'b')                     :S(S2)F(F2)
S2      OUTPUT = '2 succ [' Y ']'           :(N3)
F2      OUTPUT = '2 fail'
N3      Y = ?$H()                           :S(S3)F(F3)
S3      OUTPUT = '3 succ [' Y ']'           :(N4)
F3      OUTPUT = '3 fail'
N4      'abc' ? (*$H() | 'b') . Z           :S(S4)F(F4)
S4      OUTPUT = '4 succ [' Z ']'           :(N5)
F4      OUTPUT = '4 fail'
N5      Y = $H() 'x'                        :S(S5)F(F5)
S5      OUTPUT = '5 succ [' Y ']'           :(N6)
F5      OUTPUT = '5 fail'
N6      SETEXIT('ERR2')
        Y = 'y'
        Y = ~$H()                           :S(S6)F(F6)
S6      OUTPUT = '6 succ [' Y ']'           :(N7)
F6      OUTPUT = '6 fail'                   :(N7)
ERR2    OUTPUT = 'ERR2 ' &ERRTYPE ' Y=[' Y ']'  :(N7)
N7      SETEXIT('ERR3')
        Y = ~$H()                           :S(S7)F(F7)
S7      OUTPUT = '7 succ [' Y ']'           :(END)
F7      OUTPUT = '7 fail'                   :(END)
ERR3    OUTPUT = 'ERR3 ' &ERRTYPE
        OUTPUT = 'ERR3 two'
        SETEXIT('ERR3')                     :(CONTINUE)
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_SetExit_continue_after_an_error_in_evaluated_code_fails_the_evaluation()
    {
        // sbl -b: an error in the code of an evaluated expression ends that evaluation, which fails as a whole, and a handler's
        // :(CONTINUE) resumes at that failure, so a negation (1), a choice (2) or a negated expression (3) inside it does not take
        // the error's failure. The handler run at the error let them take it (snobol4dotnet 2705048: succ [], succ2 [c], succ3 [])
        Assert.AreEqual("ERROR 239\nfail\nERROR 239\nfail2\nERROR 239\nfail3", SetupTests.RunWithInput(@"        &ERRLIMIT = 10
        DEFINE('H()')                       :(HE)
H       H = .E
        DIFFER(H)                           :S(NRETURN)F(FRETURN)
HE      SETEXIT('ERR')
        Y = EVAL('~$H()')                   :S(S1)F(F1)
S1      OUTPUT = 'succ [' Y ']'             :(N2)
F1      OUTPUT = 'fail'                     :(N2)
ERR     OUTPUT = 'ERROR ' &ERRTYPE
        SETEXIT('ERR')                      :(CONTINUE)
N2      Y = EVAL(""(DIFFER($H()), 'c')"")     :S(S2)F(F2)
S2      OUTPUT = 'succ2 [' Y ']'            :(N3)
F2      OUTPUT = 'fail2'
N3      E = *(~$H())
        Y = EVAL(E)                         :S(S3)F(F3)
S3      OUTPUT = 'succ3 [' Y ']'            :(END)
F3      OUTPUT = 'fail3'
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
