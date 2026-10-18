using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// ARBNO whose argument can match the null string. Each repetition after the zeroth must
/// advance the cursor, as in SPITBOL; a repetition that matches null would repeat forever.
/// Expected results are SPITBOL's (sbl -bf).
/// </summary>
[TestClass]
public class ArbNoNullIteration
{
    private static string Match(string pattern)
    {
        var s = $@"
        subj = 'abc'
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
    public void TEST_ArbNo_nested_arbno_matches()
    {
        Assert.AreEqual("S", Match("POS(0) ARBNO(ARBNO(LEN(1))) RPOS(0)"));
    }

    [TestMethod]
    [Timeout(10000)]
    public void TEST_ArbNo_of_null_matches_only_null()
    {
        Assert.AreEqual("F", Match("POS(0) ARBNO('') RPOS(0)"));
    }

    [TestMethod]
    [Timeout(10000)]
    public void TEST_ArbNo_of_arb_matches()
    {
        Assert.AreEqual("S", Match("POS(0) ARBNO(ARB) RPOS(0)"));
    }
}
