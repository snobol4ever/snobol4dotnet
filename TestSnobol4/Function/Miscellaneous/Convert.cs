using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

[TestClass]
public class Convert
{
    #region String -->

    [TestMethod]
    public void TEST_Convert_String_String()
    {
        var s = @"
        d = 'failure'
        a = 'test'
        b = convert(a,'string') :f(end)
        c = datatype(b) 
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("test", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("test", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_String_Integer()
    {
        var s = @"
        d = 'failure'
        a = '32767'
        b = convert(a,'integer') :f(end)
        c = datatype(b) 
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("32767", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual(32767, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("integer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_String_Integer_O_()
    {
        var s = @"
        d = 'failure'
        a = '327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767'
        b = convert(a,'integer') :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767327673276732767", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_String_Real()
    {
        var s = @"
        d = 'failure'
        a = '3.14159'
        b = convert(a,'real')  :f(end)
        c = datatype(b) 
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3.14159", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("real", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_String_Real_O_()
    {
        var s = @"
        d = 'failure'
        a = '3.14159e900'
        b = convert(a,'real')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3.14159e900", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_String_Array()
    {
        var s = @"
        d = 'failure'
        a = '3.14159'
        b = convert(a,'array')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_String_Table()
    {
        var s = @"
        d = 'failure'
        a = '3.14159'
        b = convert(a,'table')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_String_Pattern()
    {
        var s = @"
        d = 'failure'
        a = '3.14159'
        b = convert(a,'pattern')  :f(end)
        c = datatype(b) 
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3.14159", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("3.14159", ((LiteralPattern)((PatternVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data).Literal);
        Assert.AreEqual("pattern", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_String_Name()
    {
        var s = @"
        a = 'quick'
        b = convert(a,'name')   :f(end)
        c = datatype(b)
        d = convert(b,'string')   :f(end)
        e = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("quick", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("quick", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Pointer);
        Assert.AreEqual("name", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual("quick", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
    }


    [TestMethod]
    public void TEST_Convert_String_Expression()
    {
        var s = @"
        a = convert('3.14 * 2.0', 'expression') :f(end)
        b = datatype(a);
        c = eval(a)
        d = datatype(c)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("expression", build.Execute!.IdentifierTable[build.FoldCase("b")].ToString());
        Assert.AreEqual("6.28", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
        Assert.AreEqual("real", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_String_Code()
    {
        var s = @"
        S = ""L  A = A ' ' N; N = LT(N,10) N + 1 :S(L)F(DONE)""
        X = CONVERT(S,'code') :F(end)
                    :(L)
DONE    A
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("  1 2 3 4 5 6 7 8 9 10", build.Execute!.IdentifierTable[build.FoldCase("A")].ToString());
    }

    #endregion

    #region Integer -->

    [TestMethod]
    public void TEST_Convert_Integer_String()
    {
        var s = @"
        d = 'failure'
        a = 32767
        b = convert(a,'string') :f(end)
        c = datatype(b)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(32767, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("32767", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Integer_Integer()
    {
        var s = @"
        d = 'failure'
        a = 32767
        b = convert(a,'integer') :f(end)
        c = datatype(b)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(32767, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual(32767, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("integer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual("integer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Integer_Real()
    {
        var s = @"
        d = 'failure'
        a = 32767
        b = convert(a,'real')  :f(end)
        c = datatype(b)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(32767, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual(32767.0, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("real", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Integer_Array()
    {
        var s = @"
        d = 'failure'
        a = '3.14159'
        b = convert(a,'array') :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Integer_Table()
    {
        var s = @"
        d = 'failure'
        a = 32767
        b = convert(a,'table') :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Integer_Pattern()
    {
        var s = @"
        d = 'failure'
        a = '32767'
        b = convert(a,'pattern') :f(end)
        c = datatype(b)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("32767", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        var p = ((LiteralPattern)((PatternVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data).Literal;
        Assert.AreEqual("32767", p);
        Assert.AreEqual("pattern", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Integer_Expression()
    {
        var s = @"
        a = convert(3, 'expression')
        b = datatype(a);
        c = eval(a)
        d = datatype(c)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("expression", build.Execute!.IdentifierTable[build.FoldCase("b")].ToString());
        Assert.AreEqual("3", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
        Assert.AreEqual("integer", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Integer_Code()
    {
        var s = @"
        d = 'failure'
        a = convert(3, 'code') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    #endregion

    #region real -->

    [TestMethod]
    public void TEST_Convert_Real_String()
    {
        var s = @"
        d = 'failure'
        a = 3.14159
        b = convert(a,'string') :f(end)
        c = datatype(b)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("3.14159", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Real_Integer()
    {
        var s = @"
        d = 'failure'
        a = 3.14159
        b = convert(a,'integer') :f(end)
        c = datatype(b)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual(3, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("integer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Real_Integer_O_()
    {
        var s = @"
        d = 'failure'
        a = 3.14159e200
        b = convert(a,'integer') :f(end)
        c = datatype(b)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(3.14159e200, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Real_Real()
    {
        var s = @"
        d = 'failure'
        a = 3.14159
        b = convert(a,'real') :f(end)
        c = datatype(b)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("real", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Real_Array()
    {
        var s = @"
        d = 'failure'
        a = '3.14159'
        b = convert(a,'array') :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Real_Table()
    {
        var s = @"
        d = 'failure'
        a = 3.14159
        b = convert(a,'table') :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Real_Pattern()
    {
        var s = @"
        d = 'failure'
        a = '3.14159'
        b = convert(a,'pattern') :f(end)
        c = datatype(b) 
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3.14159", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("3.14159", ((LiteralPattern)((PatternVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data).Literal);
        Assert.AreEqual("pattern", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].CreationOrder, build.Execute!.IdentifierTable[build.FoldCase("b")].CreationOrder);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Real_Name()
    {
        var s = @"
        a = 3.14159
        b = convert(a,'name')   :f(end)
        c = datatype(b)
        d = convert(b,'real')   :f(end)
        e = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("3.14159", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Pointer);
        Assert.AreEqual("name", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
    }


    [TestMethod]
    public void TEST_Convert_Real_Expression()
    {
        var s = @"
        a = convert(3.14159, 'expression')
        b = datatype(a);
        c = eval(a)
        d = datatype(c)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("expression", build.Execute!.IdentifierTable[build.FoldCase("b")].ToString());
        Assert.AreEqual("3.14159", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
        Assert.AreEqual("real", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Real_Code()
    {
        var s = @"
        d = 'failure'
        a = convert(3.14159, 'code') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    #endregion

    #region Array -->

    [TestMethod]
    public void TEST_Convert_Array_String()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
	    b[1,1] = 'a'
	    b[2,1] = 'b'
	    b[3,1] = 'c'
	    b[4,1] = 'd'
	    b[5,1] = 'e'
        b[1,2] = 1
	    b[2,2] = 2
	    b[3,2] = 3
	    b[4,2] = 4
	    b[5,2] = 5
        a = convert(b, 'string') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Integer()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
	    b[1,1] = 'a'
	    b[2,1] = 'b'
	    b[3,1] = 'c'
	    b[4,1] = 'd'
	    b[5,1] = 'e'
        b[1,2] = 1
	    b[2,2] = 2
	    b[3,2] = 3
	    b[4,2] = 4
	    b[5,2] = 5
        a = convert(b, 'integer') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Real()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
	    b[1,1] = 'a'
	    b[2,1] = 'b'
	    b[3,1] = 'c'
	    b[4,1] = 'd'
	    b[5,1] = 'e'
        b[1,2] = 1
	    b[2,2] = 2
	    b[3,2] = 3
	    b[4,2] = 4
	    b[5,2] = 5
        a = convert(b, 'real') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Array()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
	    b[1,1] = 'a'
	    b[2,1] = 'b'
	    b[3,1] = 'c'
	    b[4,1] = 'd'
	    b[5,1] = 'e'
        b[1,2] = 1
	    b[2,2] = 2
	    b[3,2] = 3
	    b[4,2] = 4
	    b[5,2] = 5
        a = convert(b, 'array') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Table()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
	    b[1,1] = 'a'
	    b[2,1] = 'b'
	    b[3,1] = 'c'
	    b[4,1] = 'd'
	    b[5,1] = 'e'
        b[1,2] = 1
	    b[2,2] = 2
	    b[3,2] = 3
	    b[4,2] = 4
	    b[5,2] = 5
        a = convert(b, 'table') :f(end)
        r = a['a'] a['b'] a['c'] a['d'] a['e']
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
        Assert.AreEqual("12345", build.Execute!.IdentifierTable[build.FoldCase("r")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Table_2_()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
        a = convert(b, 'table') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Table_3_()
    {
        var s = @"
        d = 'failure'
        b = array('5,1')
        a = convert(b, 'table') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Table_4_()
    {
        var s = @"
        d = 'failure'
        b = array('5')
        a = convert(b, 'table') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Table_5_()
    {
        var s = @"
        d = 'failure'
        b = array('5,3')
        a = convert(b, 'table') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Pattern()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
	    b[1,1] = 'a'
	    b[2,1] = 'b'
	    b[3,1] = 'c'
	    b[4,1] = 'd'
	    b[5,1] = 'e'
        b[1,2] = 1
	    b[2,2] = 2
	    b[3,2] = 3
	    b[4,2] = 4
	    b[5,2] = 5
        a = convert(b, 'pattern') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }


    [TestMethod]
    public void TEST_Convert_Array_Name()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
	    b[1,1] = 'a'
	    b[2,1] = 'b'
	    b[3,1] = 'c'
	    b[4,1] = 'd'
	    b[5,1] = 'e'
        b[1,2] = 1
	    b[2,2] = 2
	    b[3,2] = 3
	    b[4,2] = 4
	    b[5,2] = 5
        a = convert(b, 'name') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Expression()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
	    b[1,1] = 'a'
	    b[2,1] = 'b'
	    b[3,1] = 'c'
	    b[4,1] = 'd'
	    b[5,1] = 'e'
        b[1,2] = 1
	    b[2,2] = 2
	    b[3,2] = 3
	    b[4,2] = 4
	    b[5,2] = 5
        a = convert(b, 'expression') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Array_Code()
    {
        var s = @"
        d = 'failure'
        b = array('5,2')
	    b[1,1] = 'a'
	    b[2,1] = 'b'
	    b[3,1] = 'c'
	    b[4,1] = 'd'
	    b[5,1] = 'e'
        b[1,2] = 1
	    b[2,2] = 2
	    b[3,2] = 3
	    b[4,2] = 4
	    b[5,2] = 5
        a = convert(b, 'code') :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }



    #endregion

    #region Table -->

    [TestMethod]
    public void TEST_Convert_Table_String()
    {
        var s = @"
        d = 'failure'
	    t = table()
	    t[1] = 2
	    t[2] = 4
	    t[3] = 6
	    t[4] = 8
	    t[5] = 10

	    b = convert(t, 'string')  :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Table_Integer()
    {
        var s = @"
        d = 'failure'
	    t = table()
	    t[1] = 2
	    t[2] = 4
	    t[3] = 6
	    t[4] = 8
	    t[5] = 10

	    b = convert(t, 'integer')  :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Table_Real()
    {
        var s = @"
        d = 'failure'
	    t = table()
	    t[1] = 2
	    t[2] = 4
	    t[3] = 6
	    t[4] = 8
	    t[5] = 10

	    b = convert(t, 'real')  :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Table_Array()
    {
        var s = @"
	    t = table()
	    t[1] = 2
	    t[2] = 4
	    t[3] = 6
	    t[4] = 8
	    t[5] = 10

	    b = convert(t, 'array')

	    r1 = b[1,1] ' ' b[1,2]
	    r2 = b[2,1] ' ' b[2,2]
	    r3 = b[3,1] ' ' b[3,2]
	    r4 = b[4,1] ' ' b[4,2]
	    r5 = b[5,1] ' ' b[5,2]
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("1 2", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("2 4", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
        Assert.AreEqual("3 6", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r3")]).Data);
        Assert.AreEqual("4 8", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r4")]).Data);
        Assert.AreEqual("5 10", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r5")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Table_Array_2_()
    {
        var s = @"
	    t = table()
	    t[1] = 'one'
	    t[2] = 'two'
	    t[3] = 'three'
	    t[4] = 'four'
	    t[5] = 'five'

	    b = convert(t, 'array')

	    r1 = b[1,1] ' ' b[1,2]
	    r2 = b[2,1] ' ' b[2,2]
	    r3 = b[3,1] ' ' b[3,2]
	    r4 = b[4,1] ' ' b[4,2]
	    r5 = b[5,1] ' ' b[5,2]
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("1 one", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("2 two", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
        Assert.AreEqual("3 three", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r3")]).Data);
        Assert.AreEqual("4 four", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r4")]).Data);
        Assert.AreEqual("5 five", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r5")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Table_Array_3_()
    {
        var s = @"
	    t = table()
	    t[1] = any('one')
	    t[2] = any('two')
	    t[3] = any('three')
	    t[4] = any('four')
	    t[5] = any('five')

	    b = convert(t, 'array')

	    r1 = b[1,1] ' ' b[1,2]
	    r2 = b[2,1] ' ' b[2,2]
	    r3 = b[3,1] ' ' b[3,2]
	    r4 = b[4,1] ' ' b[4,2]
	    r5 = b[5,1] ' ' b[5,2]
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("pattern", build.Execute!.IdentifierTable[build.FoldCase("r1")].DataType());
        Assert.AreEqual("pattern", build.Execute!.IdentifierTable[build.FoldCase("r2")].DataType());
        Assert.AreEqual("pattern", build.Execute!.IdentifierTable[build.FoldCase("r3")].DataType());
        Assert.AreEqual("pattern", build.Execute!.IdentifierTable[build.FoldCase("r4")].DataType());
        Assert.AreEqual("pattern", build.Execute!.IdentifierTable[build.FoldCase("r5")].DataType());
    }

    [TestMethod]
    public void TEST_Convert_Table_Table()
    {
        var s = @"
        d = 'failure'
	    t = table()
	    t[1] = 2
	    t[2] = 4
	    t[3] = 6
	    t[4] = 8
	    t[5] = 10

	    b = convert(t, 'table')  :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Table_Pattern()
    {
        var s = @"
        d = 'failure'
	    t = table()
	    t[1] = 2
	    t[2] = 4
	    t[3] = 6
	    t[4] = 8
	    t[5] = 10

	    b = convert(t, 'pattern')  :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Table_Name()
    {
        var s = @"
        d = 'failure'
	    t = table()
	    t[1] = 2
	    t[2] = 4
	    t[3] = 6
	    t[4] = 8
	    t[5] = 10

	    b = convert(t, 'name')  :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Table_Expression()
    {
        var s = @"
        d = 'failure'
	    t = table()
	    t[1] = 2
	    t[2] = 4
	    t[3] = 6
	    t[4] = 8
	    t[5] = 10

	    b = convert(t, 'expression')  :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Table_Code()
    {
        var s = @"
        d = 'failure'
	    t = table()
	    t[1] = 2
	    t[2] = 4
	    t[3] = 6
	    t[4] = 8
	    t[5] = 10

	    b = convert(t, 'code')  :f(end)
        d = 'success'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    #endregion

    #region Name -->

    [TestMethod]
    public void TEST_Convert_Name_String()
    {
        var s = @"
        a = 'something'
        output = b = convert(a, 'name')   :f(end)
        output = c = datatype(b)
        output = d = convert(b, 'string')   :f(end)
        output = e = datatype(d)
        output = f = 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("something", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Pointer);
        Assert.AreEqual("name", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("f")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Name_String_2_()
    {
        var s = @"
        a = '#$%'
        output = b = convert(a, 'name')   :f(end)
        output = c = datatype(b)
        output = d = convert(b, 'string')   :f(end)
        output = e = datatype(d)
        output = f = 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("#$%", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("#$%", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Pointer);
        Assert.AreEqual("name", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual("#$%", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("f")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Name_Integer()
    {
        var s = @"
        a = 32767
        output = b = convert(a,'name')   :f(end)
        output = c = datatype(b)
        output = d = convert(b,'integer')   :f(end)
        output = e = datatype(d)
        output = f = 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(32767, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("32767", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Pointer);
        Assert.AreEqual("name", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual(32767, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("integer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("f")]).Data);
    }


    [TestMethod]
    public void TEST_Convert_Integer_Name()
    {
        var s = @"
        a = 123456
        b = convert(a,'name')   :f(end)
        c = datatype(b)
        d = convert(b,'integer')   :f(end)
        e = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(123456, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("123456", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Pointer);
        Assert.AreEqual("name", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual(123456, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Name_Real()
    {
        var s = @"
        a = 3.14159
        output = b = convert(a,'name')   :f(end)
        output = c = datatype(b)
        output = d = convert(b,'real')   :f(end)
        output = e = datatype(d)
        output = f = 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("3.14159", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Pointer);
        Assert.AreEqual("name", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("real", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("f")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Name_Array()
    {
        var s = @"
        d = 'failure'
        a = 'something'
        b = convert(.a,'array')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Name_Table()
    {
        var s = @"
        d = 'failure'
        a = 'something'
        b = convert(.a,'table')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Name_Pattern()
    {
        var s = @"
        output = a = .abc
		output = b = datatype(a)
		output = c = convert(b , 'pattern')
		output = d = datatype(c)
        output = e = 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        if (build.CaseFolding)
        {
            Assert.AreEqual("ABC", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
        }
        else
        {
            Assert.AreEqual("abc", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
        }
        Assert.AreEqual("name", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("pattern", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
        Assert.AreEqual("pattern", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Name_Name()
    {
        var s = @"
        d = 'failure'
        b = convert(.abc,'name')   :f(end)
        c = datatype(.abc)
        d = 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("name", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Name_Expression()
    {
        var s = @"
        e = convert('1 + 2', 'name')
        h = convert(e, 'expression')
        $e = 45
        c = $e    
        f = eval(e)
        g = $'1 + 2'
        d = 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("1 + 2", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Pointer);
        Assert.AreEqual(45, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual(45, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("g")]).Data);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    [TestMethod]
    public void TEST_Convert_Name_Code()
    {
        var s = @"
        d = 'failure'
        e = convert(' 1 + 2', 'name')
        h = convert(e, 'code')
        c = $e = 45
        f = code(e)
        g = $' 1 + 2'
        d = 'success'
end";
        var directives = "-b -cs";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(" 1 + 2", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Pointer);
        Assert.AreEqual(45, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual(45, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("g")]).Data);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("d")].ToString());
    }

    #endregion

    #region Pattern -->

    [TestMethod]
    public void TEST_Convert_Pattern_Array()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'array')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Pattern_Table()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'table')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Pattern_Integer()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'integer')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Pattern_Real()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'real')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Pattern_Pattern()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'pattern')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Pattern_String()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'string')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Pattern_Expression()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'expression')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Pattern_Code()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'code')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Pattern_Name()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'name')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    #endregion

    #region Expression -->

    [TestMethod]
    public void TEST_Convert_Expression_Array()
    {
        var s = @"
        d = 'failure'
        a = *(len(k) pos(m))
        b = convert(a,'array')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Expression_Table()
    {
        var s = @"
        d = 'failure'
        a = *(len(k) pos(m))
        b = convert(a,'table')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }
    
    [TestMethod]
    public void TEST_Convert_Expression_String()
    {
        var s = @"
        d = 'failure'
        a = *(len(k) pos(m))
        b = convert(a,'string')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }
    
    [TestMethod]
    public void TEST_Convert_Expression_Name()
    {
        var s = @"
        d = 'failure'
        a = *(len(k) pos(m))
        b = convert(a,'name')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Expression_Integer()
    {
        var s = @"
        d = 'failure'
        a = any('abc')
        b = convert(a,'integer')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Expression_Real()
    {
        var s = @"
        d = 'failure'
        a = *(len(k) pos(m))
        b = convert(a,'string')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Expression_Pattern()
    {
        var s = @"
        d = 'failure'
        a = *(len(k) pos(m))
        b = convert(a,'pattern')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Expression_Expression()
    {
        var s = @"
        d = 'failure'
        a = *(len(k) pos(m))
        b = convert(a,'expression')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Expression_Code()
    {
        var s = @"
        d = 'failure'
        a = *(len(k) pos(m))
        b = convert(a,'code')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    #endregion

    #region Code -->

    [TestMethod]
    public void TEST_Convert_Code_Array()
    {
        var s = @"
        d = 'failure'
        a = code(' len(k) pos(m)')
        b = convert(a,'array')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Code_Table()
    {
        var s = @"
        d = 'failure'
        a = code(' len(k) pos(m)')
        b = convert(a,'table')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Code_String()
    {
        var s = @"
        d = 'failure'
        a = code(' len(k) pos(m)')
        b = convert(a,'string')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Code_Name()
    {
        var s = @"
        d = 'failure'
        a = code(' len(k) pos(m)')
        b = convert(a,'name')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Code_Integer()
    {
        var s = @"
        d = 'failure'
        a = code(' len(k) pos(m)')
        b = convert(a,'integer')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Code_Real()
    {
        var s = @"
        d = 'failure'
        a = code(' len(k) pos(m)')
        b = convert(a,'string')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Code_Pattern()
    {
        var s = @"
        d = 'failure'
        a = code(' len(k) pos(m)')
        b = convert(a,'pattern')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Code_Expression()
    {
        var s = @"
        d = 'failure'
        a = code(' len(k) pos(m)')
        b = convert(a,'expression')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    [TestMethod]
    public void TEST_Convert_Code_Code()
    {
        var s = @"
        d = 'failure'
        a = code(' len(k) pos(m)')
        b = convert(a,'code')  :f(end)
        d = 'success'
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
    }

    #endregion
}