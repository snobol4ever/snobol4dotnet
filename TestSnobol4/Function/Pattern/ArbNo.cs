using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

[TestClass]
public class ArbNo
{

    [TestMethod]
    public void TEST_ArbNo_001()
    {
        var s = @"
        &anchor = 0
        item = span('0123456789')
        pattern = pos(0) '(' item arbno(',' item) ')' rpos(0)
        subject = '(12,34,56)'
        subject pattern :f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_ArbNo_002()
    {
        var s = @"
        &anchor = 0
        item = span('0123456789')
        pattern = pos(0) '(' item arbno(',' item) ')' rpos(0)
        subject = '(12,,56)'
        subject pattern :f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

}