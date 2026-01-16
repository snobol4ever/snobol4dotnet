using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

[TestClass]
public class Eject
{
    [TestMethod]
    public void TEST_Eject_001f()
    {
        var s = @"

        EJECT(1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }
}