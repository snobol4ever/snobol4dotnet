using Snobol4.Common;
using Test.TestLexer;

namespace Test.StringSynthesis;

[TestClass]
public class Reverse
{
    //"reverse argument is not a string" /* 177 */,

    [TestMethod]
    public void TEST_Reverse_001()
    {
        var s = @"
        a = 'this is a test'
        b = reverse(a)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("tset a si siht", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Reverse_002()
    {
        var s = @"
        a = any('this is a test')
        b = reverse(a)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(177, build.ErrorCodeHistory[0]);
    }

}