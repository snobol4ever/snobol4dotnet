namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_215_001()
    {
        var s = "end   'test'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(215, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_215_002()
    {
        var s = "end   'test\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(215, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_215_003()
    {
        var s = "end    ]";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(215, build.ErrorCodeHistory[0]);
    }

}