using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Len
{

    [TestMethod]
    public void TEST_Len_001()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = len(3) . test
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("ABC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_Len_002()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = len(2) . test 'A'
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("CD", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_Len_003()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = 'ABCD' len(0) . test 'A'
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_Len_004()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = 'A' len(0) . test 'A'
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Len_005()
    {
        var s = @"
        pattern = len(-1) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(121, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Len_006()
    {
        var s = @"
        pattern = len(2147483648) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(121, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Len_007()
    {
        var s = @"
        pattern = len(3.14) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Len_008()
    {
        var s = @"
        pattern = len('a') 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(120, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Len_009()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = len(*B) . test 'A'
        B = 2
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("CD", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }



}