using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// An included file found through the SNOLIB list is named for what SPITBOL names it: the directory as the list gives it, a slash, and the
/// name as written (a directory given with its trailing slash gives two), in &amp;FILE and in the reports of errors in the file; one found
/// under the name as written (the current directory) keeps that name. The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class IncludeFileNames
{
    private static string WithSnolib(string list, Func<string> run)
    {
        var old = Environment.GetEnvironmentVariable("SNOLIB");
        Environment.SetEnvironmentVariable("SNOLIB", list);
        try
        {
            return run().Replace("\r\n", "\n");
        }
        finally
        {
            Environment.SetEnvironmentVariable("SNOLIB", old);
        }
    }

    [TestMethod]
    public void TEST_IncludeFileNames_a_file_found_through_snolib_is_named_with_the_directory_as_listed()
    {
        var folder = Path.Combine(Path.GetTempPath(), "inc_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder, "named.inc"), "        OUTPUT = 'in ' &FILE\n");
            var program = "-INCLUDE 'named.inc'\nEND";
            Assert.AreEqual($"in {folder}/named.inc", WithSnolib(folder, () => SetupTests.RunWithInput(program)));
            Assert.AreEqual($"in {folder}//named.inc", WithSnolib(folder + "/", () => SetupTests.RunWithInput(program)));
            Assert.AreEqual($"in {folder}/named.inc", WithSnolib("/nonexistent:" + folder, () => SetupTests.RunWithInput(program)));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [TestMethod]
    public void TEST_IncludeFileNames_an_error_in_the_file_names_it_with_the_directory()
    {
        var folder = Path.Combine(Path.GetTempPath(), "inc_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder, "bad.inc"), "        OUTPUT = 'first'\n        X = FOO(1)\n");
            var output = WithSnolib(folder, () => SetupTests.RunWithInput("-INCLUDE 'bad.inc'\nEND"));
            StringAssert.Contains(output, $"{folder}/bad.inc(2) : ERROR 022 -- undefined function called");
            StringAssert.Contains(output, $"in file              {folder}/bad.inc");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
