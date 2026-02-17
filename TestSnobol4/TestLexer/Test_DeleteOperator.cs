using Snobol4.Common;

namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_001()
    {
        var s = @"
        INLINE2 = 'blank'
        INLINE2 = :(APP)
APP
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }

    [TestMethod]
    public void TEST_002()
    {
        var s = @"
        INLINE2 = 'blank'
        (INLINE2 = )
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }

    [TestMethod]
    public void TEST_003()
    {
        var s = @"
        INLINE2 = 'blank'
        (INLINE2 =)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }

    [TestMethod]
    public void TEST_004()
    {
        var s = @"
        INLINE2 = 'blank'
        INLINE2 =:(end)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }

    [TestMethod]
    public void TEST_005()
    {
        var s = @"
        INLINE2 = 'blank'
        INLINE2 =
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }

    [TestMethod]
    public void TEST_006()
    {
        var s = @"
        INLINE2 = 'blank'
        INLINE2 =;
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }

    [TestMethod]
    public void TEST_007()
    {
        var s = @"
        INLINE2 = 'blank'
        a = (INLINE2 =, 'blank')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }

    [TestMethod]
    public void TEST_008()
    {
        var s = @"
        INLINE2 = 'blank'
        a = (eq(0,1) 'blank',INLINE2 =)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }

    [TestMethod]
    public void TEST_009()
    {
        var s = @"
        INLINE2 = 'blank'
        a = table(10)
        a[INLINE2 =]
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }


    [TestMethod]
    public void TEST_010()
    {
        var s = @"
        INLINE2 = 'blank'
        a = table(10)
        a[INLINE2 = ]
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("INLINE2")]).Data);
    }

}