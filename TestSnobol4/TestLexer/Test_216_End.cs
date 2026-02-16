namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_216_001()
    {
        var s = "end";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_216_002()
    {
        var s = "end";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_216_003()
    {
        var s = "end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_216_004()
    {
        var s = "end";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_216_005()
    {
        var s = "end";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_216_006()
    {
        var s = "END";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_216_007()
    {
        var s = "END";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_216_008()
    {
        var s = "eNd";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }
}