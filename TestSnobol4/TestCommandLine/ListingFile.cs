using Test.TestLexer;

namespace Test.TestCommandLine;

/// <summary>
/// -o=file sends the listing (a -LIST control line turns it on) to file.lst and not to the program's
/// streams, as SPITBOL's -o=file[.lst] does.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ListingFile
{
    [TestMethod]
    public void TEST_ListingFile_the_listing_goes_to_the_named_file()
    {
        var dir = Path.Combine(Path.GetTempPath(), "snolisting_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var oldError = Console.Error;
        using var error = new StringWriter();
        Console.SetError(error);
        try
        {
            var build = SetupTests.SetupScript($"-b -o={Path.Combine(dir, "prog")}", @"-LIST
        X = 1
END");
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            var listing = File.ReadAllText(Path.Combine(dir, "prog.lst"));
            StringAssert.Contains(listing, "X = 1");
            Assert.AreEqual("", error.ToString());
            build.Dispose();
        }
        finally
        {
            Console.SetError(oldError);
            Directory.Delete(dir, true);
        }
    }

    [TestMethod]
    public void TEST_ListingFile_takes_the_end_of_run_dump()
    {
        // sbl -bf -o=dir/prog with &DUMP = 1 writes the end-of-run dump to prog.lst, not standard output,
        // as its end-of-run report goes; an explicit DUMP(1) still writes to standard output
        var dir = Path.Combine(Path.GetTempPath(), "snolisting_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var output = SetupTests.RunWithInputDirectives($"-b -o={Path.Combine(dir, "prog")}", @"        A = 1
        &DUMP = 1
END");
            Assert.IsFalse(output.Contains("dump of natural variables"), output);
            StringAssert.Contains(File.ReadAllText(Path.Combine(dir, "prog.lst")), "dump of natural variables");
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public void TEST_ListingFile_listing_controls_are_silent()
    {
        // sbl -b accepts -TITLE, -STITL, -PRINT, -NOPRINT, -DOUBLE, -OPTIMIZE, -NOOPTIMIZE and -EJECT without a
        // word (testpgms' test6 starts with -TITLE); snobol4dotnet warned on standard error for most of them
        var oldError = Console.Error;
        using var error = new StringWriter();
        Console.SetError(error);
        try
        {
            var output = SetupTests.RunWithInput(@"-TITLE A TITLE
-STITL SUB
-PRINT
-NOPRINT
-DOUBLE
-OPTIMIZE
-NOOPTIMIZE
-EJECT
        OUTPUT = 'ok'
END");
            Assert.AreEqual("ok", output);
            Assert.AreEqual("", error.ToString());
        }
        finally
        {
            Console.SetError(oldError);
        }
    }

    [TestMethod]
    public void TEST_ListingFile_spitbol_listing_on_standard_output()
    {
        // sbl -bh: with no -o the -LIST listing goes to standard output in SPITBOL's layout: three blank lines,
        // "page 1" after 112 blanks, a blank line, then each line with its first statement's number in an
        // eight-column field (comments, controls and continuations eight blanks), blank lines numbered, trailing
        // blanks trimmed, -NOLIST regions left out (the dotnet package's SourceLines001-003). snobol4dotnet
        // wrote its own layout to standard error
        var expected = string.Join("\n", "", "", "", new string(' ', 112) + "page 1", "",
            "        -LIST",
            "        * a comment",
            "1        X = 1 ; Y = 2",
            "3       LBL Z = 3",
            "        + 4",
            "4       ",
            "5       ",
            "6        R = 5",
            "        -LIST",
            "8        U = 8",
            "9       END");
        var output = SetupTests.RunWithInputDirectives("-b -h", "-LIST\n* a comment\n X = 1 ; Y = 2\nLBL Z = 3\n+ 4\n \n\n R = 5\n" +
                                                                 "-NOLIST\n T = 7\n-LIST\n U = 8\nEND");
        Assert.AreEqual(expected.TrimEnd(), output.Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_ListingFile_the_fatal_report_of_the_long_format_uses_form_feeds()
    {
        // sbl -bf -p -o=prog: in the long format the listing's three empty lines before the error line are a form feed, and the five after
        // it are an empty line and a form feed (standard error and the other formats keep the empty lines)
        var dir = Path.Combine(Path.GetTempPath(), "snolisting_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var oldError = Console.Error;
        using var error = new StringWriter();
        Console.SetError(error);
        try
        {
            using var build = SetupTests.SetupScript($"-b -p -o={Path.Combine(dir, "prog")}", "        OUTPUT = 'a'\n        X = FOO(1)\n        OUTPUT = 'b'\nEND");
            var listing = File.ReadAllText(Path.Combine(dir, "prog.lst")).Replace("\r\n", "\n");
            StringAssert.Contains(listing, "4       END\n\fTest.sno(2) : ERROR 022 -- undefined function called\n\n\f\nin file              Test.sno\nin line              2\n");
            StringAssert.Contains(error.ToString().Replace("\r\n", "\n"), "Test.sno(2) : ERROR 022 -- undefined function called\n\n\n");
            Assert.IsFalse(error.ToString().Contains('\f'));
        }
        finally
        {
            Console.SetError(oldError);
            Directory.Delete(dir, true);
        }
    }
}
