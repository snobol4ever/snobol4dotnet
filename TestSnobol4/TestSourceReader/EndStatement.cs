using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// After END comes nothing or the name of the label the run starts at, which the program must have defined, and nothing after that:
/// anything else is error 215, "undefined or erroneous entry label", under the element at fault. No line continues the END statement:
/// what follows it is data, a line that begins with a dot or a plus included. Every expectation is what sbl -b printed for the same
/// program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class EndStatement
{
    [TestMethod]
    public void TEST_EndStatement_an_entry_label_starts_the_run_there()
    {
        var program = @"        OUTPUT = 'a'
X       OUTPUT = 'x'
END X";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EndStatement_an_undefined_entry_label()
    {
        var program = @"        OUTPUT = 'a'
END X";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

2       END X
            !
Test.sno(2,5) : ERROR 215 -- syntax error: undefined or erroneous entry label", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EndStatement_an_entry_label_that_is_a_number()
    {
        var program = @"        OUTPUT = 'a'
END 123";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

2       END 123
            !
Test.sno(2,5) : ERROR 215 -- syntax error: undefined or erroneous entry label", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EndStatement_an_entry_label_in_parens()
    {
        var program = @"        OUTPUT = 'a'
X       OUTPUT = 'x'
END (X)";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

3       END (X)
            !
Test.sno(3,5) : ERROR 215 -- syntax error: undefined or erroneous entry label", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EndStatement_a_second_name_after_the_entry_label()
    {
        var program = @"        OUTPUT = 'a'
X       OUTPUT = 'x'
END X Y";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

3       END X Y
              !
Test.sno(3,7) : ERROR 215 -- syntax error: undefined or erroneous entry label", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EndStatement_an_entry_label_that_is_a_string()
    {
        var program = @"        OUTPUT = 'a'
END 'a'";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

2       END 'a'
            !
Test.sno(2,5) : ERROR 215 -- syntax error: undefined or erroneous entry label", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EndStatement_a_comma_after_the_entry_label()
    {
        var program = @"        OUTPUT = 'a'
X       OUTPUT = 'x'
END X,Y";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

3       END X,Y
             !
Test.sno(3,6) : ERROR 215 -- syntax error: undefined or erroneous entry label", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EndStatement_a_dot_line_after_end_continues_nothing()
    {
        var program = @"        OUTPUT = 'x'
END
./*
-TITLE foo";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EndStatement_a_plus_line_after_end_continues_nothing()
    {
        var program = @"        OUTPUT = 'x'
END
+ junk";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
