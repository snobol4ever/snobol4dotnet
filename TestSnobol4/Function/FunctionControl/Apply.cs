using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

[TestClass]
public class Apply
{

    [TestMethod]
    public void TEST_Apply_001()
    {
        var s = @"
          a = 'gt'
        b = 3
        c = 1
        r1 = gt(b,c) 'success'
        r2 = apply(a, b, c) 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Apply_002()
    {
        var s = @"
          a = 'gt'
        b = 3
        c = 1
        r1 = gt(b) 'success'
        r2 = apply(a, b) 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Apply_003()
    {
        var s = @"
          a = 'gt'
        b = 3
        c = 1
        r1 = gt(,b) 'success'
        r2 = apply(,b) 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(60, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Apply_004()
    {
        var s = @"
        a = 'gt'
        b = 3
        c = 1
        r1 = gt(c,b) 'success'
        r2 = apply(a,c,b) 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Apply_005()
    {
        var s = @"
        a = 'gt'
        b = 3
        c = 1
        r1 = gt(b) 'success'
        r2 = apply(a,b) 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

}