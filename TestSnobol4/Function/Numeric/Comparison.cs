using Snobol4.Common;
using Test.TestLexer;

namespace Test.Numeric;

[TestClass]
public class Comparison
{
    #region EQ

    [TestMethod]
    public void TEST_EQ_001()
    {
        var s = @"
        eq(0,0):s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_EQ_002()
    {
        var s = @"
        ~eq(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_EQ_003()
    {
        var s = @"
        eq(1.23,12) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_EQ_004()
    {
        var s = @"
        eq(1.0,1) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_EQ_005()
    {
        var s = @"
        eq(1,1.2) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_EQ_006()
    {
        var s = @"
        eq('1.0','1') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_EQ_007()
    {
        var s = @"
        eq('1','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_EQ_008()
    {
        var s = @"
        eq('b','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(101, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_EQ_009()
    {
        var s = @"
        eq('1.2','b') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(102, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region GE

    [TestMethod]
    public void TEST_GE_001()
    {
        var s = @"
        ge(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GE_002()
    {
        var s = @"
        ~ge(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GE_003()
    {
        var s = @"
        ge(1.2,1.23) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GE_004()
    {
        var s = @"
        ge(1.0,1) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GE_005()
    {
        var s = @"
        ge(1,1.2) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GE_006()
    {
        var s = @"
        ge('1.0','1') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GE_007()
    {
        var s = @"
        ge('1','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GE_008()
    {
        var s = @"
        ge('b','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(109, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_GE_009()
    {
        var s = @"
        ge('1.2','b') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(110, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region GT

    [TestMethod]
    public void TEST_GT_001()
    {
        var s = @"
        gt(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GT_002()
    {
        var s = @"
        ~gt(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GT_003()
    {
        var s = @"
        gt(1.2,1.23) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GT_004()
    {
        var s = @"
        gt(1.0,1) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GT_005()
    {
        var s = @"
        gt(1,1.2) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GT_006()
    {
        var s = @"
        gt('1.0','1') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GT_007()
    {
        var s = @"
        gt('1','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_GT_008()
    {
        var s = @"
        gt('b','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(111, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_GT_009()
    {
        var s = @"
        gt('1.2','b') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(112, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region LT

    [TestMethod]
    public void TEST_LT_001()
    {
        var s = @"
        lt(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LT_002()
    {
        var s = @"
        ~lt(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LT_003()
    {
        var s = @"
        lt(1.2,1.23) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LT_004()
    {
        var s = @"
        lt(1.0,1) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LT_005()
    {
        var s = @"
        lt(1,1.2) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LT_006()
    {
        var s = @"
        lt('1.0','1') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LT_007()
    {
        var s = @"
        lt('1','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LT_008()
    {
        var s = @"
        lt('b','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(147, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_LT_009()
    {
        var s = @"
        lt('1.2','b') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(148, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region LE

    [TestMethod]
    public void TEST_LE_001()
    {
        var s = @"
        le(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LE_002()
    {
        var s = @"
        ~le(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LE_003()
    {
        var s = @"
        le(1.2,1.23) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LE_004()
    {
        var s = @"
        le(1.0,1) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LE_005()
    {
        var s = @"
        le(1,1.2) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LE_006()
    {
        var s = @"
        le('1.0','1') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LE_007()
    {
        var s = @"
        le('1','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_LE_008()
    {
        var s = @"
        le('b','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(121, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_LE_009()
    {
        var s = @"
        le('1.2','b') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(122, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region NE

    [TestMethod]
    public void TEST_NE_001()
    {
        var s = @"
        ne(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_NE_002()
    {
        var s = @"
        ~ne(0,0) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_NE_003()
    {
        var s = @"
        ne(1.2,1.23) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_NE_004()
    {
        var s = @"
        ne(1.0,1) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_NE_005()
    {
        var s = @"
        ne(1,1.2) :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_NE_006()
    {
        var s = @"
        ne('1.0','1') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("false", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_NE_007()
    {
        var s = @"
        ne('1','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var a = build.Execute!.IdentifierTable[build.FoldCase("","a")];
        Assert.AreEqual("true", ((StringVar)a).Data);
    }

    [TestMethod]
    public void TEST_NE_008()
    {
        var s = @"
        ne('b','1.2') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(149, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_NE_009()
    {
        var s = @"
        ne('1.2','b') :s(true)f(false)
true    a =  'true'   :(end)
false   a =  'false'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(150, build.ErrorCodeHistory[0]);
    }

    #endregion

}