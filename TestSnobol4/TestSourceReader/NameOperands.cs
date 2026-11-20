using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// The operand of a name operator (unary . @ &, the right of binary . and $) and the target of an = inside parens or the arguments of a
/// call must be a name: a negation, an interrogation, a concatenation or an alternation there is error 212, "value used where name is
/// required" (inside parens a concatenation is SPITBOL's c_cnp, which is no pattern match; the statement's own = takes one as subject and
/// pattern). Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NameOperands
{

    // the = of a chained assignment takes a concatenation as the subject and pattern of its own match: compiled, sbl accepts these
    [TestMethod]
    public void TEST_NameOperands_a_concatenation_before_an_equal_at_the_statements_level_compiles()
    {
        foreach (var statement in new[] { "A = X 'b' = 2", "A = X Y = 2", "B = A = X 'b' = 2", "A = X Y Z = 2" })
            Assert.AreEqual(0, SetupTests.SetupScript("-b -n", $"        {statement}\nEND").ErrorCodeHistory.Count, statement);
    }

    // a pattern match gives a value, so it is no name under unary . @ &, at the right of binary . and $, or as a goto: each statement
    // is 212 as it is compiled, as sbl -b lists them (found by the infinite_snobol4 generator, where dotnet ran them)
    [TestMethod]
    public void TEST_NameOperands_a_match_is_no_name()
    {
        var build = SetupTests.SetupScript("-b -n", @"        Y = .(X ? 'a')
        Y = @(X ? 'a')
        Y = &(X ? 'a')
        Y = A . (X ? 'a')
        Y = A $ (X ? 'a')
        Y = 1     :(X ? 'L')
L
END");
        CollectionAssert.AreEqual(new[] { 212, 212, 212, 212, 212, 212 }, build.ErrorCodeHistory.ToArray());
    }

    // an alternation is no subject unless it is over a concatenation
    [TestMethod]
    public void TEST_NameOperands_an_alternation_that_is_no_pattern_match_as_a_target_is_212()
    {
        var build = SetupTests.SetupScript("-b -n", "        A | B C = 2\nEND");
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }
}
