using Snobol4.Common;
using Test.TestLexer;

namespace Test.Numeric;

[TestClass]
public class Trig
{
    #region ATAN

    [TestMethod]
    public void TEST_Integer_Atan_1()
    {
        var s = " a = atan(1,0,0,0)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Atan(1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Integer_Atan_2()
    {
        var s = " a = atan(1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Atan(1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Real_Atan_1()
    {
        var s = " a = atan(0.0)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Atan(0.0), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Real_Atan_2()
    {
        var s = " a = atan(0.7)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Atan(0.7), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Identifier_Atan_1()
    {
        var s = " a = atan";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual("", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Identifier_Atan_2()
    {
        var s = " atan = 'test'";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("test", build.Execute!.IdentifierTable[build.FoldCase("","atan")].ToString());
    }

    [TestMethod]
    public void TEST_301_Atan()
    {
        var s = " a = atan('test')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(301, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region CHOP

    [TestMethod]
    public void TEST_Chop_1()
    {
        var s = " a = chop(2.7)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(2.0, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Chop_2()
    {
        var s = " a = chop('-1.7')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(-1.0, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Chop_3()
    {
        var s = " a = chop(1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(1.0, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_302_Chop()
    {
        var s = " a = chop('test')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(302, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region COS

    [TestMethod]
    public void TEST_Cos_1()
    {
        var s = " a = cos(0.7)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Cos(0.7), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Cos_2()
    {
        var s = " a = cos('0.7')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Cos(0.7), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Cos_3()
    {
        var s = " a = cos(1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Cos(1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_303_Cos()
    {
        var s = " a = cos('test')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(303, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region EXP

    [TestMethod]
    public void TEST_Exp_1()
    {
        var s = " a = exp(0.7)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Exp(0.7), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Exp_2()
    {
        var s = " a = exp('0.7')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Exp(0.7), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Exp_3()
    {
        var s = " a = exp(1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Exp(1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_304_Exp()
    {
        var s = " a = exp('test')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(304, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_305_Exp()
    {
        var s = " a = exp(1.23e45)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(305, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region LN

    [TestMethod]
    public void TEST_Ln_1()
    {
        var s = " a = ln(2.75)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Log(2.75), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Ln_2()
    {
        var s = " a = ln('0.5')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Log(0.5), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Ln_3()
    {
        var s = " a = ln(1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(0, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_306_Ln()
    {
        var s = " a = ln('test')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(306, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_307_Ln()
    {
        var s = " a = ln(0)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(307, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_315_Ln()
    {
        var s = " a = ln(-1.0)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(315, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region REMDR

    [TestMethod]
    public void TEST_Remdr_0011()
    {
        var s = " a = remdr(100,31)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(100 % 31, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Remdr_002()
    {
        var s = " a = remdr(100.5,31.2)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(100.5 % 31.2, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    #endregion

    #region SIN

    [TestMethod]
    public void TEST_Sin_1()
    {
        var s = " a = sin(0.7)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(Math.Sin(0.7), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Sin_2()
    {
        var s = " a = sin('0.7')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(Math.Sin(0.7), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Sin_3()
    {
        var s = " a = sin(1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(Math.Sin(1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_303_Sin()
    {
        var s = " a = sin('test')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(308, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region SQRT

    [TestMethod]
    public void TEST_Sqrt_1()
    {
        var s = " a = sqrt(2.75)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Sqrt(2.75), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Sqrt_2()
    {
        var s = " a = sqrt('0.5')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(Math.Sqrt(0.5), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Sqrt_3()
    {
        var s = " a = sqrt(1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        
        Assert.AreEqual(1, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_313_Sqrt()
    {
        var s = " a = sqrt('test')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(313, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_314_Sqrt()
    {
        var s = " a = sqrt(-1.0)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(314, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region TAN

    [TestMethod]
    public void TEST_Tan_1()
    {
        var s = " a = tan(0.7)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(Math.Tan(0.7), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Tan_2()
    {
        var s = " a = tan('0.7')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(Math.Tan(0.7), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Tan_3()
    {
        var s = " a = tan(1)";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);

        Assert.AreEqual(Math.Tan(1), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Tan()
    {
        var s = " a = tan('test')";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(309, build.ErrorCodeHistory[0]);
    }


    #endregion

}