using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// What a fatal error leaves behind, as SPITBOL leaves it: &amp;ERRLIMIT is 0 after a RETURN at level zero (242) and a goto to an undefined
/// label (38); a stack overflow (246) prints its report and no end-of-run dump; :(CONTINUE) with no error pending is struck three times like
/// SCONTINUE (two trapped, &amp;ERRLIMIT 98, the third fatal); a computed goto to no label errors three times while the limit lasts and then is
/// error 332. The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class FatalErrorEffects
{
    [TestMethod]
    public void TEST_FatalErrorEffects_return_at_level_zero_zeroes_the_error_limit()
    {
        var program = @"        &ERRLIMIT = 100
        &DUMP = 1
        :(RETURN)
END";
        Assert.AreEqual(@"


Test.sno(3) : ERROR 242 -- function return from level zero





in file              Test.sno
in line              3
in statement         3
stmts executed       3
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -




dump of natural variables



dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 1
&ERRLIMIT = 0
&ERRTEXT = 'function return from level zero'
&ERRTYPE = 242
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 2
&LASTNO = 2
&LINE = 3
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 3
&STLIMIT = 2147483647
&STNO = 3
&TRACE = 0
&TRIM = 1", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_FatalErrorEffects_goto_to_an_undefined_label_zeroes_the_error_limit()
    {
        var program = @"        &ERRLIMIT = 100
        &DUMP = 1
        :(NOLAB)
END";
        Assert.AreEqual(@"


Test.sno(3) : ERROR 038 -- goto undefined label





in file              Test.sno
in line              3
in statement         3
stmts executed       3
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -




dump of natural variables



dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 1
&ERRLIMIT = 0
&ERRTEXT = 'goto undefined label'
&ERRTYPE = 38
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 2
&LASTNO = 2
&LINE = 3
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 3
&STLIMIT = 2147483647
&STNO = 3
&TRACE = 0
&TRIM = 1", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_FatalErrorEffects_stack_overflow_prints_no_dump()
    {
        // sbl -b prints the report and then, where the dump would be, a second one that says "stack overflow in garbage collection" (the
        // dump overflows the stack again); it prints no dump. That second report is not made here: the dump is only left out
        var program = @"        &ERRLIMIT = 100
        &DUMP = 1
        DEFINE('F(X)')  :(GO)
F       F = F(X + 1)  :(RETURN)
GO      F(1)
END";
        var output = SetupTests.MaskReport(SetupTests.RunWithInput(program));
        StringAssert.Contains(output, "Test.sno(4) : ERROR 246 -- stack overflow");
        Assert.IsFalse(output.Contains("dump of natural variables"), output);
    }

    [TestMethod]
    public void TEST_FatalErrorEffects_continue_with_no_error_is_struck_three_times()
    {
        var program = @"        &ERRLIMIT = 100
        &DUMP = 1
        :(CONTINUE)
END";
        Assert.AreEqual(@"


Test.sno(3) : ERROR 037 -- goto continue with no preceding error





in file              Test.sno
in line              3
in statement         3
stmts executed       3
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -




dump of natural variables



dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 1
&ERRLIMIT = 98
&ERRTEXT = 'goto continue with no preceding error'
&ERRTYPE = 37
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 2
&LASTNO = 2
&LINE = 3
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 3
&STLIMIT = 2147483647
&STNO = 3
&TRACE = 0
&TRIM = 1", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_FatalErrorEffects_computed_goto_to_no_label_errors_three_times_then_332_with_limit_100()
    {
        var program = @"        &ERRLIMIT = 100
        &DUMP = 1
        :($'NOLAB')
END";
        Assert.AreEqual(@"


Test.sno(3) : ERROR 332 -- goto continue with error in failure goto





in file              Test.sno
in line              3
in statement         3
stmts executed       3
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -




dump of natural variables



dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 1
&ERRLIMIT = 97
&ERRTEXT = 'goto continue with error in failure goto'
&ERRTYPE = 332
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 2
&LASTNO = 2
&LINE = 3
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 3
&STLIMIT = 2147483647
&STNO = 3
&TRACE = 0
&TRIM = 1", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_FatalErrorEffects_computed_goto_to_no_label_with_limit_1()
    {
        var program = @"        &ERRLIMIT = 1
        &DUMP = 1
        :($'NOLAB')
END";
        Assert.AreEqual(@"


Test.sno(3) : ERROR 332 -- goto continue with error in failure goto





in file              Test.sno
in line              3
in statement         3
stmts executed       3
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -




dump of natural variables



dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 1
&ERRLIMIT = 0
&ERRTEXT = 'goto continue with error in failure goto'
&ERRTYPE = 332
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 2
&LASTNO = 2
&LINE = 3
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 3
&STLIMIT = 2147483647
&STNO = 3
&TRACE = 0
&TRIM = 1", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_FatalErrorEffects_computed_goto_to_no_label_with_limit_2()
    {
        var program = @"        &ERRLIMIT = 2
        &DUMP = 1
        :($'NOLAB')
END";
        Assert.AreEqual(@"


Test.sno(3) : ERROR 332 -- goto continue with error in failure goto





in file              Test.sno
in line              3
in statement         3
stmts executed       3
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -




dump of natural variables



dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 1
&ERRLIMIT = 0
&ERRTEXT = 'goto continue with error in failure goto'
&ERRTYPE = 332
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 2
&LASTNO = 2
&LINE = 3
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 3
&STLIMIT = 2147483647
&STNO = 3
&TRACE = 0
&TRIM = 1", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_FatalErrorEffects_computed_goto_to_no_label_with_limit_4()
    {
        var program = @"        &ERRLIMIT = 4
        &DUMP = 1
        :($'NOLAB')
END";
        Assert.AreEqual(@"


Test.sno(3) : ERROR 332 -- goto continue with error in failure goto





in file              Test.sno
in line              3
in statement         3
stmts executed       3
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -




dump of natural variables



dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 1
&ERRLIMIT = 1
&ERRTEXT = 'goto continue with error in failure goto'
&ERRTYPE = 332
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 2
&LASTNO = 2
&LINE = 3
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 3
&STLIMIT = 2147483647
&STNO = 3
&TRACE = 0
&TRIM = 1", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }
}
