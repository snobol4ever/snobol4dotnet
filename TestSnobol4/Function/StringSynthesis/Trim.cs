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
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
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
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
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
        Assert.AreEqual("   abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
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
        Assert.AreEqual("   abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
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
        Assert.AreEqual("1234", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
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
        // TRIM of a pattern is 200 ("trim argument is not a string"), as sbl says
        Assert.AreEqual(200, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Trim_007_all_spaces()
    {
        var s = @"
        b = trim('     ')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_Trim_008_multiple_trailing_spaces()
    {
        // TRIM removes all trailing spaces
        var s = @"
        b = trim('hello      ')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("hello", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_Trim_009_empty_string()
    {
        var s = @"
        b = trim('')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

}
