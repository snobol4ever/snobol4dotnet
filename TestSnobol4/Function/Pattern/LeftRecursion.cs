using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A left-recursive pattern (P = *P 'Z' | 'Y') never moves the cursor; SPITBOL overflows its stack and
/// ends the run with error 246, whatever &amp;ERRLIMIT allows. snobol4dotnet looped forever.
/// </summary>
[TestClass]
public class LeftRecursion
{
    [TestMethod]
    public void TEST_LeftRecursion_is_a_fatal_stack_overflow()
    {
        // sbl -b: lr.sno(3) : ERROR 246 -- stack overflow, and nothing after it runs
        var build = SetupTests.SetupScript("-b", @"
        &ERRLIMIT = 1
        P = *P 'Z' | 'Y'
        'Y' P                          :S(Y)F(N)
Y       R = 'yes'                      :(END)
N       R = 'no'
end");
        Assert.AreEqual(1, build.ErrorCodeHistory.Count);
        Assert.AreEqual(246, build.ErrorCodeHistory[0]);
        Assert.AreEqual("", build.Execute!.IdentifierTable["R"].ToString());
    }
}
