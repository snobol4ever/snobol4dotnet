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
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("ABC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("123", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }
    
    [TestMethod]
    public void TEST_Abort_002()
    {
        var s = @"
	    '1AB+' ANY('AB') FENCE('+') :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(end)
N	    R1 = 'FAILURE'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("SUCCESS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
    }

    [TestMethod]
    public void TEST_Abort_003()
    {
        var s = @"
        '1AB+' ANY('AB') FENCE '+' :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(end)
N	    R1 = 'FAILURE'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("FAILURE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
    }

    [TestMethod]
    public void TEST_Abort_004()
    {
        var s = @"
	    'ABC'  FENCE('B') :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(end)
N	    R1 = 'FAILURE'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("SUCCESS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
    }

    [TestMethod]
    public void TEST_Abort_005()
    {
        var s = @"
	    'ABC'  FENCE 'B' :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(end)
N	    R1 = 'FAILURE'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("FAILURE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
    }

    [TestMethod]
    public void TEST_Abort_006()
    {
        var s = @"
        B = *'B'
	    'ABC' FENCE(B) :S(Y)F(N)
Y	    R1 = 'SUCCESS' :(end)
N	    R1 = 'FAILURE'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("SUCCESS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
    }

    [TestMethod]
    public void TEST_Fence_007_nullary_blocks_backtrack()
    {
        // Nullary FENCE: once position passes FENCE, no backtrack past it
        var s = @"
        &anchor = 0
        subject = 'abc'
        subject ('a' fence 'x')   :s(bad)f(ok)
bad     result = 'bad'   :(end)
ok      result = 'ok'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Fence_008_unary_inner_match()
    {
        // Unary FENCE(p): if p matches, commit — no backtrack into p
        var s = @"
        &anchor = 0
        subject = 'abc'
        subject fence('abc') . cap   :s(ok)f(bad)
ok      result = cap   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Fence_function_backup_passes_through_and_the_scan_goes_on()
    {
        // sbl -b: match. Backing up into FENCE('a') at the start 0 does not abort the match, as &FENCE does (SPITBOL manual,
        // FENCE(pattern)); the scan tries the next start and matches the last 'a' of the subject.
        var s = @"
        P = (BREAKX('abc') | FENCE('a'))
        'aa a' P RPOS(0) :S(Y)F(N)
Y       R1 = 'match' :(end)
N       R1 = 'nomatch'
end
";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual("match", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);

        // sbl -b: the same shape on 'AB' matches unanchored, at the second character, and fails under &ANCHOR = 1
        var unanchored = SetupTests.SetupScript("-b", @"
        'AB' FENCE(LEN(1) | LEN(2)) RPOS(0) :S(Y)F(N)
Y       R1 = 'match' :(end)
N       R1 = 'nomatch'
end
");
        Assert.AreEqual("match", ((StringVar)unanchored.Execute!.IdentifierTable[unanchored.FoldCase("R1")]).Data);
        var anchored = SetupTests.SetupScript("-b", @"
        &ANCHOR = 1
        'AB' FENCE(LEN(1) | LEN(2)) RPOS(0) :S(Y)F(N)
Y       R1 = 'match' :(end)
N       R1 = 'nomatch'
end
");
        Assert.AreEqual("nomatch", ((StringVar)anchored.Execute!.IdentifierTable[anchored.FoldCase("R1")]).Data);
    }

}
