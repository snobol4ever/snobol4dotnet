namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_226_001()
    {
        var s = "\tB A(;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(226, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_226_002()
    {
        var s = "\tB A (B,C;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(226, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }
}