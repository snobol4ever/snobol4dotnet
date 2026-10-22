using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A pattern variable that holds a deferred reference, G0 = *G1, may itself be starred, P = *G0, to any depth: matching *P
/// evaluates P, and while the value is an expression evaluates that, until the value is a pattern or a string, as in SPITBOL
/// (sbl -bf; the rungs entries pos_defer_branch_2, span_pos_rpos_branch_7 and arbno_pos_rpos_branch_84 print match).
/// </summary>
[DoNotParallelize]
[TestClass]
public class DeferredReference
{
    private static string Run(string script) =>
        SetupTests.RunWithInputDirectives("-bf", script, null).Replace("\r\n", "\n");

    [TestMethod]
    public void TEST_DeferredReference_a_string_through_two_references()
    {
        // sbl -bf: match
        Assert.AreEqual("match", Run("""
                 G1 = 'aa'; G0 = *G1; P = *G0
                 'aa a' POS(0) *P               :S(OK)F(NO)
         OK      OUTPUT = 'match'               :(END)
         NO      OUTPUT = 'nomatch'
         END
         """));
    }

    [TestMethod]
    public void TEST_DeferredReference_span_through_three_references()
    {
        // sbl -bf: match
        Assert.AreEqual("match", Run("""
                 digits = '0123456789'; P1 = SPAN(digits); P2 = *P1; P3 = *P2; S = 'A1'
                 S POS(0) 'A' *P3 RPOS(0)       :F(NO)
                 OUTPUT = 'match'               :(END)
         NO      OUTPUT = 'nomatch'
         END
         """));
    }

    [TestMethod]
    public void TEST_DeferredReference_arbno_through_three_references()
    {
        // sbl -bf: match
        Assert.AreEqual("match", Run("""
                 G2 = ARBNO(LEN(1) | LEN(2) | LEN(3)); G1 = *G2; G0 = *G1; P = *G0
                 'abcdef' POS(0) *P RPOS(0)     :S(OK)F(NO)
         OK      OUTPUT = 'match'               :(END)
         NO      OUTPUT = 'nomatch'
         END
         """));
    }

    [TestMethod]
    public void TEST_DeferredReference_a_reference_to_itself_ends_in_stack_overflow()
    {
        // sbl -bf loops on X = *X without end; here the chain is cut at the left-recursion limit, which is
        // SPITBOL's stack overflow (error 246), so a program that can never finish ends instead of hanging
        var build = SetupTests.SetupScript("-bf", """
                 X = *X
                 'a' POS(0) *X
         END
         """);
        Assert.IsTrue(build.ErrorCodeHistory.Contains(246));
    }
}
