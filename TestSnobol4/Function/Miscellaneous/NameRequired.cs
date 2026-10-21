using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SPITBOL finds a value where a name is required when it generates the code of a statement (cdgnm): a literal, or an arithmetic result
/// or a name taken as a value (.Y, @Y), as the target of an assignment, the subject of a replacement, the right operand of . and $, or the
/// operand of unary . @ and &amp; is error 212, listed on the page with no column, and nothing runs. A variable, an indirection, a call,
/// an array or table reference and a keyword are names. Every expected text is what sbl -b printed for the same program saved as
/// Test.sno, with the head's identity and clock lines masked.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NameRequired
{
    private static readonly string Head = "\n\n\n<identity>\n<clock>\n\n\n\n" + new string(' ', 112) + "page 1\n\n";

    private static string Run(string script) => SetupTests.MaskListingHead(SetupTests.RunWithInput(script));

    [TestMethod]
    public void TEST_NameRequired_a_literal_target_of_an_assignment()
    {
        var s = "        OUTPUT = 'before'\n        'A' = 2\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual(Head + "2               'A' = 2\n\nTest.sno(2) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_an_arithmetic_result_as_target()
    {
        var s = "        X = 1\n        X + 1 = 2\nEND";
        Assert.AreEqual(Head + "2               X + 1 = 2\n\nTest.sno(2) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_a_literal_right_of_the_period()
    {
        var s = "        'abc' ? LEN(1) . 1\nEND";
        Assert.AreEqual(Head + "1               'abc' ? LEN(1) . 1\n\nTest.sno(1) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_a_literal_right_of_the_dollar()
    {
        var s = "        'abc' ? LEN(1) $ 'V'\nEND";
        Assert.AreEqual(Head + "1               'abc' ? LEN(1) $ 'V'\n\nTest.sno(1) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_a_literal_cursor_target()
    {
        var s = "        OUTPUT = 'x'\n        'FOO' @1\nEND";
        Assert.AreEqual(Head + "2               'FOO' @1\n\nTest.sno(2) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_a_literal_keyword()
    {
        var s = "        X = &1\nEND";
        Assert.AreEqual(Head + "1               X = &1\n\nTest.sno(1) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_a_literal_name()
    {
        var s = "        X = .1\nEND";
        Assert.AreEqual(Head + "1               X = .1\n\nTest.sno(1) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_a_name_taken_as_a_value_is_no_name()
    {
        var s = "        X = @.Y\nEND";
        Assert.AreEqual(Head + "1               X = @.Y\n\nTest.sno(1) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_a_literal_subject_of_a_replacement()
    {
        var s = "        1 ? 'a' = 'b'\nEND";
        Assert.AreEqual(Head + "1               1 ? 'a' = 'b'\n\nTest.sno(1) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_a_literal_subject_before_the_blank_pattern()
    {
        var s = "        'A' Y = 1\nEND";
        Assert.AreEqual(Head + "1               'A' Y = 1\n\nTest.sno(1) : ERROR 212 -- syntax error: value used where name is required", Run(s));
    }

    [TestMethod]
    public void TEST_NameRequired_a_variable_an_indirection_an_array_or_table_reference_and_a_replacement_subject_are_names()
    {
        var s = "        A = ARRAY(2)\n        T = TABLE()\n        N = 'V'\n        $N = 'dollar'\n        A<1> = 'arr'\n        T<'k'> = 'tab'\n        'abc' ? LEN(1) . Y\n        'abc' ? LEN(2) $ Z\n        S = 'hello'\n        S 'l' = 'L'\n        &ANCHOR = 0\n        OUTPUT = V ' ' A<1> ' ' T<'k'> ' ' Y ' ' Z ' ' S\nEND";
        Assert.AreEqual("dollar arr tab a ab heLlo", SetupTests.RunWithInput(s));
    }
}
