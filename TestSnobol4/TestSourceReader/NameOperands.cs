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
    [TestMethod]
    public void TEST_NameOperands_the_right_of_a_dot_is_a_negation()
    {
        var program = @"        X = A . ~B
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A . ~B

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_right_of_a_dot_is_an_interrogation()
    {
        var program = @"        X = A . ?B
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A . ?B

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_right_of_a_dollar_is_a_negation()
    {
        var program = @"        X = A $ ~B
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A $ ~B

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_right_of_a_dot_is_a_negated_call()
    {
        var program = @"        X = A . ~LGT('abc', null)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A . ~LGT('abc', null)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_right_of_a_dot_is_a_parenthesized_concatenation()
    {
        var program = @"        X = A . (B C)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A . (B C)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_right_of_a_dollar_is_a_parenthesized_concatenation()
    {
        var program = @"        X = A $ (B C)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A $ (B C)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_operand_of_a_unary_dot_is_a_parenthesized_concatenation()
    {
        var program = @"        X = .(B C)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = .(B C)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_operand_of_a_keyword_operator_is_a_parenthesized_concatenation()
    {
        var program = @"        X = &(B C)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = &(B C)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_subject_of_an_equal_in_a_call_is_a_concatenation()
    {
        var program = @"        X = DIFFER(B C = POP())
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = DIFFER(B C = POP())

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_subject_of_an_equal_in_a_call_after_other_arguments()
    {
        var program = @"        X = HOST(2, I G = I + 1)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = HOST(2, I G = I + 1)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_NameOperands_the_statements_own_equal_takes_a_pattern_and_replacement()
    {
        var program = @"        Y = 'abc'
        Y 'b' = 'X'
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    // the = of a chained assignment takes a concatenation as the subject and pattern of its own match: compiled, sbl accepts these
    [TestMethod]
    public void TEST_NameOperands_a_concatenation_before_an_equal_at_the_statements_level_compiles()
    {
        foreach (var statement in new[] { "A = X 'b' = 2", "A = X Y = 2", "B = A = X 'b' = 2", "A = X Y Z = 2" })
            Assert.AreEqual(0, SetupTests.SetupScript("-b -n", $"        {statement}\nEND").ErrorCodeHistory.Count, statement);
    }

    // an alternation is no subject unless it is over a concatenation
    [TestMethod]
    public void TEST_NameOperands_an_alternation_that_is_no_pattern_match_as_a_target_is_212()
    {
        var build = SetupTests.SetupScript("-b -n", "        A | B C = 2\nEND");
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }
}
