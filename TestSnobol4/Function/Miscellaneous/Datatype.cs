using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

[TestClass]
public class Datatype
{
    [TestMethod]
    public void TEST_DataType_Array_1()
    {
        var s = @"
        b = datatype(array(20))
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ARRAY", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Array_2()
    {
        var s = @"
        a = array(10)
        b = datatype(a)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ARRAY", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Array_3()
    {
        var s = @"
        a = array(10)
        b = datatype(a<2>)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Code()
    {
        var s = @"
        b = datatype(code(' c = 2'))
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("CODE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Expression()
    {
        var s = @"
        b = datatype(*c)
end";

        var directives = "-b -cs";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("EXPRESSION", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Integer()
    {
        var s = @"
        b = datatype(1)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("INTEGER", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Name()
    {
        var s = @"
        b = datatype(.c)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("NAME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Pattern()
    {
        var s = @"
        b = datatype( arb )
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("PATTERN", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Real()
    {
        var s = @"
        b = datatype( 3.14 )
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("REAL", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_String()
    {
        var s = @"
        b = datatype( 'test' )
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("STRING", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Table_1()
    {
        var s = @"
        b = datatype(table(20))
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("TABLE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Table_2()
    {
        var s = @"
        a = table(10)
        b = datatype(a)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("TABLE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_Table_3()
    {
        var s = @"
        a = table(,,10)
        b = datatype(a[2])
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("INTEGER", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_UserDefinedData()
    {
        var s = @"
     	data('product(name,price,quantity,mfg)')
	    item1 = product('CAPERS', 2.39, 48, 'BRINE BROTHERS')
        b = datatype(item1);
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // Under case folding the type name is folded to upper case, as sbl -b answers
        Assert.AreEqual("PRODUCT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_UserDefinedData_keeps_its_spelling_without_case_folding()
    {
        // sbl -bf answers node and Pair; sbl -b answers NODE and PAIR
        var s = @"
        DATA('node(val,nxt)')
        DATA('Pair(a,b)')
        R = DATATYPE(node(1,2)) ' ' DATATYPE(Pair(1,2)) ' ' DATATYPE(1)
END";
        var build = SetupTests.SetupScript("-b -f", s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("node Pair INTEGER", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}
