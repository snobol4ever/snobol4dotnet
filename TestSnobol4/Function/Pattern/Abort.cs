using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

[TestClass]
public class Abort
{

    [TestMethod]
    public void TEST_Abort_001()
    {
        var s = @"
        &anchor = 0
        subject = '-ab-1-'
        pattern = any('ab') | '1' abort
        subject pattern :f(n)
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Abort_002()
    {
        var s = @"
        &anchor = 0
        subject = '-1a-b-'
        pattern = any('ab') | '1' abort
        subject pattern :f(n)
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

}