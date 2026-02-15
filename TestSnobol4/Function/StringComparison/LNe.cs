using Snobol4.Common;
using Test.TestLexer;

namespace Test.StringComparison;

[TestClass]
public class LNe
{
    [TestMethod]
    public void TEST_LNe_001()
    {
        var s = @"
        r = 'success'        
        a = 'this is a test'
        b = 'this is a test'
        lne(a,b) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_LNe_002()
    {
        var s = @"
        r = 'success'        
        a = 'this is a test'
        b = 'this is not a test'
        lne(a,b) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_LNe_003()
    {
        var s = @"
        r = 'success'        
        a = 'this is a test'
        b = 'this is not a test'
        lne(b,a) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_LNe_004()
    {
        var s = @"
        r = 'success'        
        a = any('this is a test')
        b = 'this is not a test'
        lne(a,b) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(132, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_LNe_005()
    {
        var s = @"
        a = 'this is a test'
        b = any('this is a test')
        lne(a,b)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(133, build.ErrorCodeHistory[0]);
    }
}