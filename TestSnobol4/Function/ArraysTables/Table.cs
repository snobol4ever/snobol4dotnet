using Snobol4.Common;
using Test.TestLexer;

namespace Test.ArraysTables;

[TestClass]
public class Table
{

    [TestMethod]
    public void TEST_Table_001()
    {
        var s = @"
        a = table()
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Table_002()
    {
        var s = @"
        a = table(,,'a')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }


    [TestMethod]
    public void TEST_Table_003()
    {
        var s = @"
        a = table(,'a','b')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Table_004()
    {
        var s = @"
        a = table(,'a',array(20))
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Table_005a()
    {
        var s = @"
        a = table()
        abc = any('abc')
        def = any('def')
        a[abc] = 'ABC'
        a<def> = 'DEF'
        r1 = a[abc]
        r2 = a[def]
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ABC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("DEF", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Table_005b()
    {
        var s = @"
        a = table()
        abc = any('abc')
        def = any('def')
        a[any('abc')] = 'ABC'
        a<any('def')> = 'DEF'
        r1 = a[any('abc')]
        r2 = a[any('def')]
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Table_005c()
    {
        var s = @"
        a = table()
        a['abc'] = 'ABC'
        a<'def'> = 'DEF'
        r1 = a<'abc'>
        r2 = a['def']
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ABC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("DEF", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }



    [TestMethod]
    public void TEST_Table_006()
    {
        var s = @"
        a = table()
        a['abc','def'] = 'ABC'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(237, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Table_007()
    {
        var s = @"
        a = 'abc'
        b = a['abc']
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(235, build.ErrorCodeHistory[0]);
    }
}