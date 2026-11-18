using Snobol4.Common;
using Test.TestLexer;

namespace Test.Compilation;

/// <summary>
/// SPITBOL's error section takes an error at execute-time compile (CODE, EVAL, CONVERT) to err04, which pops to the deeper of the
/// innermost function call and the compile's own call; in a function's statement the two are one, the function's is found first and
/// the stage is left at compile (sbl.min err04, errc4). Every later error is then taken there too: with &amp;ERRLIMIT at 0 it fails the
/// statement of the innermost function, printing nothing, until a compile at run time succeeds and puts the stage back. sbl -bf on
/// the program below: R1=f 307 R2=f 307, and after EVAL('1') the error ends the run (snolib LOG.tst: LOG.INC's DEXP hands CODE a
/// LOGB text that does not compile, and CLOG(EULERS_NUMBER) then fails quietly).
/// </summary>
[TestClass]
public class AFailedCompileInAFunction
{
    private const string Functions = @"         DEFINE('F()') :(F_E)
F        EVAL('X +') :(RETURN)
F_E      DEFINE('G(X)') :(G_E)
G        G = LN(X) :S(RETURN)F(FRETURN)
G_E      Z = 0
";

    private static bool Reached(Builder build) => build.Execute!.IdentifierTable.ContainsKey("R") && build.Execute.IdentifierTable["R"] is StringVar { Data: "reached" };

    [TestMethod]
    public void TEST_After_a_failed_compile_in_a_function_an_error_in_a_function_fails()
    {
        var build = SetupTests.SetupScript("-bf", Functions + @"         F()
         G(Z) :S(END)
         R = 'R1=f ' &ERRTYPE
         G(Z) :S(END)
         R = R ' R2=f ' &ERRTYPE
END");
        Assert.AreEqual("R1=f 307 R2=f 307", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }

    [TestMethod]
    public void TEST_A_compile_that_succeeds_puts_the_stage_back()
    {
        var build = SetupTests.SetupScript("-bf", Functions + @"         F()
         EVAL('1')
         G(Z) :S(END)
         R = 'reached'
END");
        Assert.IsFalse(Reached(build));
        CollectionAssert.Contains(build.ErrorCodeHistory, 307);
    }

    [TestMethod]
    public void TEST_A_failed_compile_in_the_main_program_leaves_errors_fatal()
    {
        var build = SetupTests.SetupScript("-bf", Functions + @"         EVAL('X +')
         G(Z) :S(END)
         R = 'reached'
END");
        Assert.IsFalse(Reached(build));
    }
}
