using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SORT and RSORT of a table make one numbered object, the array they return: the array SPITBOL makes from the table on the way is no object
/// the dump numbers, so the next array or table made is the next number. The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SortBlockNumbers
{
    [TestMethod]
    public void TEST_SortBlockNumbers_sort_of_a_table_uses_one_number()
    {
        var program = @"        T = TABLE()
        T<'x'> = 2
        T<'y'> = 1
        B = SORT(T, 2)
        C = ARRAY(3)
        DUMP(1)
END";
        Assert.AreEqual(@"


dump of natural variables

B = ARRAY(2,2) #2
C = ARRAY(3) #3
T = TABLE(11) #1


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 0
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 5
&LASTNO = 5
&LINE = 6
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 6
&STLIMIT = 2147483647
&STNO = 6
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SortBlockNumbers_rsort_of_a_table_uses_one_number()
    {
        var program = @"        T = TABLE()
        T<'x'> = 2
        T<'y'> = 1
        B = RSORT(T, 2)
        C = ARRAY(3)
        DUMP(1)
END";
        Assert.AreEqual(@"


dump of natural variables

B = ARRAY(2,2) #2
C = ARRAY(3) #3
T = TABLE(11) #1


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 0
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 5
&LASTNO = 5
&LINE = 6
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 6
&STLIMIT = 2147483647
&STNO = 6
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SortBlockNumbers_convert_of_a_table_is_numbered()
    {
        var program = @"        T = TABLE()
        T<'x'> = 2
        B = CONVERT(T, 'ARRAY')
        C = ARRAY(3)
        DUMP(1)
END";
        Assert.AreEqual(@"


dump of natural variables

B = ARRAY(1,2) #2
C = ARRAY(3) #3
T = TABLE(11) #1


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 0
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 4
&LASTNO = 4
&LINE = 5
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 5
&STLIMIT = 2147483647
&STNO = 5
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SortBlockNumbers_sort_of_an_array_makes_a_new_array()
    {
        var program = @"        A = ARRAY(2)
        A<1> = 3
        A<2> = 1
        B = SORT(A)
        C = ARRAY(3)
        DUMP(1)
END";
        Assert.AreEqual(@"


dump of natural variables

A = ARRAY(2) #1
B = ARRAY(2) #2
C = ARRAY(3) #3


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 0
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 5
&LASTNO = 5
&LINE = 6
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 6
&STLIMIT = 2147483647
&STNO = 6
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
