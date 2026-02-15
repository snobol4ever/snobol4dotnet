using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

public class Exponentiation
{
    [TestMethod]
    public void TEST_Integer_Exponentiation_1()
    {
        var s = " a = 2 ^ 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(8L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Exponentiation_2()
    {
        var s = " a = 0 ^ 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(0L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Exponentiation_4()
    {
        var s = " a = 2 ^ \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(8L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Exponentiation_5()
    {
        var s = " a = \"2\" ^ 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(8L, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Exponentiation_6()
    {
        var s = " a = \"2\" ^ \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(8L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Exponentiation_1()
    {
        var s = " a = 2.1 ^ 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Pow(2.1, 3.1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Exponentiation_2()
    {
        var s = " a = 2.1 ^ \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Pow(2.1, 3.1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Exponentiation_3()
    {
        var s = " a = \"2.1\" ^ 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Pow(2.1, 3.1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Exponentiation_4()
    {
        var s = " a = \"2.1\" ^ \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Pow(2.1, 3.1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Exponentiation_1()
    {
        var s = " a = 2 ^ 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Pow(2, 3.1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Exponentiation_002()
    {
        var s = " a = 2.1 ^ 3;";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Pow(2.1, 3), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_015_Exponentiation()
    {
        var s = " a = 'a' ^ 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(15, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_016_Exponentiation()
    {
        var s = " a = 2 ^ 'b'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(16, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_018_Exponentiation()
    {
        //-9223372036854775808 to 9223372036854775807
        var s = " a = 9223372036854775807 ^ 2";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(17, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_018_Exponentiation001()
    {
        //-9223372036854775808 to 9223372036854775807
        var s = " 0 ^ 0";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(18, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_018_Exponentiation002()
    {
        //-9223372036854775808 to 9223372036854775807
        var s = " 0.0 ^ 0.0";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(18, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_018_Exponentiation003()
    {
        //-9223372036854775808 to 9223372036854775807
        var s = " 0.0 ^ (0 - 1.0)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(18, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_018_Exponentiation004()
    {
        //-9223372036854775808 to 9223372036854775807
        var s = " 0 ^ (0 - 2)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(18, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_018_Exponentiation_005()
    {
        //-9223372036854775808 to 9223372036854775807
        //±5.0e?324 to ±1.7e308
        var s = " a = 0.0 ^ ( 0.0 - 3.0)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(18, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_266_Exponentiation_001()
    {
        //-9223372036854775808 to 9223372036854775807
        //±5.0e?324 to ±1.7e308
        var s = " a = 1.6e308 ^ 0.7e308";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(266, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_311_Exponentiation_001()
    {
        var s = " a = ( 0 - 2 ) ^ (0 - 3.5)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(311, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_311_Exponentiation_003()
    {
        var s = " a = (0 - 2.5) ^ (0 - 3.5)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(311, build.ErrorCodeHistory[0]);
    }
}