using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class RTab
{

    [TestMethod]
    public void TEST_RTab_001()
    {
        var s = @"
        &anchor = 0
        subject = 'abcde'
        pattern = rtab(2) . tab1
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","tab1")]).Data);
    }

    [TestMethod]
    public void TEST_RTab_002()
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
    public void TEST_RTab_003()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = rtab(3) len(1) . test
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("C", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_RTab_004()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDE'
        pattern = rtab(5)
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
    public void TEST_RTab_005()
    {
        var s = @"
        pattern = rtab(-1)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(182, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_RTab_006()
    {
        var s = @"
        pattern = rtab(2147483648) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(182, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_RTab_007()
    {
        var s = @"
        pattern = rtab(3.14) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_RTab_008()
    {
        var s = @"
        pattern = rtab('a')
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(181, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_RTab_009()
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