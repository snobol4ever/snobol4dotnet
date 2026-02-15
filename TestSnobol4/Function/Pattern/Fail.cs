using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

[TestClass]
public class Fail
{

    [TestMethod]
    public void TEST_Fail_001()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer'
        pattern = fail
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

}