using Test.TestLexer;

namespace Test.Miscellaneous;

[TestClass]
public class Time
{
    [TestMethod]
    public void TEST_Time_001()
    {
        var s = @"
        OUTPUT = TIME()
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }
}