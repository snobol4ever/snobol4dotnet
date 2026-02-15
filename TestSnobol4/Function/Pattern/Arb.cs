using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Arb
{

    [TestMethod]
    public void TEST_Arb_001()
    {
        var s = @"
        &anchor = 0
        subject = 'programmer'
        pattern = 'p' arb . test 'er'
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("rogramm", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_Arb_002()
    {
        var s = @"
        &anchor = 0
        subject = 'MOUNTAIN'
        pattern = 'O' arb . test 'A'
        subject pattern      :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("UNT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","test")]).Data);
    }

    [TestMethod]
    public void TEST_Arb_003()
    {
        var s = @"
        &anchor = 0
        subject = 'MOUNTAIN'
        pattern = 'O' arb . test 'U'
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
    public void TEST_Arb_004()
    {
        var s = @"
		CATANDDOG = 'CAT' ARB 'DOG' | 'DOG' ARB 'CAT'
		'CATALOG FOR SEADOGS' CATANDDOG $ R1
		'DOGS HATE POLECATS' CATANDDOG $ R2
		'CATDOG' CATANDDOG $ R3
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("CATALOG FOR SEADOG", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("DOGS HATE POLECAT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
        Assert.AreEqual("CATDOG", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r3")]).Data);
    }

    [TestMethod]
    public void TEST_Arb_005()
    {
        var s = @"
    	'MOUNTAIN' 'O' (ARB $ R1) 'A'
	    'MOUNTAIN' 'O' (ARB $ R2) 'U'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("UNT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Arb_006()
    {
        var s = @"
	    'AXXXBXXX' ('A' arb $ r1 'B' arb $ r2 'C') :f(n)
	    r3 = 'success' :(end)
n	    r3 = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("XXXBXXX", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("XXX", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r3")]).Data);
    }

}