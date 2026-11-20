namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_224_001()
    {
        var s = "\tB A);end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(224, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_224_002()
    {
        var s = "\tA[B + C)];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(224, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_224_003()
    {
        var s = "\t(A + B[);end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(224, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }
}