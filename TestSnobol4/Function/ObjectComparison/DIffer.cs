using Snobol4.Common;
using Test.TestLexer;

namespace Test.ObjectComparison;

[TestClass]
public class Differ
{
    [TestMethod]
    public void TEST_NotDiffer_AnyPattern()
    {
        var s = @"
        a = any('abc')
        b = any('abc')
        differ(a,b)    :s(diff)
        out = 'not differ'    :(end)
diff    out = 'differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_AnyPattern()
    {
        var s = @"
        a = any('abc')
        b = a
        differ(a,b)    :s(diff)
        out = 'not differ'    :(end)
diff    out = 'differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_NotDiffer_ArbNoPattern()
    {
        var s = @"
        a = arbno(any('abc'))
        b = arbno(any('abc'))
        differ(a,b)    :s(diff)
        out = 'not differ'    :(end)
diff    out = 'differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_ArbNoPattern()
    {
        var s = @"
        a = arbno(any('abc'))
        b = a
        differ(a,b)    :s(diff)
        out = 'not differ'    :(end)
diff    out = 'differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_NotDiffer_ArbNoAnyPattern()
    {
        var s = @"
        a = arbno(any('abc'))
        b = arbno(notany('abc'))
        differ(a,b)    :s(diff)
        out = 'not differ'    :(end)
diff    out = 'differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_NotDiffer_AtPattern()
    {
        var s = @"
        a = any('abc') @n break('xyz')
        b = any('abc') @n break('xyz')
        differ(a,b)    :s(diff)
        out = 'not differ'    :(end)
diff    out = 'differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_NotDiffer_BalPattern()
    {
        var s = @"
        a = any('abc') bal len(10)
        b = any('abc') bal len(10)
        differ(a,b)    :s(diff)
        out = 'not differ'    :(end)
diff    out = 'differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_BalPattern_Non()
    {
        var s = @"
        a = any('abc') bal len(10)
        b = any('abc') bal len(12)
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_NotDiffer_FencePattern()
    {
        var s = @"
        a = fence 'abc' pos(10)
        b = fence 'abc' pos(10)
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }


    [TestMethod]
    public void TEST_NotDiffer_ComplexPattern()
    {
        var s = @"
        a = snoExpr7 ('#' 2) | epsilon
        b = snoExpr7 ('#' 2) | epsilon
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_NotDiffer_Array_Array()
    {
        var s = @"
        a = array(10)
        b = array(10)
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_NotDiffer_Table_Table()
    {
        var s = @"
        a = table()
        b = table()
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Array_Array()
    {
        var s = @"
        a = array(10)
        b = a
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Table_Table()
    {
        var s = @"
        a = table()
        b = a
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }


    [TestMethod]
    public void TEST_Differ_Integer_Array()
    {
        var s = @"
        a = 2
        b = array(10)
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Integer_Code()
    {
        var s = @"
        a = 2
        b = code(' b = 1.0')
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Integer_Integer()
    {
        var s = @"
        a = 2
        b = 2
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Integer_Integer_Diff()
    {
        var s = @"
        a = 2
        b = 1
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }


    [TestMethod]
    public void TEST_Differ_Integer_Pattern()
    {
        var s = @"
        a = 2
        b = Any('1.0')
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Integer_Real()
    {
        var s = @"
        a = 2
        b = 1.0
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Integer_String()
    {
        var s = @"
        a = 2
        b = '1.0'
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Integer_Table()
    {
        var s = @"
        a = 2
        b = table()
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    
    [TestMethod]
    public void TEST_Differ_Real_Array()
    {
        var s = @"
        a = 1.0
        b = array(10)
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Real_Code()
    {
        var s = @"
        a = 1.0
        b = code(' b = 1.0')
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Real_Integer()
    {
        var s = @"
        a = 1.0
        b = 1
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Real_Pattern()
    {
        var s = @"
        a = 1.0
        b = Any('1.0')
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Real_Real()
    {
        var s = @"
        a = 1.0
        b = 1.0
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Real_String()
    {
        var s = @"
        a = 1.0
        b = '1.0'
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_Real_Table()
    {
        var s = @"
        a = 1.0
        b = table()
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    
    [TestMethod]
    public void TEST_Differ_String_Array()
    {
        var s = @"
        a = '1.0'
        b = array(10)
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_String_Code()
    {
        var s = @"
        a = '1.0'
        b = code(' b = 1.0')
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_String_Integer()
    {
        var s = @"
        a = '1.0'
        b = 1
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_String_Pattern()
    {
        var s = @"
        a = '1.0'
        b = Any('1.0')
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_String_Real()
    {
        var s = @"
        a = '1.0'
        b = 1.0
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_String_String()
    {
        var s = @"
        a = '1.0'
        b = '1.0'
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }

    [TestMethod]
    public void TEST_Differ_String_Table()
    {
        var s = @"
        a = '1.0'
        b = table()
        differ(a,b)    :s(diff)
        out = 'differ'    :(end)
diff     out = 'not differ'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not differ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","out")]).Data);
    }


}