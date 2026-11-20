namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_228_001()
    {
        var s = "    :S<end;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(228, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_228_002()
    {
        var s = "    :S(end)F<end;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(228, build.ErrorCodeHistory[0]);
        Assert.AreEqual(16, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_228_004()
    {
        var s = "    :S<end;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(228, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_228_005()
    {
        var s = "    :S<end>F<end;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(228, build.ErrorCodeHistory[0]);
        Assert.AreEqual(16, build.ColumnHistory[0]);
    }
}