using Snobol4.Common;
using Test.TestLexer;

namespace Test.ObjectComparison;

[TestClass]
public class Ident
{
    [TestMethod]
    public void TEST_NotIdent_AnyPattern()
    {
        var s = @"
        a = any('abc')
        b = any('abc')
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_AnyPattern()
    {
        var s = @"
        a = any('abc')
        b = a
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_NotIdent_ArbNoPattern()
    {
        var s = @"
        a = arbno(any('abc'))
        b = arbno(any('abc'))
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_ArbNoPattern()
    {
        var s = @"
        a = arbno(any('abc'))
        b = a
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_NotIdent_ArbNoAnyPattern()
    {
        var s = @"
        a = arbno(any('abc'))
        b = arbno(notany('abc'))
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_NotIdent_AtPattern()
    {
        var s = @"
        a = any('abc') @n break('xyz')
        b = any('abc') @n break('xyz')
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_NotIdent_BalPattern()
    {
        var s = @"
        a = any('abc') bal len(10)
        b = any('abc') bal len(10)
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_BalPattern_Non()
    {
        var s = @"
        a = any('abc') bal len(10)
        b = any('abc') bal len(12)
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_NotIdent_FencePattern()
    {
        var s = @"
        a = fence 'abc' pos(10)
        b = fence 'abc' pos(10)
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }


    [TestMethod]
    public void TEST_NotIdent_ComplexPattern()
    {
        var s = @"
        a = snoExpr7 ('#' 2) | epsilon
        b = snoExpr7 ('#' 2) | epsilon
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_NotIdent_Array_Array()
    {
        var s = @"
        a = array(10)
        b = array(10)
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_NotIdent_Table_Table()
    {
        var s = @"
        a = table()
        b = table()
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Array_Array()
    {
        var s = @"
        a = array(10)
        b = a
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Table_Table()
    {
        var s = @"
        a = table()
        b = a
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }


    [TestMethod]
    public void TEST_Ident_Integer_Array()
    {
        var s = @"
        a = 2
        b = array(10)
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Integer_Code()
    {
        var s = @"
        a = 2
        b = code(' b = 1.0')
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Integer_Integer()
    {
        var s = @"
        a = 2
        b = 2
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Integer_Integer_Diff()
    {
        var s = @"
        a = 2
        b = 1
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }


    [TestMethod]
    public void TEST_Ident_Integer_Pattern()
    {
        var s = @"
        a = 2
        b = any('1.0')
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Integer_Real()
    {
        var s = @"
        a = 2
        b = 1.0
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Integer_String()
    {
        var s = @"
        a = 2
        b = '1.0'
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Integer_Table()
    {
        var s = @"
        a = 2
        b = table()
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    
    [TestMethod]
    public void TEST_Ident_Real_Array()
    {
        var s = @"
        a = 1.0
        b = array(10)
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Real_Code()
    {
        var s = @"
        a = 1.0
        b = code(' b = 1.0')
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Real_Integer()
    {
        var s = @"
        a = 1.0
        b = 1
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Real_Pattern()
    {
        var s = @"
        a = 1.0
        b = any('1.0')
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Real_Real()
    {
        var s = @"
        a = 1.0
        b = 1.0
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Real_String()
    {
        var s = @"
        a = 1.0
        b = '1.0'
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_Real_Table()
    {
        var s = @"
        a = 1.0
        b = table()
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    
    [TestMethod]
    public void TEST_Ident_String_Array()
    {
        var s = @"
        a = '1.0'
        b = array(10)
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_String_Code()
    {
        var s = @"
        a = '1.0'
        b = code(' b = 1.0')
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_String_Integer()
    {
        var s = @"
        a = '1.0'
        b = 1
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_String_Pattern()
    {
        var s = @"
        a = '1.0'
        b = any('1.0')
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_String_Real()
    {
        var s = @"
        a = '1.0'
        b = 1.0
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_String_String()
    {
        var s = @"
        a = '1.0'
        b = '1.0'
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_String_Table()
    {
        var s = @"
        a = '1.0'
        b = table()
        ident(a,b)    :s(equ)
        out = 'not identical'    :(end)
equ     out = 'identical'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
    }


}