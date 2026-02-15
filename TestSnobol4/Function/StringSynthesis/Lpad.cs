using Snobol4.Common;
using Test.TestLexer;

namespace Test.StringSynthesis;

[TestClass]
public class Lpad
{

    [TestMethod]
    public void TEST_Lpad_001()
    {
        var s = @"
        s = 'this is a test'
        r = lpad(s,20,'-+')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("------this is a test", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_Lpad_002()
    {
        var s = @"
        s = 'this is a test'
        r = lpad(s,20)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("      this is a test", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_Lpad_003()
    {
        var s = @"
        s = 'this is a test'
        r = lpad(s,20,'')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("      this is a test", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_Lpad_004()
    {
        var s = @"
        s = any('this is a test')
        r = lpad(s,20,'')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(146, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Lpad_005()
    {
        var s = @"
        s = 'this is a test'
        r = lpad(s,'abc','')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(145, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Lpad_006()
    {
        var s = @"
        s = 'this is a test'
        r = lpad(s,2,any('abc'))
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(144, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Lpad_007()
    {
        var s = @"
        s = 'this is a test'
        r = lpad(s,2)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("this is a test", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_Lpad_008()
    {
        var s = @"
        r = ''
        s = 'this is a test'
        r = lpad(s,-2) :s(end)
        r = 'fail'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }
}