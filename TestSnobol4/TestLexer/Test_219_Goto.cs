namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_219_001()
    {
        var s = $"test1  output = test2 :{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(219, build.ErrorCodeHistory[0]);
        Assert.AreEqual(23, build.ColumnHistory[0]);
    }

}
