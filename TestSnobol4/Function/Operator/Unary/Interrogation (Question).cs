using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

[TestClass]
public class Interrogation
{
    [TestMethod]
    public void TEST_Interrogation_001()
    {
        var s = @"
        S = 'this'
        P = 'is'
        N = 123
        N = ?(S ? P) N + 1
END";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(124, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("n")]).Data);
    }

    [TestMethod]
    public void TEST_Interrogation_002()
    {
        var s = @"
	    int = ?integer('a')   :f(n)
	    results = 'succeed'  :(end)
n	    results = 'failure'
end";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("results")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("int")]).Data);
    }

    [TestMethod]
    public void TEST_Interrogation_00e()
    {
        var s = @"
	    int = ?integer('42')   :f(n)
	    results = 'succeed'  :(end)
n	    results = 'failure'
end";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("succeed", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("results")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("int")]).Data);
    }
}