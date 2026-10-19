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
}
