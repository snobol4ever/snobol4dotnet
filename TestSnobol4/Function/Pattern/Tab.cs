using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Tab
{

    [TestMethod]
    public void TEST_Tab_001()
    {
        var s = @"
        &anchor = 0
        subject = 'abcde'
        pattern = tab(2) . tab1
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("ab", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","tab1")]).Data);
    }

    [TestMethod]
    public void TEST_Tab_002()
    {
        var s = @"
        &anchor = 0
        subject = ''
        pattern = len(3) . test tab(3)
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Tab_003()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = tab(3) len(1) . test
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("D", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_Pos_004()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDE'
        pattern = tab(0)
        subject pattern = '****'     :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("****ABCDE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
    }

    [TestMethod]
    public void TEST_Tab_005()
    {
        var s = @"
        pattern = tab(-1)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(184, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Tab_006()
    {
        var s = @"
        pattern = tab(2147483648) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(184, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Tab_007()
    {
        var s = @"
        pattern = tab(3.14) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Tab_008()
    {
        var s = @"
        pattern = tab('a')
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(183, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Tab_009()
    {
        var s = @"
        &anchor = 0
        subject = ''
        pattern = len(*B) . test tab(*B)
        B = 3
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

}