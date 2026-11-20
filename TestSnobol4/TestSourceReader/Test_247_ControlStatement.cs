using Test.TestLexer;

namespace Test.TestSourceReader;

[TestClass]
public partial class TestSourceReader
{
    [TestMethod]
    public void TEST_247_001()
    {
        var s = @"
-WTF
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(247, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

}
