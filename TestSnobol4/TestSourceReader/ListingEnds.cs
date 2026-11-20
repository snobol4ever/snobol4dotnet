using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// How the listing ends: sbl lists a line when it has read the next, so the last line of a source with no END is never listed (a
/// continuation line of the last statement, or a comment, included); the END of an included file is listed with its number and depth and
/// none of its text, the include being gone when sbl looks at the line again. Every expectation is what sbl -b printed for the same
/// program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ListingEnds
{

    // sbl -bf -h printed these two lines for the program below: the END of the include, number and depth and no text
    [TestMethod]
    public void TEST_ListingEnds_the_end_of_an_included_file_is_listed_without_its_text()
    {
        var dir = Path.Combine(Path.GetTempPath(), "listing_ends_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var old = Environment.GetEnvironmentVariable("SNOLIB");
        try
        {
            File.WriteAllText(Path.Combine(dir, "listing_end_probe.inc"), "        A = 1\nEND\n");
            Environment.SetEnvironmentVariable("SNOLIB", dir);
            var listing = SetupTests.MaskListingHead(SetupTests.RunWithInput("-LIST\n-INCLUDE 'listing_end_probe.inc'\n        B = 2\n"));
            StringAssert.EndsWith(listing, "        -LIST\n        -INCLUDE 'listing_end_probe.inc'\n1    1          A = 1\n2    1");
        }
        finally
        {
            Environment.SetEnvironmentVariable("SNOLIB", old);
            Directory.Delete(dir, true);
        }
    }
}
