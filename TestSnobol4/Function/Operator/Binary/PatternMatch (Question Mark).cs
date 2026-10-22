using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// S ? P used as a value is the substring P matched (SPITBOL manual, Binary operator extensions); only as the left operand
/// of = is it the subject to replace into.  Expected results are SPITBOL's (sbl -b).
/// </summary>
[TestClass]
public class PatternMatchValue
{
    private static string Get(Builder build, string name) =>
        ((StringVar)build.Execute!.IdentifierTable[build.FoldCase(name)]).Data;

    // (was V = ABCDEF, D = subject, C = <abc>, N = 4, L = abc, E error 1, K null, I failed)
    [TestMethod]
    public void TEST_PatternMatch_a_match_used_as_a_value_is_the_matched_substring()
    {
        var build = SetupTests.SetupScript("-b", @"
        V = ('ABCDEF' ? LEN(3))
        D = DATATYPE(V)
        C = '<' ('abc' ? 'b') '>'
        N = SIZE('abcd' ? 'bc')
        L = 'abc' ? 'b' ? 'b'
        E = ('12345' ? LEN(2)) + 1
        T = TABLE()
        T<'abc' ? 'b'> = 'x'
        K = T<'b'>
        I = IDENT('b', 'abc' ? 'b') 'ident'
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ABC", Get(build, "V"));
        Assert.AreEqual("STRING", Get(build, "D"));
        Assert.AreEqual("<b>", Get(build, "C"));
        Assert.AreEqual(2L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("N")]).Data);
        Assert.AreEqual("b", Get(build, "L"));
        Assert.AreEqual(13L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("E")]).Data);
        Assert.AreEqual("x", Get(build, "K"));
        Assert.AreEqual("ident", Get(build, "I"));
    }

    // A variable or element given a match holds a string, so a later assignment to it assigns, where it replaced into the
    // old match (was R = a<abc>c, W = xwz); a replacement still replaces
    [TestMethod]
    public void TEST_PatternMatch_a_stored_match_is_assigned_over_not_replaced_into()
    {
        var build = SetupTests.SetupScript("-b", @"
        R = ('abc' ? 'b')
        R = '<' ('abc' ? 'b') '>'
        A = ARRAY(1)
        A<1> = 'xyz' ? 'y'
        A<1> = 'w'
        W = A<1>
        S = 'hello'
        X = (S ? 'l' = 'L')
        S2 = 'abc'
        S2 'b' = 'Q'
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("<b>", Get(build, "R"));
        Assert.AreEqual("w", Get(build, "W"));
        Assert.AreEqual("heLlo", Get(build, "X"));
        Assert.AreEqual("heLlo", Get(build, "S"));
        Assert.AreEqual("aQc", Get(build, "S2"));
    }
}
