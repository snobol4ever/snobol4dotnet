using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

[TestClass]
public class Negation
{
    [TestMethod]
    public void TEST_Negation_001()
    {
        var s = @"
	    ~integer('a')   :f(n)
	    result = 'succeed' :(end)
n	    result = 'failure'
end
";
        var directives = "-b -F";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("succeed", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Negation_002()
    {
        var s = @"
	    ~integer('3')   :f(n)
	    result = 'succeed' :(end)
n	    result = 'failure'
end
";
        var directives = "-b -F";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }


    [TestMethod]
    public void TEST_Negation_003()
    {
        var s = @"
	    ~integer(5)   :f(n)
	    result = 'succeed' :(end)
n	    result = 'failure'
end
";
        var directives = "-b -F";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }





}