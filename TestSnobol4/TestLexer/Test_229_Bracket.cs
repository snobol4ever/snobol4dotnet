namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_229_001()
    {
        var s = "   A<;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(229, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_229_002()
    {
        var s = "   A[;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(229, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_229_003()
    {
        var s = "   A<0<>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(229, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_229_004()
    {
        var s = " B A<;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(229, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_229_005()
    {
        var s = " B A[;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(229, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_229_006()
    {
        var s = "  (B A)<;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(229, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_229_007()
    {
        var s = "  (B A)[;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(229, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_009()
    {
        var s = "   A<0(>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(6, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_229_010()
    {
        var s = "   A[0<];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(222, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

}