using Snobol4.Common;
using Test.TestLexer;

namespace Test.Numeric;

public class NumericFunction
{
    [TestMethod]
    public void TEST_Integer_Remainder_1()
    {
        var s = " a = remdr(7,3)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(1L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Remainder_2()
    {
        var s = " a = remdr(7,'3')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(1L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Remainder_3()
    {
        var s = " a = remdr('7',3)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(1L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Remainder_4()
    {
        var s = " a = remdr('7','3')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(1L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Remainder_1()
    {
        var s = " a = remdr(7.1,3.1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(7.1 % 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Remainder_2()
    {
        var s = " a = remdr(7.1,'3.1')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(7.1 % 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Remainder_3()
    {
        var s = " a = remdr('7.1',3.1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(7.1 % 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Remainder_4()
    {
        var s = " a = remdr('7.1','3.1')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(7.1 % 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Remainder_1()
    {
        var s = " a = remdr(7,3.1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(7 % 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Remainder_2()
    {
        var s = " a = remdr(7.1,3)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(7.1 % 3, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Remainder_3()
    {
        var s = " a = remdr('7',3.1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(7 % 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Remainder_4()
    {
        var s = " a = remdr('7.1','3')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(7.1 % 3, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_165_Remainder()
    {
        var s = " a = remdr(2.1,'b')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(165, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_166_Remainder()
    {
        var s = " a = remdr('a', 3.2)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(166, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_167_Remainder_001()
    {
        var s = " a = remdr(2,0)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(167, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_312_Remainder_0021()
    {
        var s = " a = remdr(2.1,0)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(312, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_312_Remainder_0022()
    {
        var s = " a = remdr(2.1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(312, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Remainder_Identifier_1()
    {
        var s = " a = remdr";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", build.Execute!.IdentifierTable[build.FoldCase("","remdr")].ToString());
        Assert.AreEqual("", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Remainder_Identifier_2()
    {
        var s = " a = remdr = 'test'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("test", build.Execute!.IdentifierTable[build.FoldCase("","remdr")].ToString());
        Assert.AreEqual("test", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

}