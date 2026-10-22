using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// A control statement is read as SPITBOL reads it: its name is told by its first four letters, a name it does not know is ignored, and
/// a number, a string, a call of a function or a comma where a name should stand, or a missing or wrong operand of -LINE, -INCLUDE,
/// -COPY, -CASE or -SPACE, is error 247 under the first character of the element at fault. After a comma comes another control, but
/// after -CASE, -DUMP, -EJECT, -INCLUDE, -LINE, -SPACE, -STITL and -TITLE the rest of the line is not read. -INnnn cuts the lines
/// after it to nnn characters. Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ControlStatements
{
    [TestMethod]
    public void TEST_ControlStatements_a_number_is_no_control()
    {
        var program = @"-5
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -5
         !
Test.sno(1,2) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_call_of_a_function_is_no_control()
    {
        var program = @"-LIST(1)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST(1)
         !
Test.sno(1,2) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_string_is_no_control()
    {
        var program = @"-""LIST""
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -""LIST""
         !
Test.sno(1,2) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_comma_first_is_no_control()
    {
        var program = @"-,LIST
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -,LIST
         !
Test.sno(1,2) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_number_after_a_comma()
    {
        var program = @"-LIST,5
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST,5
              !
Test.sno(1,7) : ERROR 247 -- invalid control statement

1               OUTPUT = 'x'
2       END", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_name_after_a_comma_can_be_a_second_control()
    {
        var program = @"-NOLIST,LIST,NOLIST
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_name_is_told_by_its_first_four_letters()
    {
        var program = @"-NOEXECC
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_name_of_three_letters_is_ignored()
    {
        var program = @"-NOE
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_an_unknown_name_is_ignored()
    {
        var program = @"-HIDE
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_an_unknown_name_ends_the_controls_unless_a_comma_follows()
    {
        var program = @"-FOO,NOEXEC
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_line_without_a_number()
    {
        var program = @"-LINE
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LINE
         !
Test.sno(1,2) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_line_zero()
    {
        var program = @"-LINE 0
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LINE 0
              !
Test.sno(1,7) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_line_of_a_name()
    {
        var program = @"-LINE x
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LINE x
              !
Test.sno(1,7) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_line_of_a_real()
    {
        var program = @"-LINE 5.0
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LINE 5.0
              !
Test.sno(1,7) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_line_with_a_file_that_is_no_string()
    {
        var program = @"-LINE 5 foo
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LINE 5 foo
                !
Test.sno(1,9) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_line_with_a_second_number()
    {
        var program = @"-LINE 5 7
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LINE 5 7
                !
Test.sno(1,9) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_line_reads_nothing_after_the_file()
    {
        var program = @"-LINE 5 'foo',LIST
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_include_without_a_name()
    {
        var program = @"-INCLUDE
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -INCLUDE
         !
Test.sno(1,2) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_include_of_an_unquoted_name()
    {
        var program = @"-INCLUDE x
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -INCLUDE x
                 !
Test.sno(1,10) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_copy_without_a_name()
    {
        var program = @"-COPY
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -COPY
         !
Test.sno(1,2) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_copy_of_an_unquoted_name()
    {
        var program = @"-COPY x
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -COPY x
              !
Test.sno(1,7) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_case_of_a_name()
    {
        var program = @"-CASE x
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -CASE x
              !
Test.sno(1,7) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_case_of_a_string_that_is_no_number()
    {
        var program = @"-CASE 'x'
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -CASE 'x'
              !
Test.sno(1,7) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_case_of_a_real_is_cut()
    {
        var program = @"-CASE 1.5
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_case_reads_nothing_after_its_number()
    {
        var program = @"-CASE 1,LIST
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_case_of_the_null_string()
    {
        var program = @"-CASE ''
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_space_of_a_name_with_a_listing()
    {
        var program = @"-LIST,SPACE x
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST,SPACE x
                    !
Test.sno(1,13) : ERROR 247 -- invalid control statement

1               OUTPUT = 'x'
2       END", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_eject_reads_nothing_after_it()
    {
        var program = @"-EJECT,LIST
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_dump_reads_nothing_after_it()
    {
        var program = @"-DUMP,LIST
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_an_invalid_number_item()
    {
        var program = @"-LINE 5A
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LINE 5A
              !
Test.sno(1,7) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_an_invalid_number_item_after_a_control()
    {
        var program = @"-LIST 5A
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST 5A
              !
Test.sno(1,7) : ERROR 231 -- syntax error: invalid numeric item

1               OUTPUT = 'x'
2       END", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_an_unmatched_quote_after_a_control()
    {
        var program = @"-LIST 'abc
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST 'abc
              !
Test.sno(1,7) : ERROR 232 -- syntax error: unmatched string quote

1               OUTPUT = 'x'
2       END", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_control_in_error_lists_once_when_listing_was_on()
    {
        var program = @"-LIST
-NOLIST,5
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
        -NOLIST,5
                !
Test.sno(2,9) : ERROR 247 -- invalid control statement", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_control_after_a_semicolon()
    {
        var program = @"        X = 1;-LIST
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1;-LIST
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_control_ends_its_line()
    {
        var program = @"-LIST;LIST
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST;LIST
1               OUTPUT = 'x'
2       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_in_cuts_the_lines_after_it()
    {
        var program = @"-IN8
        OUTPUT = 'x'
        OUTPUT = 'y'
END";
        Assert.AreEqual(@"", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_in_of_a_real()
    {
        var program = @"-IN5.5
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_in_and_a_listing()
    {
        var program = @"-IN5,LIST
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -IN5,LIST
1       
2       END", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_in_with_an_invalid_number()
    {
        var program = @"-IN5A
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -IN5A
           !
Test.sno(1,4) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_in_in_lower_case_whatever_the_case_folding()
    {
        var program = @"-CASE 0
-in5
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_a_name_in_lower_case_is_a_control_only_while_names_fold()
    {
        var program = @"-CASE 0
-noexec
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ControlStatements_code_with_a_control_in_error_fails()
    {
        var program = @"        C = CODE('-LINE x') :S(OK)
        OUTPUT = 'failed ' &ERRTYPE :(END)
OK      OUTPUT = 'ok'
END";
        Assert.AreEqual(@"failed 247", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
