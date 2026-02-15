using Snobol4.Common;
using Test.TestLexer;

namespace Test.ArraysTables;

[TestClass]
public class Item
{

    [TestMethod]
    public void TEST_TableItem_005()
    {
        var s = @"
        a = table()
        a['abc'] = 'ABC'
        a<'def'> = 'DEF'
        r1 = item(a,'abc')
        r2 = item(a,'def')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ABC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("DEF", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_TableItem_005a()
    {
        var s = @"
        a = table()
        item(a,'abc') = 'ABC'
        item(a,'def') = 'DEF'
        r1 = item(a,'abc')
        r2 = item(a,'def')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ABC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("DEF", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_TableItem_006()
    {
        var s = @"
        a = table()
        item(a,'abc','def') = 'ABC'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(237, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_TableItem_007()
    {
        var s = @"
        a = 'abc'
        b = item(a,'abc')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(235, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_010()
    {
        var s = @"
        a = 'abc'
        b = item(a,3)
end
";
        var directives = "-b -cs";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(235, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_011()
    {
        var s = @"
        a = array(20)
        b = item(a, 'abc')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(238, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_012()
    {
        var s = @"
        a = array('20,10,5')
        b = item(a,30,1)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(236, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_013()
    {
        var s = @"
        a = array(0)
        b = item(a,30)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_014()
    {
        var s = @"
        a = array(10)
        b = item(a,-1)       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_015()
    {
        var s = @"
        a = array(0)
        b = item(a,3,3)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_016()
    {
        var s = @"
        a = array('3,3')
        b = item(a,1)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(236, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_021()
    {
        var s = @"
        a = array('20,10,5')
        b = item(a,6,1,5,2)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(236, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_022()
    {
        var s = @"
        a = array(10)
        b = item(a,0)       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_023()
    {
        var s = @"
        a = array(10)
        b = item(a,1)       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_024()
    {
        var s = @"
        a = array(10)
        b = item(a,10)       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_025()
    {
        var s = @"
        a = array(10)
        b = item(a,11)       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_026()
    {
        var s = @"
        a = array(10,999)
        b = item(a,3)
        eq(b,999) :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

}