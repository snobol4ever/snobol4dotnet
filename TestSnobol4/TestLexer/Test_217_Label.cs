namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_217_001()
    {
        var s = $"test a = b{Environment.NewLine}test a = b{Environment.NewLine}end";
        var directives = $"-b ";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(217, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_217_003()
    {
        var s = $"test a = b{Environment.NewLine}test a = b{Environment.NewLine}end";
        var directives = $"-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(217, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_217_005()
    {
        var s = $"test a = b{Environment.NewLine}test a = b{Environment.NewLine}end";
        var directives = $"-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(217, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }
}