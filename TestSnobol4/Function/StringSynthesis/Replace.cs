using Snobol4.Common;
using Test.TestLexer;

namespace Test.StringSynthesis;

//"replace third argument is not a string" /* 168 */,
//"replace second argument is not a string" /* 169 */,
//"replace first argument is not a string" /* 170 */,
//"null or unequally long 2nd 3rd args to replace" /* 171 */,

[TestClass]
public class Replace
{
    [TestMethod]
    public void TEST_Replace_001()
    {
        var s = @"
        a = 'abc'
        from = 'cb'
        to = 'CB'
        b = replace(a,from, to)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("aBC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Replace_002()
    {
        var s = @"
        a = ''
        from = 'cb'
        to = 'CB'
        b = replace(a,from, to)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Replace_170()
    {
        var s = @"
        a = 'abc' | 'def'
        from = 'cb'
        to = 'CB'
        b = replace(a,from, to)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(170, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Replace_169()
    {
        var s = @"
        a = 'abc'
        from = 'cb' | 'CB'
        to = 'CB'
        b = replace(a,from, to)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(169, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Replace_168()
    {
        var s = @"
        a = 'abc'
        from = 'cb'
        to = 'CB' | 'cb'
        b = replace(a,from, to)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(168, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Replace_171a()
    {
        var s = @"
        a = 'abc'
        from = ''
        to = 'CB'
        b = replace(a,from, to)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(171, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Replace_171b()
    {
        var s = @"
        a = 'abc'
        from = 'cb'
        to = ''
        b = replace(a,from, to)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(171, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Replace_171c()
    {
        var s = @"
        a = 'abc'
        from = 'c'
        to = 'CB'
        b = replace(a,from, to)
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(171, build.ErrorCodeHistory[0]);
    }
}