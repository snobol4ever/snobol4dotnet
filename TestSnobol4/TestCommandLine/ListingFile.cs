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
}
