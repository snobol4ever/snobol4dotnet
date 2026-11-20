using Test.TestLexer;

namespace Test.TestCommandLine;

/// <summary>
/// -n and the -NOEXECUTE control compile a program without running it; -EXECUTE undoes
/// -NOEXECUTE, the last of the two winning. Expected results are SPITBOL's (sbl -b; Budne's noexec).
/// </summary>
[DoNotParallelize]
[TestClass]
public class NoExecute
{

    [TestMethod]
    public void TEST_NoExecute_dash_n_compiles_and_does_not_run()
    {
        var build = SetupTests.SetupScript("-b -n", "        R = 'ran'\nend");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", build.Execute!.IdentifierTable[build.FoldCase("R")].ToString());
    }
}
