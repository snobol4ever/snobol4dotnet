using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// The integer argument of LEN, POS, RPOS, TAB and RTAB, written out or evaluated at match time, may be as large as the longest object
/// SPITBOL makes (-m, 16,777,216 bytes by default) and no larger: past it is the function's out-of-range error. The expected text is what
/// sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class IntegerArgumentLimit
{
    [TestMethod]
    public void TEST_IntegerArgumentLimit_pos_immediate_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' POS(16777216)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_pos_deferred_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777216
        'abcdef' POS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_pos_immediate_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' POS(16777217)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

2               'abcdef' POS(16777217)  :S(YES)F(NO)

Test.sno(2) : ERROR 163 -- pos argument is negative or too large", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_pos_deferred_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777217
        'abcdef' POS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 51", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_rpos_immediate_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' RPOS(16777216)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_rpos_deferred_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777216
        'abcdef' RPOS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_rpos_immediate_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' RPOS(16777217)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

2               'abcdef' RPOS(16777217)  :S(YES)F(NO)

Test.sno(2) : ERROR 186 -- rpos argument is negative or too large", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_rpos_deferred_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777217
        'abcdef' RPOS(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 53", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_tab_immediate_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' TAB(16777216)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_tab_deferred_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777216
        'abcdef' TAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_tab_immediate_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' TAB(16777217)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

2               'abcdef' TAB(16777217)  :S(YES)F(NO)

Test.sno(2) : ERROR 184 -- tab argument is negative or too large", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_tab_deferred_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777217
        'abcdef' TAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 58", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_rtab_immediate_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' RTAB(16777216)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_rtab_deferred_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777216
        'abcdef' RTAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_rtab_immediate_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' RTAB(16777217)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

2               'abcdef' RTAB(16777217)  :S(YES)F(NO)

Test.sno(2) : ERROR 182 -- rtab argument is negative or too large", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_rtab_deferred_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777217
        'abcdef' RTAB(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 55", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_len_immediate_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' LEN(16777216)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_len_deferred_at_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777216
        'abcdef' LEN(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_len_immediate_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        'abcdef' LEN(16777217)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

2               'abcdef' LEN(16777217)  :S(YES)F(NO)

Test.sno(2) : ERROR 121 -- len argument is negative or too large", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IntegerArgumentLimit_len_deferred_past_the_limit()
    {
        var program = @"        &ERRLIMIT = 100
        X = 16777217
        'abcdef' LEN(*X)  :S(YES)F(NO)
YES     OUTPUT = 'S ' &ERRTYPE  :(END)
NO      OUTPUT = 'F ' &ERRTYPE
END";
        Assert.AreEqual(@"F 48", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
