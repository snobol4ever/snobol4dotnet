using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

[TestClass]
public class Plus
{
    [TestMethod]
    public void TEST_Integer_UnaryPlus_1()
    {
        var s = " a = +3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(3L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_UnaryPlus_2()
    {
        var s = "  a = +'3'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(3L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_UnaryPlus_3()
    {
        var s = " a = +''";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(0, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_UnaryPlus_4()
    {
        var s = " a = +\"\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(0, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_UnaryPlus_1n()
    {
        var s = " a = +-3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(-3L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_UnaryPlus_2n()
    {
        var s = "  a = +-'3'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(-3L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_UnaryPlus_3n()
    {
        var s = " a = +-''";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(0, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_UnaryPlus_4n()
    {
        var s = " a = +-\"\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(0, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_004_UnaryPlus()
    {
        var s = " a = +'a'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(4, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Real_UnaryPlus_keeps_the_sign()
    {
        // sbl -b: +'4.5' is 4.5, +X with X = 6.125 is 6.125, +'-2.5' is -2.5 (affirmation does not negate)
        var build = SetupTests.SetupScript("-b", @"
        X = 6.125
        R = +'4.5' ' ' +X ' ' +'-2.5'
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("4.5 6.125 -2.5", build.Execute!.IdentifierTable["R"].ToString());
    }
}
