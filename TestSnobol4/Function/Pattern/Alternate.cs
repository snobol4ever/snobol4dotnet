using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// An alternative starts where its alternation started, however far the arm before it moved
/// the cursor before failing. Expected results are SPITBOL's (sbl -b).
/// </summary>
[TestClass]
public class Alternate
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
    public void TEST_Alternate_restarts_at_the_alternation_after_span_moved_the_cursor()
    {
        Assert.AreEqual("F", Match("aab", "POS(0) (SPAN('a') 'c' | 'b')"));
    }

    [TestMethod]
    public void TEST_Alternate_restarts_at_the_alternation_after_len_moved_the_cursor()
    {
        Assert.AreEqual("F", Match("aab", "POS(0) (LEN(1) LEN(1) 'c' | LEN(1) 'b')"));
        Assert.AreEqual("S", Match("aab", "POS(0) (LEN(1) LEN(1) 'c' | LEN(2) 'b')"));
    }

    [TestMethod]
    public void TEST_Alternate_an_arbno_in_one_arm_never_continues_into_the_next_arm()
    {
        // Budne's nqueens: the attack test matched with one arm's ARBNO and another arm's tail
        Assert.AreEqual("F", Match("aab", "POS(0) (ARBNO('a') 'c' | ARBNO('x') 'b')"));
        Assert.AreEqual("F", Match("aab", "POS(0) (ARBNO('a') 'c' | 'a' 'b')"));
    }

    [TestMethod]
    public void TEST_Alternate_nested_alternatives_restart_at_their_own_alternation()
    {
        Assert.AreEqual("F", Match("aab", "POS(0) ((SPAN('a') 'q' | 'a' 'q') | 'b')"));
        Assert.AreEqual("S", Match("aab", "POS(0) ((SPAN('a') 'q' | 'aa') 'b')"));
        Assert.AreEqual("S", Match("aab", "POS(0) (SPAN('a') 'c' | 'aab')"));
    }
}
