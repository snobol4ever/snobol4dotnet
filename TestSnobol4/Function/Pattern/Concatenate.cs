using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Concatenate
{

    [TestMethod]
    public void TEST_Concatenate_001()
    {
        var s = @"
        &anchor = 0
        ss = 'PLEASEHELPMEOUT'
        p1 = 'PLEASE'
        p2 = 'HELP'
        p3 = 'ME'
        p4 = 'OUT'
        p = p2 p3
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("HELPME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","p")]).Data);
    }

    [TestMethod]
    public void TEST_Concatenate_002()
    {
        var s = @"
        &anchor = 0
        ss = 'PLEASEHELPMEOUT'
        p1 = 'PLEASE'
        p2 = 'HELP'
        p3 = 'ME'
        p4 = 'OUT'
        p = p2 p3
        ss p		:f(n)
        r = 'success' :(end)
n		r = 'fail'  :(end)

end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("HELPME", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","p")]).Data);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

}