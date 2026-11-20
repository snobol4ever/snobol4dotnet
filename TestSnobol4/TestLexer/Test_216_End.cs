namespace Test.TestLexer;

public partial class TestLexer
{

    [TestMethod]
    public void TEST_216_008()
    {
        var s = "eNd";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        }
        else
        {
            Assert.AreEqual(1, build.ErrorCodeHistory.Count);
            Assert.AreEqual(216, build.ErrorCodeHistory[0]);
        }
    }
}