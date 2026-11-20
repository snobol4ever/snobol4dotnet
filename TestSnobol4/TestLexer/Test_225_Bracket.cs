namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_225_001()
    {
        var s = "   A>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(225, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_225_002()
    {
        var s = "   A];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(225, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_225_003()
    {
        var s = " B A>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(225, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_225_004()
    {
        var s = " B A];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(225, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_225_005()
    {
        var s = " (A + B]);end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(225, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }


    [TestMethod]
    public void TEST_225_006()
    {
        var s = "   A[B + C(];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(225, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_225_007()
    {
        var s = "    :S(end(>F<end>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(225, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_225_008()
    {
        var s = "   A<0[>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(225, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }
}