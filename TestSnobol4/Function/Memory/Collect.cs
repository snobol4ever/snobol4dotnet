using Test.TestLexer;

namespace Test.Memory;

[TestClass]
public class Collect
{
    [TestMethod]
    public void TEST_Collect_001()
    {
        var s = @"

        COLLECT(1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }
}