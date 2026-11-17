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

    // Under -f (no folding) sbl -bf still reads a line spelled end, all lower case, as the END line, as it reads END; a mixed spelling
    // (End, eNd) is a label like any other (sbl -bf, 2026-11-17: "OUTPUT = 1 / end / OUTPUT = 2 / END" prints 1, the same with End prints 1 2)
    [TestMethod]
    public void TEST_EndStatement_under_no_folding_a_line_spelled_end_ends_the_program()
    {
        var program = @"    OUTPUT = 1
end
    OUTPUT = 2
END";
        Assert.AreEqual("1", SetupTests.RunWithInputDirectives("-bf", program).TrimEnd());
    }

    [TestMethod]
    public void TEST_EndStatement_under_no_folding_a_line_spelled_End_is_a_label()
    {
        var program = @"    OUTPUT = 1
End
    OUTPUT = 2
END";
        Assert.AreEqual("1\n2", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd());
    }

    [TestMethod]
    public void TEST_EndStatement_under_no_folding_end_takes_an_entry_label()
    {
        var program = @"start    OUTPUT = 1
    OUTPUT = 2
end start";
        Assert.AreEqual("1\n2", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd());
    }

    [TestMethod]
    public void TEST_EndStatement_under_no_folding_a_goto_to_end_reaches_a_line_spelled_end()
    {
        var program = @"    OUTPUT = 1  :(end)
    OUTPUT = 2
end
    junk )(";
        Assert.AreEqual("1", SetupTests.RunWithInputDirectives("-bf", program).TrimEnd());
    }

    [TestMethod]
    public void TEST_EndStatement_under_no_folding_a_variable_named_end_and_a_label_End_are_untouched()
    {
        var program = @"    end = 5
    OUTPUT = end
    OUTPUT = 6 :(End)
End OUTPUT = 7
end";
        Assert.AreEqual("5\n6\n7", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd());
    }
}
