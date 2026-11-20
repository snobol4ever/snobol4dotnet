namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_222_001()
    {
        var s = "   A[[]];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_002()
    {
        var s = "   [0];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_003()
    {
        var s = "   ([0]);end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_004()
    {
        var s = "   [[0]];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_005()
    {
        var s = "   A[[0]];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_006()
    {
        var s = "   A([0]);end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }


    [TestMethod]
    public void TEST_222_007()
    {
        var s = "   A<<>>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_008()
    {
        var s = "   <0>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_009()
    {
        var s = "   (<0>);end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_010()
    {
        var s = "   <<0>>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_011()
    {
        var s = "   A<<0>>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_222_012()
    {
        var s = "   A(<0>);end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }
}