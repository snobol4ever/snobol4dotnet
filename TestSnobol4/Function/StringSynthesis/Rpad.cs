using Snobol4.Common;
using Test.TestLexer;

namespace Test.StringSynthesis;

[TestClass]
public class Rpad
{

    [TestMethod]
    public void TEST_Rpad_001()
    {
        var s = @"
        s = 'this is a test'
        r = rpad(s,20,'-+')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("this is a test------", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_Rpad_002()
    {
        var s = @"
        s = 'this is a test'
        r = rpad(s,20)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("this is a test      ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_Rpad_003()
    {
        var s = @"
        s = 'this is a test'
        r = rpad(s,20,'')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("this is a test      ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_Rpad_004()
    {
        var s = @"
        s = any('this is a test')
        r = rpad(s,20,'')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(180, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Rpad_005()
    {
        var s = @"
        s = 'this is a test'
        r = rpad(s,'abc','')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(179, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Rpad_006()
    {
        var s = @"
        s = 'this is a test'
        r = rpad(s,20,any('abc'))
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(178, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Rpad_007()
    {
        var s = @"
        s = 'this is a test'
        r = rpad(s,2)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("this is a test", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }
    [TestMethod]
    public void TEST_Rpad_008()
    {
        var s = @"
        r = ''
        s = 'this is a test'
        r = rpad(s,-2) :s(end)
        r = 'fail'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

}