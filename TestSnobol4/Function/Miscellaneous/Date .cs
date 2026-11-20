using Test.TestLexer;

namespace Test.Miscellaneous;

[TestClass]
public class Date
{
    [TestMethod]
    public void TEST_Date_001()
    {
        var s = @"
        OUTPUT = DATE()
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }
}