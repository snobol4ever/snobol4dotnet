using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

[TestClass]
public class Fence
{

    [TestMethod]
    public void TEST_Abort_001()
    {
        var s = @"
        P = FENCE(BREAK(',') | REM) $ STR *DIFFER(STR)
        'ABC' P . R1
        '123,,456' P . R2
END
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("ABC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("123", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }
    
    [TestMethod]
    public void TEST_Abort_002()
    {
        var s = @"
	    '1AB+' ANY('AB') FENCE('+') :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(END)
N	    R1 = 'FAILURE'
END
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("FAILURE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
    }

    [TestMethod]
    public void TEST_Abort_003()
    {
        var s = @"
        '1AB+' ANY('AB') FENCE '+' :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(END)
N	    R1 = 'FAILURE'
END
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("FAILURE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
    }

    [TestMethod]
    public void TEST_Abort_004()
    {
        var s = @"
	    'ABC'  FENCE('B') :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(END)
N	    R1 = 'FAILURE'
END
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("FAILURE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
    }

    [TestMethod]
    public void TEST_Abort_005()
    {
        var s = @"
	    'ABC'  FENCE 'B' :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(END)
N	    R1 = 'FAILURE'
END
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("FAILURE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
    }

    [TestMethod]
    public void TEST_Abort_006()
    {
        var s = @"
        B = *'B'
	    'ABC' FENCE(B) :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(END)
N	    R1 = 'FAILURE'
END
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("FAILURE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
    }


}