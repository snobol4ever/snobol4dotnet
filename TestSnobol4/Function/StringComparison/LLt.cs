using Snobol4.Common;
using Test.TestLexer;

namespace Test.StringComparison;

[TestClass]
public class LLt
{
    [TestMethod]
    public void TEST_LLt_001()
    {
        var s = @"
        r = 'success'        
        a = 'this is a test'
        b = 'this is a test'
        llt(a,b) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_LLt_002()
    {
        var s = @"
        r = 'success'        
        a = 'this is a test'
        b = 'this is not a test'
        llt(a,b) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_LLt_003()
    {
        var s = @"
        r = 'success'        
        a = 'this is a test'
        b = 'this is not a test'
        llt(b,a) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_LLt_004()
    {
        var s = @"
        a = any('this is a test')
        b = 'this is a test'
        llt(a,b)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(130, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_LLt_005()
    {
        var s = @"
        a = 'this is a test'
        b = any('this is a test')
        llt(a,b)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(131, build.ErrorCodeHistory[0]);
    }
}