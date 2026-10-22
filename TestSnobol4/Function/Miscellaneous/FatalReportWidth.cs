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

    [TestMethod]
    public void TEST_FatalReportWidth_only_the_last_piece_of_a_cut_line_is_copied_to_standard_error()
    {
        // sbl -b -t40 -o=prog: the error line is cut into two pieces on the listing, and only the second reaches standard error
        var dir = Path.Combine(Path.GetTempPath(), "snowidth_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var oldError = Console.Error;
        using var error = new StringWriter();
        Console.SetError(error);
        try
        {
            using var build = SetupTests.SetupScript($"-b -t40 -o={Path.Combine(dir, "prog")}", "        OUTPUT = 'a'\n        :(CONTINUE)\n        OUTPUT = 'b'\nEND");
            var copy = error.ToString().Replace("\r\n", "\n");
            StringAssert.StartsWith(copy, " with no preceding error\n\n\nin file              Test.sno\n");
            StringAssert.Contains(File.ReadAllText(Path.Combine(dir, "prog.lst")).Replace("\r\n", "\n"), "Test.sno(2) : ERROR 037 -- goto continue\n with no preceding error\n");
        }
        finally
        {
            Console.SetError(oldError);
            Directory.Delete(dir, true);
        }
    }

    [TestMethod]
    public void TEST_FatalReportWidth_a_cut_compile_error_line_is_copied_to_standard_error_by_its_last_piece_too()
    {
        // sbl -b -t40 -o=prog on X = 1 / 0: the listing has "division caus" and "ed integer overflow", standard error only the second
        var dir = Path.Combine(Path.GetTempPath(), "snowidth_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var oldError = Console.Error;
        using var error = new StringWriter();
        Console.SetError(error);
        try
        {
            using var build = SetupTests.SetupScript($"-b -t40 -o={Path.Combine(dir, "prog")}", "        X = 1 / 0\n        OUTPUT = 'b'\nEND");
            Assert.AreEqual("1               X = 1 / 0\n\ned integer overflow\n\n", error.ToString().Replace("\r\n", "\n"));
            StringAssert.Contains(File.ReadAllText(Path.Combine(dir, "prog.lst")).Replace("\r\n", "\n"), "Test.sno(1) : ERROR 014 -- division caus\ned integer overflow\n");
        }
        finally
        {
            Console.SetError(oldError);
            Directory.Delete(dir, true);
        }
    }
}
