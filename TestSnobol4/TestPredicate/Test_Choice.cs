using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestPredicate;

[TestClass]
public class TestChoice
{
    [TestMethod]
    public void TEST_Choice_001()
    {
        var s = @"
     a = 1
     b = 2
     c = (lt(a,b) - 1,~ne(a,b) + 0,gt(a,b) + 1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(-1L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
    }

    [TestMethod]
    public void TEST_Choice_002()
    {
        var s = @"
     a = 5
     b = 5
     c = (lt(a,b) - 1,~ne(a,b) + 0,gt(a,b) + 1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
    }

    [TestMethod]
    public void TEST_Choice_003()
    {
        var s = @"
     a = 8
     b = 2
     c = (lt(a,b) - 1,~ne(a,b) + 0,gt(a,b) + 1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(1L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
    }

}