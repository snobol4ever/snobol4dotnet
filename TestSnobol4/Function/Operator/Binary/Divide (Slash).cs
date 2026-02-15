using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

public class Divide
{
    [TestMethod]
    public void TEST_Integer_Division_1()
    {
        var s = " a = 7 / 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Division_2()
    {
        var s = " a = 7 / \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Division_3()
    {
        var s = " a = \"7\" / 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Division_4()
    {
        var s = " a = \"7\" / \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Division_1()
    {
        var s = " a = 2.1 / 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 / 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Division_2()
    {
        var s = " a = 2.1 / \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 / 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Division_3()
    {
        var s = " a = \"2.1\" / 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 / 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Division_4()
    {
        var s = " a = \"2.1\" / \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 / 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Division_1()
    {
        var s = " a = 2 / 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2 / 3.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Division_2()
    {
        var s = " a = 2.1 / 3;";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(2.1 / 3, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_001_Division()
    {
        var s = " a = 'a' / 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(12, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_013_Division()
    {
        var s = " a = 2 / 'b'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(13, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_014_Division()
    {
        //-9223372036854775808 to 9223372036854775807
        var s = " a = 9223372036854775807 / 0";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(14, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_262_Division()
    {
        //-9223372036854775808 to 9223372036854775807
        //±5.0e?324 to ±1.7e308
        var s = " a = 1.6e308 / 0.7e-308";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(262, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_262_Division_002()
    {
        //-9223372036854775808 to 9223372036854775807
        //±5.0e?324 to ±1.7e308
        var s = " a = 1.0 / 0.0";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(262, build.ErrorCodeHistory[0]);
    }


}