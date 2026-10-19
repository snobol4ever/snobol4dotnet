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
        // sbl -b: b = a is the same pattern, so IDENT(a,b) succeeds and DIFFER(a,b) fails
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
        Assert.AreEqual("identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
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
        // sbl -b: b = a is the same pattern, so IDENT(a,b) succeeds and DIFFER(a,b) fails
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
        Assert.AreEqual("identical", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("out")]).Data);
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

    [TestMethod]
    public void TEST_Ident_names_compare_by_what_they_name()
    {
        // sbl -b: a name is identical only to a name of the same variable or element; IDENT(.X) fails and
        // DIFFER(.X) succeeds, and each .&KEYWORD is a new name (aisnobol's IN tests IDENT(IN...N) to see
        // whether a name was passed). snobol4dotnet called every name identical to everything
        var build = SetupTests.SetupScript("-b", """
                 N = .X
                 A = ARRAY(3) ; B = ARRAY(3) ; T = TABLE()
                 R = R IDENT(N, .X) 'a'
                 R = R IDENT(N, .Y) 'b'
                 R = R IDENT(N, 'X') 'c'
                 R = R IDENT(N) 'd'
                 R = R DIFFER(N) 'e'
                 R = R IDENT(.A<1>, .A<1>) 'f'
                 R = R IDENT(.A<1>, .A<2>) 'g'
                 R = R IDENT(.A<1>, .B<1>) 'h'
                 R = R IDENT(.T<'k'>, .T<'k'>) 'i'
                 R = R IDENT(.T<'k'>, .T<'j'>) 'j'
                 R = R IDENT(.&ANCHOR, .&ANCHOR) 'k'
                 K = .&ANCHOR
                 R = R IDENT(K, K) 'l'
         end
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("aefil", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data);
    }

    [TestMethod]
    public void TEST_Ident_patterns_keep_identity_through_copies()
    {
        // sbl -bf: a pattern is one object however it is copied (Q = P, P '', '' P, an array element, BAL NULL
        // is BAL), while two constructions are two patterns; testpgms' test1 checks DIFFER(BAL NULL,BAL).
        // snobol4dotnet cloned the pattern on every copy, so no copy was identical
        var build = SetupTests.SetupScript("-bf", """
                 P = LEN(1)
                 Q = P
                 R = R IDENT(Q, P) 'a'
                 R = R IDENT(P '', P) 'b'
                 R = R IDENT('' P, P) 'c'
                 A = ARRAY(2)
                 A<1> = P
                 R = R IDENT(A<1>, P) 'd'
                 R = R IDENT(BAL NULL, BAL) 'e'
                 R = R IDENT(LEN(1), LEN(1)) 'f'
                 R = R IDENT(P, LEN(1)) 'g'
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abcde", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data);
    }
}
