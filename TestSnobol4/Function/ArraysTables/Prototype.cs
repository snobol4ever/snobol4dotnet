using Snobol4.Common;
using Test.TestLexer;

namespace Test.ArraysTables;

[TestClass]
public class Prototype
{

    [TestMethod]
    public void TEST_Prototype_001()
    {
        var s = @"
        a = array('-5:10,3:5,20')
        b = prototype(a)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("-5:10,3:5,20", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }


    [TestMethod]
    public void TEST_Prototype_002()
    {
        var s = @"
        a = array(20)
        b = prototype(a)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("20", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }


    [TestMethod]
    public void TEST_Prototype_003()
    {
        var s = @"
        a = table(20)
        b = prototype(a)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_Prototype_004()
    {
        var s = @"
        prototype(u)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(164, build.ErrorCodeHistory[0]);
    }
}