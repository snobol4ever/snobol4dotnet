namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_232_001()
    {
        var s = "\t\"test;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(232, build.ErrorCodeHistory[0]);
        Assert.AreEqual(1, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_232_002()
    {
        var s = "\t'test;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(232, build.ErrorCodeHistory[0]);
        Assert.AreEqual(1, build.ColumnHistory[0]);
    }
}