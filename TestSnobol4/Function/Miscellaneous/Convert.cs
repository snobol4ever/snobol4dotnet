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
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
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
        Assert.AreEqual("INTEGER", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
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
        Assert.AreEqual("REAL", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
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
        Assert.AreEqual("PATTERN", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
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
        // under -b a name's text is folded, as sbl -b prints for this program: quick NAME QUICK success
        Assert.AreEqual("QUICK", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Pointer);
        Assert.AreEqual("NAME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual("QUICK", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
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
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
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
        Assert.AreEqual("INTEGER", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual("INTEGER", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
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
        Assert.AreEqual("REAL", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
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
        Assert.AreEqual("PATTERN", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
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
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
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
        Assert.AreEqual("INTEGER", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
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
        Assert.AreEqual("REAL", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
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
        Assert.AreEqual("PATTERN", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreNotEqual(build.Execute!.IdentifierTable[build.FoldCase("a")].SequenceId, build.Execute!.IdentifierTable[build.FoldCase("b")].SequenceId);
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
        Assert.AreEqual("NAME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
    }

#endregion
#region Array -->
#endregion
#region Table -->

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
        Assert.AreEqual("PATTERN", build.Execute!.IdentifierTable[build.FoldCase("r1")].DataType());
        Assert.AreEqual("PATTERN", build.Execute!.IdentifierTable[build.FoldCase("r2")].DataType());
        Assert.AreEqual("PATTERN", build.Execute!.IdentifierTable[build.FoldCase("r3")].DataType());
        Assert.AreEqual("PATTERN", build.Execute!.IdentifierTable[build.FoldCase("r4")].DataType());
        Assert.AreEqual("PATTERN", build.Execute!.IdentifierTable[build.FoldCase("r5")].DataType());
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
        // under -b a name's text is folded, as sbl -b prints for this program: SOMETHING NAME SOMETHING STRING success
        Assert.AreEqual("SOMETHING", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Pointer);
        Assert.AreEqual("NAME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual("SOMETHING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
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
        Assert.AreEqual("NAME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual("#$%", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
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
        Assert.AreEqual("NAME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual(32767, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("INTEGER", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
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
        Assert.AreEqual("NAME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
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
        Assert.AreEqual("NAME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
        Assert.AreEqual(3.14159, ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("REAL", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("f")]).Data);
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
        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreEqual("ABC", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
        }
        else
        {
            Assert.AreEqual("abc", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
        }
        Assert.AreEqual("NAME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("PATTERN", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
        Assert.AreEqual("PATTERN", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("d")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("e")]).Data);
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
#endregion
#region Expression -->
#endregion
#region Code -->

    #endregion

    #region The type argument, and blank strings (expected results are SPITBOL's, sbl -b; Budne's space)

    private static string Result(string body)
    {
        var build = SetupTests.SetupScript("-b", body + "\nend");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        return ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r")]).Data;
    }

#endregion
}
