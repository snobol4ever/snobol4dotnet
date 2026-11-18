using Snobol4.Common;

namespace Test.TestLexer;

/// <summary>
/// The subject of a statement ends at the first blank of its outermost level, but a blank in the right operand of | or &amp;, the
/// operators below concatenation, is a concatenation in that operand: S | 'q' 'z' is the expression S | ('q' 'z') with no pattern,
/// so the statement succeeds, while S 'z' | 'q' matches S against 'z' | 'q' (sbl -bf, S = 'ab': the first succeeds, the second fails).
/// snobol4dotnet matched S | 'q' against 'z', error 241 (rosetta-snobol4 shell-one-liner-1, the line
/// echo 'a output = "Hello, World!";end' | snobol4 -b, graded by its driver's dump).
/// </summary>
[TestClass]
public class SubjectOfALowOperator
{
    private static string Run(string statement) =>
        ((StringVar)SetupTests.SetupScript("-bf", $"        DEFINE('F(A,B)') :(F_E)\nF       F = A :(RETURN)\nF_E     OPSYN('&','F',2)\n        S = 'ab'\n{statement} :S(Y)\n        R = 'f' :(END)\nY       R = 's'\nEND").Execute!.IdentifierTable["R"]).Data;

    [TestMethod] public void TEST_A_blank_after_a_pipe_is_a_concatenation() => Assert.AreEqual("s", Run("        S | 'q' 'z'"));
    [TestMethod] public void TEST_A_blank_after_an_ampersand_is_a_concatenation() => Assert.AreEqual("s", Run("        S & 'q' 'z'"));
    [TestMethod] public void TEST_A_blank_before_a_pipe_ends_the_subject() => Assert.AreEqual("f", Run("        S 'z' | 'q'"));
    // . binds tighter than concatenation: S . V 'b' matches the pattern S . V, error 241 (sbl -bf)
    [TestMethod]
    public void TEST_A_blank_after_a_period_ends_the_subject()
    {
        var build = SetupTests.SetupScript("-bf", "        S = 'ab'\n        S . V 'b'\nEND");
        CollectionAssert.AreEqual(new[] { 241 }, build.ErrorCodeHistory.ToArray());
    }
}
