using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Pos
{

    [TestMethod]
    public void TEST_Pos_001()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = pos(0) . test 'b'
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_Pos_002()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = len(3) . test pos(3)
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
    public void TEST_Pos_003()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = pos(3) len(1) . test
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
        subject = 'ABCDA'
        pattern = pos(0) 'ABCD' rpos(0)
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Pos_005()
    {
        var s = @"
        pattern = pos(-1) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(163, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Pos_006()
    {
        var s = @"
        pattern = pos(2147483648) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(163, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Pos_007()
    {
        var s = @"
        pattern = pos(3.14) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Pos_008()
    {
        var s = @"
        pattern = pos('a') 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(162, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Pos_009()
    {
        var s = @"
        &anchor = 0
        subject = 'ABCDA'
        pattern = pos(*A) len(*B) . test
        A = 3
        B = 1
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("D", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

}