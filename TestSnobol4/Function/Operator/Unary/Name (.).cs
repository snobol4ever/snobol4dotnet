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
        Assert.AreEqual("kharma", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","dog")]).Data);
        Assert.AreEqual("DOG", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Pointer);
        Assert.AreEqual("kharma", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
        Assert.AreEqual("KHARMA", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Pointer);
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
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Name_005()
    {
        var s = @"
        a = .array('10')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
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
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","x")]).Data);
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","y")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","z")]).Data);
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
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","x")]).Data);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","y")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","z")]).Data);
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
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","x")]).Data);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","y")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","z")]).Data);
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
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","x")]).Data);
        Assert.AreEqual("something", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","y")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","z")]).Data);
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
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","x")]).Data);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","y")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","z")]).Data);
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
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","x")]).Data);
        Assert.AreEqual("else", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","y")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","z")]).Data);
    }
}
