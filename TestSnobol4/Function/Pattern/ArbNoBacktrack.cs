using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// Backtracking reaches into every ARBNO repetition's alternatives, as in SPITBOL: a later
/// failure retries an earlier repetition with its argument's next alternative. Expected
/// results are SPITBOL's (sbl -bf) and the rungs entries' refs.
/// </summary>
[TestClass]
public class ArbNoBacktrack
{
    private static string Match(string subject, string pattern)
    {
        var s = $@"
        subj = '{subject}'
        subj ? {pattern}   :f(n)
        result = 'S'       :(end)
n       result = 'F'
end";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        return ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data;
    }

    [TestMethod]
    [Timeout(10000)]
    public void TEST_ArbNo_retries_a_repetition_with_a_shorter_alternative()
    {
        // arbno_pos_rpos_branch_12: 'AB' leaves 'BB' unmatched, so the repetition is retried as 'A'
        Assert.AreEqual("S", Match("ABB", "POS(0) ARBNO('ABC' | 'AB' | 'A') 'BB' RPOS(0)"));
    }

    [TestMethod]
    [Timeout(10000)]
    public void TEST_ArbNo_retries_an_alternative_two_deep()
    {
        // arbno_pos_rpos_branch_13
        Assert.AreEqual("S", Match("ABCBB", "POS(0) ARBNO('ABC' | 'AB' | 'A') 'CBB' RPOS(0)"));
    }

    [TestMethod]
    [Timeout(10000)]
    public void TEST_ArbNo_retries_a_nested_alternation()
    {
        // arbno_pos_rpos_branch_3: 'a' first, then 'ab' when RPOS(0) fails
        Assert.AreEqual("S", Match("ab", "POS(0) ARBNO(('a' | 'ab') | 'c') RPOS(0)"));
    }

    [TestMethod]
    [Timeout(10000)]
    public void TEST_ArbNo_still_fails_when_no_repetition_count_fits()
    {
        Assert.AreEqual("F", Match("ABX", "POS(0) ARBNO('AB' | 'A') 'B' RPOS(0)"));
    }
}
