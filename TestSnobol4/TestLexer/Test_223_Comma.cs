namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_223_001()
    {
        var s = "   C = A , B;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(223, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_223_002()
    {
        var s = "   C =  A, B;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(223, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_223_003()
    {
        var s = "   C = A ,B;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(223, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

}