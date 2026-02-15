using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Literal
{

    [TestMethod]
    public void TEST_Literal_001()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer'
        pattern = 'gram'
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("programmer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Literal_002()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer'
        pattern = 'gram'
        subject pattern = ''      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("promer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Literal_003()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer'
        pattern = 'gram'
        subject pattern =     :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("promer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Literal_004()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer'
        pattern = 'gram'
        subject pattern = 'meter'     :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("prometermer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Literal_005()
    {
        var s = @"
        &anchor = 1
        subject = 'programmer'
        pattern = 'gram'
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("programmer", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Literal_006()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer    '
        pattern = 'gram'
        subject pattern  =    :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("promer    ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Literal_007()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer    '
        pattern = 'gram'
        subject pattern =     :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("promer    ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Literal_008()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer    '
        pattern = ''
        subject pattern =     :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("programmer    ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Literal_009()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer    '
        pattern = *B
        B = 'gram'
        subject pattern =     :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("promer    ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","subject")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }
    
}