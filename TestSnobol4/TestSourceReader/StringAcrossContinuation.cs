using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// A string literal cannot run over the end of a line into its continuation: the quote that is not closed on its own line is error 232,
/// unmatched string quote, at the opening quote, whether the next line goes on with + or with a dot. A string closed on its line, with
/// the statement going on after it, is fine. The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class StringAcrossContinuation
{
    [TestMethod]
    public void TEST_StringAcrossContinuation_a_string_open_at_the_end_of_a_line_is_232()
    {
        var program = @"        X = 'a
+       b'
        OUTPUT = X
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 'a
                    !
Test.sno(1,13) : ERROR 232 -- syntax error: unmatched string quote", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_StringAcrossContinuation_a_dot_continuation_is_the_same()
    {
        var program = @"        X = 'a
.       b'
        OUTPUT = X
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 'a
                    !
Test.sno(1,13) : ERROR 232 -- syntax error: unmatched string quote", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_StringAcrossContinuation_a_double_quote_is_the_same()
    {
        var program = @"        X = ""a
+       b""
        OUTPUT = X
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = ""a
                    !
Test.sno(1,13) : ERROR 232 -- syntax error: unmatched string quote", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_StringAcrossContinuation_a_string_closed_on_its_line_goes_on()
    {
        var program = @"        X = 'a' 
+       'b'
        OUTPUT = X
END";
        Assert.AreEqual(@"ab", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_StringAcrossContinuation_the_quote_in_the_second_line_is_232_at_its_own_column()
    {
        var program = @"        X = 'a' 'b
+       c'
        OUTPUT = X
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 'a' 'b
                        !
Test.sno(1,17) : ERROR 232 -- syntax error: unmatched string quote", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
