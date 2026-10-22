using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// A continuation line joins the statement before it with a blank in place of its continuation character: SPITBOL's scanner steps over the
/// character and counts it as blanks seen, so two names or two strings on either side of a break are concatenated, not run into one. Every
/// expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ContinuationJoin
{
    [TestMethod]
    public void TEST_ContinuationJoin_two_names_either_side_of_a_break()
    {
        var program = @"        A = 'x'
        B = 'y'
        X = A
+B
        OUTPUT = X
END";
        Assert.AreEqual(@"xy", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ContinuationJoin_a_dot_continues_as_a_plus_does()
    {
        var program = @"        Y = 'p'
.'q'
        OUTPUT = Y
END";
        Assert.AreEqual(@"pq", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ContinuationJoin_two_strings_either_side_of_a_break()
    {
        var program = @"        OUTPUT = 'abc'
+'def'
END";
        Assert.AreEqual(@"abcdef", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ContinuationJoin_a_name_is_not_split_by_a_break()
    {
        var program = @"        AB = 'ab'
        C = 'c'
        OUTPUT = AB
+C
END";
        Assert.AreEqual(@"abc", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ContinuationJoin_a_number_then_a_name()
    {
        var program = @"        N = 12
        OUTPUT = N
+N
END";
        Assert.AreEqual(@"1212", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ContinuationJoin_three_lines()
    {
        var program = @"        A = 'a'
        OUTPUT = A
+A
+A
END";
        Assert.AreEqual(@"aaa", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ContinuationJoin_an_unmatched_quote_on_a_continuation_line()
    {
        var program = @"        OUTPUT = 'a'
+       'b' 'c
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1       +       'b' 'c
                    !
Test.sno(2,13) : ERROR 232 -- syntax error: unmatched string quote", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ContinuationJoin_an_error_in_the_first_line_of_a_continued_statement()
    {
        var program = @"        X = (
+       Y
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1       +       Y
                !
Test.sno(2,9) : ERROR 226 -- syntax error: missing right paren", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ContinuationJoin_an_unmatched_quote_on_the_third_of_three_lines()
    {
        var program = @"        OUTPUT = 'a'
+       'b'
+       'c' 'd
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1       +       'c' 'd
                    !
Test.sno(3,13) : ERROR 232 -- syntax error: unmatched string quote", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
