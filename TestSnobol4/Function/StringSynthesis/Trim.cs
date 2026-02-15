using Snobol4.Common;
using Test.TestLexer;

namespace Test.StringSynthesis;

[TestClass]
public class Trim
{
    [TestMethod]
    public void TEST_Trim_001()
    {
        var s = @"
        a = 'abc   '
        b = trim(a)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Trim_002()
    {
        var s = @"
        a = 'abc'
        b = trim(a)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Trim_003()
    {
        var s = @"
        a = '   abc'
        b = trim(a)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("   abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Trim_004()
    {
        var s = @"
        a = '   abc         '
        b = trim(a)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("   abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }



    [TestMethod]
    public void TEST_Trim_005()
    {
        var s = @"
        a = 1234
        b = trim(a)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("1234", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Trim_006()
    {
        var s = @"
        a = 'ab' | 'cd'
        b = trim(a)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(247, build.ErrorCodeHistory[0]);
    }


}