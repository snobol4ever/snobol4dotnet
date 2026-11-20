using Snobol4.Common;
using Test.TestLexer;

namespace Test.Compilation;

[TestClass]
public class Eval
{
    [TestMethod]
    public void TEST_Eval001()
    {
        var s = @"
        a = eval(17)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17, i.Data);
    }

    [TestMethod]
    public void TEST_Eval002()
    {
        var s = @"
        a = eval(17 * 2)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(34, i.Data);
    }

    [TestMethod]
    public void TEST_Eval003()
    {
        var s = @"
        a = eval('17')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17, i.Data);
    }

    [TestMethod]
    public void TEST_Eval004()
    {
        var s = @"
        a = eval('17' '34')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(1734, i.Data);
    }

    [TestMethod]
    public void TEST_Eval005()
    {
        var s = @"
        a = eval(17.34)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var r = (RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17.34, r.Data);
    }


    [TestMethod]
    public void TEST_Eval006()
    {
        var s = @"
        a = eval(1.0 / 3.0)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var r = (RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(1.0 / 3.0, r.Data);
    }

    [TestMethod]
    public void TEST_Eval007()
    {
        var s = @"
        a = eval('17.34')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var r = (RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17.34, r.Data);
    }

    [TestMethod]
    public void TEST_Eval008()
    {
        var s = @"
        a = eval('17' '.34')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var r = (RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17.34, r.Data);
    }

    [TestMethod]
    public void TEST_Eval009()
    {
        var s = @"
        a = eval('17 + 34')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(51, i.Data);
    }

    [TestMethod]
    public void TEST_Eval010()
    {
        var s = @"
        n = 15
        a = eval('n * 2')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(30, i.Data);
    }

    [TestMethod]
    public void TEST_Eval011()
    {
        var s = @"
        a = eval('4 ' ' - 8')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(-4, i.Data);
    }

    [TestMethod]
    public void TEST_Eval012()
    {
        var s = @"
        a = eval('2 + eval(3)')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(5, i.Data);
    }

    [TestMethod]
    public void TEST_Eval013()
    {
        var s = @"
        N = 15
        a = eval('3 * N + 2')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(47, i.Data);
    }

    [TestMethod]
    public void TEST_Eval014()
    {
        var s = @"
        e = *('n squared is ' n ^ 2)
        n = 15
        a = eval(e)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var v = (StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual("n squared is 225", v.Data);
    }
}