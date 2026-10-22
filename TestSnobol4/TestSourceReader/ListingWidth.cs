using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// A line longer than the width of the listing (120) is cut into lines of that width, mid-word, as sbl prints it: a statement in error
/// on the listing page and a source line listed by -LIST. Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ListingWidth
{
    [TestMethod]
    public void TEST_ListingWidth_a_long_statement_in_error_is_cut_at_120()
    {
        var program = @"        A = 'abc'
       -    IsType(cpmExpr, 'expression')                                                               :f(Xerror(239))
        OUTPUT = 'end'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

2              -    IsType(cpmExpr, 'expression')                                                               :f(Xerro
r(239))
               !
Test.sno(2,8) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingWidth_a_long_listed_line_is_cut_at_120()
    {
        var program = @"-LIST
        X = 'abababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababababab'
        OUTPUT = SIZE(X)
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               X = 'abababababababababababababababababababababababababababababababababababababababababababababababababa
babababababababababababababababababababababababababababababababababababababababababababababababababababababababababababa
babababababababababababababababababababababababababababababababababababababababab'
2               OUTPUT = SIZE(X)
3       END
300", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingWidth_a_line_of_exactly_the_width_is_not_cut()
    {
        var program = @"-LIST
        X = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
        OUTPUT = SIZE(X)
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               X = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
a'
2               OUTPUT = SIZE(X)
3       END
100", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingWidth_a_line_of_one_more_than_the_width_is_cut()
    {
        var program = @"-LIST
        X = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
        OUTPUT = SIZE(X)
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               X = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aa'
2               OUTPUT = SIZE(X)
3       END
101", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
