using Snobol4.Common;
using Test.TestLexer;

namespace Test.StringComparison;

[TestClass]
public class LGt
{
    [TestMethod]
    public void TEST_LGt_001()
    {
        var s = @"
        r = 'success'        
        a = 'this is a test'
        b = 'this is a test'
        lgt(a,b) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_LGt_002()
    {
        var s = @"
        r = 'success'        
        a = 'this is a test'
        b = 'this is not a test'
        lgt(a,b) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_LGt_003()
    {
        var s = @"
        r = 'success'        
        a = 'this is a test'
        b = 'this is not a test'
        lgt(b,a) :s(end)
        r = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_LGt_004()
    {
        var s = @"
        a = any('this is a test')
        b = 'this is a test'
        lgt(a,b)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(126, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_LGt_005()
    {
        var s = @"
        a = 'this is a test'
        b = any('this is a test')
        lgt(a,b)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(127, build.ErrorCodeHistory[0]);
    }
}