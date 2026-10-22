using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// The fatal run-time report is cut at the page width, 120 by default and -t's value otherwise, mid-word. The expected text is what sbl -b -t
/// printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class FatalReportWidth
{
    [TestMethod]
    public void TEST_FatalReportWidth_width_40()
    {
        var program = @"        OUTPUT = 'a'
        :(CONTINUE)
        OUTPUT = 'b'
END";
        Assert.AreEqual(@"a



Test.sno(2) : ERROR 037 -- goto continue
 with no preceding error





in file              Test.sno
in line              2
in statement         2
stmts executed       2
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -", SetupTests.MaskReport(SetupTests.RunWithInputDirectives("-b -t40", program)));
    }

    [TestMethod]
    public void TEST_FatalReportWidth_width_60()
    {
        var program = @"        OUTPUT = 'a'
        :(CONTINUE)
        OUTPUT = 'b'
END";
        Assert.AreEqual(@"a



Test.sno(2) : ERROR 037 -- goto continue with no preceding e
rror





in file              Test.sno
in line              2
in statement         2
stmts executed       2
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -", SetupTests.MaskReport(SetupTests.RunWithInputDirectives("-b -t60", program)));
    }

    [TestMethod]
    public void TEST_FatalReportWidth_width_200()
    {
        var program = @"        OUTPUT = 'a'
        :(CONTINUE)
        OUTPUT = 'b'
END";
        Assert.AreEqual(@"a



Test.sno(2) : ERROR 037 -- goto continue with no preceding error





in file              Test.sno
in line              2
in statement         2
stmts executed       2
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -", SetupTests.MaskReport(SetupTests.RunWithInputDirectives("-b -t200", program)));
    }
}
