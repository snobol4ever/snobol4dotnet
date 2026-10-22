using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

[TestClass]
public class Name
{
    [TestMethod]
    public void TEST_Name_001()
    {
        var s = @"
        dog = 'kharma'
        dog
        a = .dog
        datatype(a)
        b = $.dog
        c = .$dog
        
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("kharma", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("dog")]).Data);

        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreEqual("DOG", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Pointer);
            Assert.AreEqual("KHARMA", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Pointer);

        }
        else
        {
            Assert.AreEqual("dog", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Pointer);
            Assert.AreEqual("kharma", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Pointer);

        }
        Assert.AreEqual("kharma", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_Name_002()
    {
        var s = @"
        a = .'123'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Name_003()
    {
        var s = @"
        a = .123
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Name_004()
    {
        var s = @"
        a = .table(0)
end";
        // sbl -b answers this program with ERROR 021, function called by name returned a value: the name of a call
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(21, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Name_005()
    {
        var s = @"
        a = .array('10')
end";
        // sbl -b answers this program with ERROR 021, function called by name returned a value: the name of a call
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(21, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Name_006()
    {
        var s = @"
		a = table()
		b = 'something'
		n = .b
		x = b
		y = $n
		z = datatype(b)        
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data);
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("z")]).Data);
    }



    [TestMethod]
    public void TEST_Name_007()
    {
        var s = @"
		a = table()
		b = 'something'
		n = .b
		x = b
		y = $n
		z = datatype(b)    

		b = 'else'
		x = b
		y = $n
		z = datatype(b)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("z")]).Data);
    }

    [TestMethod]
    public void TEST_Name_008()
    {
        var s = @"
		a = table()
		b = 'something'
		n = .b
		x = b
		y = $n
		z = datatype(b)  

		$n = 'else'
		x = b
		y = $n
		z = datatype(b)

end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("z")]).Data);
    }


    [TestMethod]
    public void TEST_Name_009()
    {
        var s = @"
        a = table()
		a[2] = 'something'
		n = .a[2]
		x = a[2]
		y = $n
		z = datatype(a[2])

end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data);
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("z")]).Data);
    }

    [TestMethod]
    public void TEST_Name_010()
    {
        var s = @"
        a = table()
		a[2] = 'something'
		n = .a[2]
		x = a[2]
		y = $n
		z = datatype(a[2])

		$n = 'else'
		a[2] = 'else'
		x = a[2]
		y = $n
		z = datatype(a[2])
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("z")]).Data);
    }

    [TestMethod]
    public void TEST_Name_011()
    {
        var s = @"
        a = table()
		a[2] = 'something'
		n = .a[2]
		x = a[2]
		y = $n
		z = datatype(a[2])

		$n = 'else'
		x = a[2]
		y = $n
		z = datatype(a[2])
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("z")]).Data);
    }

    [TestMethod]
    public void TEST_Name_of_an_element_written_from_a_function_value()
    {
        // sbl -b answers new: B[2] holds the value SW assigned, and .B[2] is still the element's name,
        // not SW's (Gimpel's HSORT swapped through such names and came out unsorted)
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('SW(A2)')               :(SW_END)
SW      SW = 'v'
        $A2 = SW
        SW =                           :(RETURN)
SW_END
        B = ARRAY(2)
        SW(.B[2])
        N = .B[2]
        $N = 'new'
        R = B[2]
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("new", build.Execute!.IdentifierTable["R"].ToString());
    }

    [TestMethod]
    public void TEST_Name_of_a_call_that_returned_a_value_is_error_21()
    {
        // sbl -bf: the name of a call that handed back a value, a RETURN's or a builtin's, is error 21 (was a NAME of G for .G(), and
        // error 212 for .SIZE('abc')); the name of an NRETURN and of ITEM() are names
        var build = SetupTests.SetupScript("-bf", @"
        &ERRLIMIT = 10
        DEFINE('F()')
        DEFINE('G()')                             :(FE)
F       F = .ZZ                                   :(NRETURN)
G       G = 'val'                                 :(RETURN)
FE      N = .F()
        R = DATATYPE(N)
        M = .G()                                  :S(Y3)
        R = R ' ' &ERRTYPE
Y3      &ERRTYPE = 0
        K = .SIZE('abc')                          :S(Y4)
        R = R ' ' &ERRTYPE
Y4      A = ARRAY(2)
        P = .ITEM(A, 1)
        R = R ' ' DATATYPE(P)
END");
        Assert.AreEqual("NAME 21 21 NAME", build.Execute!.IdentifierTable["R"].ToString());
    }
}
