using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

[TestClass]
public class Char
{
    [TestMethod]
    public void TEST_Char_1()
    {
        var s = @"
        b = char(array(20))
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(281, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Char_2()
    {
        var s = @"
        b = char(-1)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(282, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Char_3()
    {
        var s = @"
        b = char(32768)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(282, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Char_4()
    {
        var s = @"
        b = char(32767)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("翿", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }
}