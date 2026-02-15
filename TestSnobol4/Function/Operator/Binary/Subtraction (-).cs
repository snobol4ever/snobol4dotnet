using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

public class Subtraction
{
    [TestMethod]
    public void TEST_Integer_Subtraction_1()
    {
        var s = " a = 2 - 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(-1L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Subtraction_2()
    {
        var s = "  a = 2 - \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(-1L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Subtraction_3()
    {
        var s = " a = \"2\" - 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(-1L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Subtraction_4()
    {
        var s = " a = \"2\" - \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(-1L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Subtraction_1()
    {
        var s = " a = 2.1 - 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 - 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Subtraction_2()
    {
        var s = " a = 2.1 - \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 - 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Subtraction_3()
    {
        var s = " a = \"2.1\" - 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 - 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Subtraction_4()
    {
        var s = " a = \"2.1\" - \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 - 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Subtraction_1()
    {
        var s = " a = 2 - 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2 - 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Subtraction_2()
    {
        var s = " a = 2.1 - 3;";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 - 3, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_032_Subtraction()
    {
        var s = " a = 'a' - 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(32, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_033_Subtraction()
    {
        var s = " a = 2 - 'b'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(33, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_034_Subtraction()
    {
        //-9223372036854775808 to 9223372036854775807
        var s = " a = (0 - 9223372036854775807) - 2";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(34, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_261_Subtraction()
    {
        //-9223372036854775808 to 9223372036854775807
        //±5.0e?324 to ±1.7e308
        var s = " a = ( 0 - 1.6e308 ) - 0.7e308";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(264, build.ErrorCodeHistory[0]);
    }
}