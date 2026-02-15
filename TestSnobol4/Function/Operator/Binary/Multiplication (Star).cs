using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

public class Multiplication
{
    [TestMethod]
    public void TEST_Integer_Multiplication_1()
    {
        var s = " a = 2 * 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(6L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Multiplication_2()
    {
        var s = " a = 2 * \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(6L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Multiplication_3()
    {
        var s = " a = \"2\" * 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(6L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Multiplication_4()
    {
        var s = " a = \"2\" * \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(6L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Multiplication_1()
    {
        var s = " a = 2.1 * 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 * 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Multiplication_2()
    {
        var s = " a = 2.1 * \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 * 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Multiplication_3()
    {
        var s = " a = \"2.1\" * 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 * 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Multiplication_4()
    {
        var s = " a = \"2.1\" * \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 * 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Multiplication_1()
    {
        var s = " a = 2 * 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2 * 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Multiplication_2()
    {
        var s = "  a = 2.1 * 3;";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 * 3, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }
    [TestMethod]
    public void TEST_026_Multiplication()
    {
        var s = " a = 'a' * 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(26, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_027_Multiplication()
    {
        var s = " a = 2 * 'b'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(27, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_028_Multiplication()
    {
        //-9223372036854775808 to 9223372036854775807
        var s = " a = 9223372036854775807 * 2";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(28, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_261_Multiplication()
    {
        //-9223372036854775808 to 9223372036854775807
        //±5.0e?324 to ±1.7e308
        var s = " a = 1.6e308 * 0.7e308";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(263, build.ErrorCodeHistory[0]);
    }
}