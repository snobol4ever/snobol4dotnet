using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

/// <summary>
/// FENCE crosscheck tests 058–067 from corpus/crosscheck/patterns/.
/// Tests 061 (seal), 064 (capture), 065 (decimal) are known SPITBOL bugs
/// and may also fail here; marked with [Ignore] if needed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CorpusRef_FenceTests
{
    [TestMethod]
    public void TEST_Fence_058_pat_fence_keyword()
    {
        // 058 - FENCE keyword (0-arg): matches null, aborts match on backtrack
        var s = @"*  058 - FENCE keyword (0-arg): matches null, aborts match on backtrack
        X = 'AB'
        X  LEN(1)  FENCE  LEN(2)                              :S(YES)F(NO)
YES     OUTPUT = 'should not reach'                            :(END)
NO      OUTPUT = 'aborted correctly'
END
";
        Assert.AreEqual(@"aborted correctly", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Fence_059_pat_fence_fn_basic()
    {
        // 059 - FENCE(P) basic: P matches, overall match succeeds
        var s = @"*  059 - FENCE(P) basic: P matches, overall match succeeds
        X = 'hello'
        X  FENCE('hello')                                      :F(NO)
        OUTPUT = 'matched'                                     :(END)
NO      OUTPUT = 'failed'
END
";
        Assert.AreEqual(@"matched", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Fence_060_pat_fence_fn_fail()
    {
        // 060 - FENCE(P) failure: P fails, statement takes F branch (no abort)
        var s = @"*  060 - FENCE(P) failure: P fails, statement takes F branch (no abort)
        X = 'hello'
        X  FENCE('world')                                      :S(YES)F(NO)
YES     OUTPUT = 'should not reach'                            :(END)
NO      OUTPUT = 'failed gracefully'
END
";
        Assert.AreEqual(@"failed gracefully", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Fence_061_pat_fence_fn_seal()
    {
        // 061 - FENCE(P) seals: backtrack cannot re-enter P's alternatives
        var s = @"*  061 - FENCE(P) seals: backtrack cannot re-enter P's alternatives
*  Without fence: FENCE(LEN(1)|LEN(2)) RPOS(0) on 'AB' would succeed
*  via LEN(2). With fence: LEN(1) chosen, sealed, RPOS(0) fails -> match fails.
*  Anchored by POS(0) as the corpus program is (rungs entry fence_pos_rpos_branch_24): unanchored, sbl
*  goes on to the next start, where FENCE(LEN(1)) takes the last character and RPOS(0) holds.
        X = 'AB'
        X  POS(0) FENCE(LEN(1) | LEN(2))  RPOS(0)             :S(YES)F(NO)
YES     OUTPUT = 'should not reach'                            :(END)
NO      OUTPUT = 'sealed correctly'
END
";
        Assert.AreEqual(@"sealed correctly", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Fence_062_pat_fence_fn_outer()
    {
        // 062 - FENCE(P) outer alts: enclosing pattern can still backtrack outside fence
        var s = @"*  062 - FENCE(P) outer alts: enclosing pattern can still backtrack outside fence
        X = 'bx'
        X  ('a' | 'b')  FENCE('x')                            :F(NO)
        OUTPUT = 'outer alt worked'                            :(END)
NO      OUTPUT = 'failed'
END
";
        Assert.AreEqual(@"outer alt worked", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Fence_063_pat_fence_fn_optional()
    {
        // 063 - FENCE(P) optional suffix: FENCE(SPAN(digits)|'') — greedy or empty, committed
        var s = @"*  063 - FENCE(P) optional suffix: FENCE(SPAN(digits)|'') — greedy or empty, committed
        digits = '0123456789'
        &ANCHOR = 1
        X = '123abc'
        X  FENCE(SPAN(digits) | '') . N
        OUTPUT = N
END
";
        Assert.AreEqual(@"123", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Fence_064_pat_fence_fn_capture()
    {
        // 064 - FENCE(P) with $ capture: identifier pattern from corpus
        var s = @"*  064 - FENCE(P) with $ capture: identifier pattern from corpus
        alnum = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_'
        &ANCHOR = 1
        X = 'hello_world rest'
        X  ANY(&UCASE &LCASE)  FENCE(SPAN(alnum) | '') . ID
        OUTPUT = ID
END
";
        Assert.AreEqual(@"ello_world", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Fence_065_pat_fence_fn_decimal()
    {
        // 065 - FENCE(P) decimal literal pattern: ANY(1-9) FENCE(SPAN(digits)|'')
        var s = @"*  065 - FENCE(P) decimal literal pattern: ANY(1-9) FENCE(SPAN(digits)|'')
        digits = '0123456789'
        &ANCHOR = 1
        X = '42rest'
        X  ANY('123456789')  FENCE(SPAN(digits) | '') . N
        OUTPUT = N
END
";
        Assert.AreEqual(@"2", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Fence_066_pat_fence_fn_nested()
    {
        // 066 - FENCE(P) nested: real number pattern with inner FENCE
        var s = @"*  066 - FENCE(P) nested: real number pattern with inner FENCE
        digits = '0123456789'
        &ANCHOR = 1
        Real = SPAN(digits) '.' FENCE(SPAN(digits) | '')
+          |   SPAN(digits)
        X = '3.14rest'
        X  FENCE(Real) . N
        OUTPUT = N
END
";
        Assert.AreEqual(@"3.14", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Fence_067_pat_fence_fn_vs_kw()
    {
        // 067 - FENCE(P) vs &FENCE: FENCE(P) fails gracefully, &FENCE aborts entire match
        var s = @"*  067 - FENCE(P) vs &FENCE: FENCE(P) fails gracefully, &FENCE aborts entire match
*  &FENCE: backtrack into it kills the whole statement (goes to F, not S)
        X = 'AX'
        X  LEN(1)  FENCE  LEN(2)                              :S(BAD)
*  FENCE(P): when P fails, statement takes F branch normally
        X  FENCE('Z')                                         :S(BAD)
        OUTPUT = 'both correct'                               :(END)
BAD     OUTPUT = 'wrong'
END
";
        Assert.AreEqual(@"both correct", SetupTests.RunWithInput(s));
    }

}