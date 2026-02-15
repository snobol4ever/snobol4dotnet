using Snobol4.Common;
using Test.TestLexer;

namespace Test.StringSynthesis;

[TestClass]
public class Substring
{
    //"substr third argument is not integer" /* 192 */,
    //"substr second argument is not integer" /* 193 */,
    //"substr first argument is not a string" /* 194 */,

    [TestMethod]
    public void TEST_Substr_001()
    {
        var s = @"
        a = 'this is a test'
        b = substr(a,6,2)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("is", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }


    [TestMethod]
    public void TEST_Substr_002()
    {
        var s = @"
        a = 'this is a test'
        b = substr(a,6)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("is a test", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Substr_003()
    {
        var s = @"
        a = 'this is a test'
        b = substr(any(a),6,2)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(194, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Substr_004()
    {
        var s = @"
        a = 'this is a test'
        b = substr(a,'a',2)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(193, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Substr_005()
    {
        var s = @"
        a = 'this is a test'
        b = substr(a,6,'e')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(192, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Substr_006()
    {
        var s = @"
        r = ''
        a = 'this is a test'
        b = substr(a,0) :s(end)
        r = 'fail'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }


    [TestMethod]
    public void TEST_Substr_007()
    {
        var s = @"
        r = ''        
        a = 'this is a test'
        b = substr(a,20) :s(end)
        r = 'fail'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_Substr_008()
    {
        var s = @"
        r = ''
        a = 'this is a test'
        b = substr(a,10,10) :s(end)
        r = 'fail'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }
}