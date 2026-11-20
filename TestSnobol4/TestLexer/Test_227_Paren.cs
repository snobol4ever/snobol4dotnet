namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_227_001()
    {
        var s = "    :S(end;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(227, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_227_002()
    {
        var s = "    :S(end)F(end;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(227, build.ErrorCodeHistory[0]);
        Assert.AreEqual(16, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_227_003()
    {
        var s = "    :S(end)F(();end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(227, build.ErrorCodeHistory[0]);
        Assert.AreEqual(15, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_227_004()
    {
        var s = "    :S(end;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(227, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_227_005()
    {
        var s = "    :S<end>F(end;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(227, build.ErrorCodeHistory[0]);
        Assert.AreEqual(16, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_227_006()
    {
        var s = "    :S<end>F(();end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(227, build.ErrorCodeHistory[0]);
        Assert.AreEqual(15, build.ColumnHistory[0]);
    }

}