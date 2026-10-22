using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// A bracket after blanks opens the subscripts of the name before it, as in SPITBOL: A [1] is A<1>, T ['k'] is T<'k'>, and tabs and
/// blanks inside the brackets are nothing. A name that is the subscripted object can be an array or a table. The lexer took the
/// blank for the end of the name and a bracket there for error 222.
/// Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SubscriptsAfterBlanks
{
    [TestMethod]
    public void TEST_SubscriptsAfterBlanks_an_array_element_after_a_blank()
    {
        var program = @"        A = ARRAY(3)
        A [1] = 'x'
        OUTPUT = A [1]
END";
        Assert.AreEqual(@"x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SubscriptsAfterBlanks_a_table_element_after_a_blank()
    {
        var program = @"        T = TABLE()
        T ['k'] = 'v'
        OUTPUT = T ['k']
END";
        Assert.AreEqual(@"v", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SubscriptsAfterBlanks_an_angle_bracket_after_a_blank()
    {
        var program = @"        A = ARRAY(3)
        A <2> = 'y'
        OUTPUT = A <2>
END";
        Assert.AreEqual(@"y", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SubscriptsAfterBlanks_two_subscripts_after_a_blank()
    {
        var program = @"        A = ARRAY('2,2')
        A [1, 2] = 'z'
        OUTPUT = A [1, 2]
END";
        Assert.AreEqual(@"z", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SubscriptsAfterBlanks_several_blanks_and_a_tab()
    {
        var program = @"        A = ARRAY(3)
        A  [1] = 'p'
        OUTPUT = A	[1]
END";
        Assert.AreEqual(@"p", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SubscriptsAfterBlanks_blanks_inside_the_brackets()
    {
        var program = @"        A = ARRAY(3)
        A [ 1 ] = 'q'
        OUTPUT = A [ 1 ]
END";
        Assert.AreEqual(@"q", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SubscriptsAfterBlanks_a_subscript_after_a_blank_in_a_pattern_match()
    {
        var program = @"        A = ARRAY(2)
        A [1] = 'abc'
        A [1] ? 'b' . OUTPUT
END";
        Assert.AreEqual(@"b", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SubscriptsAfterBlanks_a_bracket_with_nothing_before_it_is_still_222()
    {
        var program = @"        X = [1]
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = [1]
                    !
Test.sno(1,13) : ERROR 222 -- syntax error: invalid use of left bracket", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SubscriptsAfterBlanks_a_bracket_after_an_operator_is_still_222()
    {
        var program = @"        X = 1 + [1]
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 + [1]
                        !
Test.sno(1,17) : ERROR 222 -- syntax error: invalid use of left bracket", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
