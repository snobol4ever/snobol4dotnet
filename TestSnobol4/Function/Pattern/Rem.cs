using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Rem
{

    [TestMethod]
    public void TEST_Rem_001()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer'
        pattern = 'gra' rem . test
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("mmer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_Rem_002()
    {
        var s = @"
        &anchor = 0
        subject = 'THE WINTER WINDS'
        pattern = 'WIN' rem . test
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("TER WINDS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_Rem_003()
    {
        var s = @"
        &anchor = 0
        subject = 'THE WINTER WINDS'
        pattern = 'WINDS' rem . test
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

}