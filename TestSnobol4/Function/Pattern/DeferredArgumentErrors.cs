using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// The argument of a pattern function written as *X is evaluated when the match reaches it, and a value that does not suit is an error with the
/// code of the function: ANY 43, BREAK 44, BREAKX 45, NOTANY 49, SPAN 56 (a string that is not null), LEN 47 and 48, POS 50 and 51, RPOS 52 and 53,
/// RTAB 54 and 55, TAB 57 and 58 (an integer that is not negative or too large); an expression that fails fails the match with no error, and a
/// variable holding an expression is evaluated again. The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class DeferredArgumentErrors
{
    [TestMethod]
    public void TEST_DeferredArgumentErrors_any_with_a_null_value()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' ANY(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 43", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_any_with_an_array()
    {
        var program = @"        &ERRLIMIT = 100
        X = ARRAY(2)
        'abcdef' ANY(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 43", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_notany_with_a_null_value()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' NOTANY(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 49", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_notany_with_an_array()
    {
        var program = @"        &ERRLIMIT = 100
        X = ARRAY(2)
        'abcdef' NOTANY(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 49", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_span_with_a_null_value()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' SPAN(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 56", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_span_with_an_array()
    {
        var program = @"        &ERRLIMIT = 100
        X = ARRAY(2)
        'abcdef' SPAN(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 56", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_break_with_a_null_value()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' BREAK(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 44", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_break_with_an_array()
    {
        var program = @"        &ERRLIMIT = 100
        X = ARRAY(2)
        'abcdef' BREAK(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 44", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_breakx_with_a_null_value()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' BREAKX(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 45", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_breakx_with_an_array()
    {
        var program = @"        &ERRLIMIT = 100
        X = ARRAY(2)
        'abcdef' BREAKX(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 45", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_len_with_a_string()
    {
        var program = @"        &ERRLIMIT = 100
        X = 'abc'
        'abcdef' LEN(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 47", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_len_with_a_negative_number()
    {
        var program = @"        &ERRLIMIT = 100
        X = -1
        'abcdef' LEN(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 48", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_len_with_a_number_too_large()
    {
        var program = @"        &ERRLIMIT = 100
        X = 99999999999
        'abcdef' LEN(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 48", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_pos_with_a_string()
    {
        var program = @"        &ERRLIMIT = 100
        X = 'abc'
        'abcdef' POS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 50", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_pos_with_a_negative_number()
    {
        var program = @"        &ERRLIMIT = 100
        X = -1
        'abcdef' POS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 51", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_pos_with_a_number_too_large()
    {
        var program = @"        &ERRLIMIT = 100
        X = 99999999999
        'abcdef' POS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 51", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_rpos_with_a_string()
    {
        var program = @"        &ERRLIMIT = 100
        X = 'abc'
        'abcdef' RPOS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 52", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_rpos_with_a_negative_number()
    {
        var program = @"        &ERRLIMIT = 100
        X = -1
        'abcdef' RPOS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 53", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_rpos_with_a_number_too_large()
    {
        var program = @"        &ERRLIMIT = 100
        X = 99999999999
        'abcdef' RPOS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 53", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_tab_with_a_string()
    {
        var program = @"        &ERRLIMIT = 100
        X = 'abc'
        'abcdef' TAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 57", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_tab_with_a_negative_number()
    {
        var program = @"        &ERRLIMIT = 100
        X = -1
        'abcdef' TAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 58", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_tab_with_a_number_too_large()
    {
        var program = @"        &ERRLIMIT = 100
        X = 99999999999
        'abcdef' TAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 58", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_rtab_with_a_string()
    {
        var program = @"        &ERRLIMIT = 100
        X = 'abc'
        'abcdef' RTAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 54", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_rtab_with_a_negative_number()
    {
        var program = @"        &ERRLIMIT = 100
        X = -1
        'abcdef' RTAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 55", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_rtab_with_a_number_too_large()
    {
        var program = @"        &ERRLIMIT = 100
        X = 99999999999
        'abcdef' RTAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 55", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_len_with_an_expression_that_fails()
    {
        var program = @"        DEFINE('F()')   :(GO)
F       :(FRETURN)
GO      &ERRLIMIT = 100
        'abcdef' LEN(*F())  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_any_with_an_expression_that_fails()
    {
        var program = @"        DEFINE('F()')   :(GO)
F       :(FRETURN)
GO      &ERRLIMIT = 100
        'abcdef' ANY(*F())  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredArgumentErrors_len_with_a_variable_holding_an_expression()
    {
        var program = @"        &ERRLIMIT = 100
        X = *Y
        'abcdef' LEN(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"S 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
