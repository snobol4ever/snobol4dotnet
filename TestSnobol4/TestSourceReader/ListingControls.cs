using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// The controls that lay out a listing: -TITLE and -STITL set the head of the pages (the title under the page number at column 113,
/// the subtitle on a line of its own, and a blank line; -TITLE makes the subtitle an empty line) and bring a new page at the next line
/// listed; -EJECT is a new page; -SPACE n is n blank lines (one for none or for 0), or a new page when they do not fit; -DOUBLE spaces the
/// lines that follow and -SINGLE ends it; -NOPRINT keeps control statements out of the listing and -PRINT brings them back. A title wider
/// than the page is cut into lines first and the page number goes over the last. Every expectation is what sbl -b printed for the same
/// program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ListingControls
{
    [TestMethod]
    public void TEST_ListingControls_eject()
    {
        var program = @"-LIST
        A = 1
-EJECT
        B = 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               A = 1



                                                                                                                page 2

        -EJECT
2               B = 2
3               OUTPUT = 'x'
4       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_eject_first()
    {
        var program = @"-LIST
-EJECT
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



                                                                                                                page 2

        -EJECT
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_eject_with_the_listing_off_does_nothing()
    {
        var program = @"-EJECT
-LIST
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_two_ejects()
    {
        var program = @"-LIST
-EJECT
-EJECT
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



                                                                                                                page 2

        -EJECT



                                                                                                                page 3

        -EJECT
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_title()
    {
        var program = @"-LIST
-TITLE My title
        A = 1
-EJECT
        B = 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



My title                                                                                                        page 2


        -TITLE My title
1               A = 1



My title                                                                                                        page 3


        -EJECT
2               B = 2
3               OUTPUT = 'x'
4       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_title_and_subtitle()
    {
        var program = @"-LIST
-STITL sub
        A = 1
-TITLE T2
        B = 2
-STITL s2
        C = 3
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



                                                                                                                page 2
sub

        -STITL sub
1               A = 1



T2                                                                                                              page 3


        -TITLE T2
2               B = 2



T2                                                                                                              page 4
s2

        -STITL s2
3               C = 3
4               OUTPUT = 'x'
5       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_title_set_before_the_listing_is_on()
    {
        var program = @"-TITLE T
        A = 1
-LIST
        B = 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



T                                                                                                               page 1


        -LIST
2               B = 2
3               OUTPUT = 'x'
4       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_title_with_the_listing_off_then_on()
    {
        var program = @"-NOLIST
-TITLE T
-LIST
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



T                                                                                                               page 1


        -LIST
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_title_with_nothing()
    {
        var program = @"-LIST
-TITLE
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



                                                                                                                page 2


        -TITLE
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_title_is_read_from_the_eighth_character()
    {
        var program = @"-LIST
-TITL abcdef
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



bcdef                                                                                                           page 2


        -TITL abcdef
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_a_title_of_112_characters()
    {
        var program = @"-LIST
-TITLE 0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901page 2


        -TITLE 012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
5678901
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_a_title_wider_than_the_page()
    {
        var program = @"-LIST
-TITLE 01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
01234567                                                                                                        page 2


        -TITLE 012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
56789012345678901234567
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_the_title_in_the_head_of_an_error_page()
    {
        var program = @"-TITLE My title
        A = (
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



My title                                                                                                        page 1


1               A = (
                    !
Test.sno(2,13) : ERROR 226 -- syntax error: missing right paren", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_the_title_and_subtitle_in_the_head_of_an_error_page()
    {
        var program = @"-TITLE ttl
-STITL sss
        A = (
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



ttl                                                                                                             page 1
sss

1               A = (
                    !
Test.sno(3,13) : ERROR 226 -- syntax error: missing right paren", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_space()
    {
        var program = @"-LIST
        A = 1
-SPACE 2
        B = 2
-SPACE
        C = 3
-SPACE 0
        D = 4
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               A = 1


        -SPACE 2
2               B = 2

        -SPACE
3               C = 3

        -SPACE 0
4               D = 4
5               OUTPUT = 'x'
6       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_space_first_opens_the_page()
    {
        var program = @"-LIST
-SPACE 3
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



        -SPACE 3
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_space_too_big_for_the_page_is_a_new_page()
    {
        var program = @"-LIST
        A = 1
-SPACE 60
        B = 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               A = 1



                                                                                                                page 2

        -SPACE 60
2               B = 2
3               OUTPUT = 'x'
4       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_space_that_fits_is_blank_lines()
    {
        var program = @"-LIST
        A = 1
-SPACE 40
        B = 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               A = 1








































        -SPACE 40
2               B = 2
3               OUTPUT = 'x'
4       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_two_spaces_that_do_not_fit_together()
    {
        var program = @"-LIST
-SPACE 40
-SPACE 40
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST








































        -SPACE 40



                                                                                                                page 2

        -SPACE 40
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_double()
    {
        var program = @"-LIST
-DOUBLE
        A = 1
        B = 2
-SINGLE
        C = 3
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
        -DOUBLE

1               A = 1

2               B = 2

        -SINGLE
3               C = 3
4               OUTPUT = 'x'
5       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_double_with_a_continued_statement()
    {
        var program = @"-LIST
-DOUBLE
        A = 1 +
+       2
        B = 3
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
        -DOUBLE

1               A = 1 +

        +       2

2               B = 3

3               OUTPUT = 'x'

4       END

x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_noprint()
    {
        var program = @"-LIST
-NOPRINT
-FOO
        A = 1
-PRINT
-BAR
        B = 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               A = 1
        -PRINT
        -BAR
2               B = 2
3               OUTPUT = 'x'
4       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_noprint_keeps_comments()
    {
        var program = @"-LIST
        A = 1
-NOPRINT
* a comment
        B = 2
-FOO
-PRINT
        C = 3
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST
1               A = 1
        * a comment
2               B = 2
        -PRINT
3               C = 3
4               OUTPUT = 'x'
5       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_controls_after_a_comma()
    {
        var program = @"-LIST,DOUBLE,NOPRINT
-FOO
        A = 1
-PRINT
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               A = 1

        -PRINT

2               OUTPUT = 'x'

3       END

x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_nothing_follows_a_title_on_its_line()
    {
        var program = @"-LIST,TITLE x
        A = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



ITLE x                                                                                                          page 1


        -LIST,TITLE x
1               A = 1
2               OUTPUT = 'x'
3       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ListingControls_a_title_after_a_semicolon()
    {
        var program = @"-LIST
        A = 1 ;-TITLE x
        C = 3
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -LIST



 A = 1 ;-TITLE x                                                                                                page 2


1               A = 1 ;-TITLE x
2               C = 3
3               OUTPUT = 'x'
4       END
x", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
