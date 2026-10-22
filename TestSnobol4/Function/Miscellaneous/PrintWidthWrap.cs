using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// The lines SPITBOL prints through its print buffer (trace lines, the dump) are cut at the page width, 120 columns, mid-word, where the
/// OUTPUT statement's lines are not. The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class PrintWidthWrap
{
    [TestMethod]
    public void TEST_PrintWidthWrap_function_trace_lines_are_cut_at_120()
    {
        var program = @"        DEFINE('F(A)')
        &FTRACE = 4
        Y = F(DUPL('abcdefghij', 13))
        :(END)
F       F = A  :(RETURN)
END";
        Assert.AreEqual(@"****3*******  F('abcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabc
defghijabcdefghijabcdefghij')
****5*******  RETURN F = 'abcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcd
efghijabcdefghijabcdefghijabcdefghij'", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_PrintWidthWrap_traced_value_lines_are_cut_at_120()
    {
        var program = @"        &TRACE = 10
        TRACE('X', 'VALUE')
        X = DUPL('0123456789', 15)
        OUTPUT = X
END";
        Assert.AreEqual(@"****3*******  X = '01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
1234567890123456789012345678901234567890123456789'
012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_PrintWidthWrap_dump_lines_are_cut_at_120()
    {
        var program = @"        LONG = DUPL('abcdefghij', 20)
        A = ARRAY(2)
        A<2> = DUPL('xy', 70)
        DUMP(2)
END";
        Assert.AreEqual(@"


dump of natural variables

A = ARRAY(2) #1
LONG = 'abcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijab
cdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghij'


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
&LASTLINE = 3
&LASTNO = 3
&LINE = 4
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 4
&STLIMIT = 2147483647
&STNO = 4
&TRACE = 0
&TRIM = 1

ARRAY(2) #1
A<2> = 'xyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxyxy
xyxyxyxyxyxyxyxyxyxyxyxyxyxy'", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
