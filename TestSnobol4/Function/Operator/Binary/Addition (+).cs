using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

[TestClass]
// ReSharper disable once InconsistentNaming
public class Addition
{
    [TestMethod]
    public void TEST_Integer_Addition_1()
    {
        var s = " a = 2 + 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(5L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Addition_2()
    {
        var s = " a = 2 + \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(5L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Addition_3()
    {
        var s = " a = \"2\" + 3";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(5L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Addition_4()
    {
        var s = " a = \"2\" + \"3\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(5L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Addition_1()
    {
        var s = " a = 2.1 + 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(5.2, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Addition_2()
    {
        var s = " a = 2.1 + \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(5.2, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Addition_3()
    {
        var s = " a = \"2.1\" + 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(5.2, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_REAL_Addition_4()
    {
        var s = " a = \"2.1\" + \"3.1\"";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(5.2, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_Mixed_Addition_1()
    {
        var s = " a = 2 + 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(5.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }


    [TestMethod]
    public void TEST_Mixed_Addition_2()
    {
        var s = " a = 2.1 + 3;";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(5.1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
    }

    [TestMethod]
    public void TEST_001_Addition()
    {
        var s = " a = 'a' + 3.1";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(1, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_002_Addition()
    {
        var s = " a = 2 + 'b'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(2, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_003_Addition()
    {
        //-9223372036854775808 to 9223372036854775807
        var s = @"
        &ERRLIMIT = 3
        a = 9223372036854775807 + 2  :S(OK)F(ERR)
ERR     result = 'Fail'              :(END)
OK      result = 'Succeed'           :(END)
END
                    ";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(3, build.ErrorCodeHistory[0]);
        Assert.AreEqual("Fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_261_Addition()
    {
        //-9223372036854775808 to 9223372036854775807
        //±5.0e?324 to ±1.7e308
        var s = " a = 1.6e308 + 0.7e308";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(261, build.ErrorCodeHistory[0]);
    }
}