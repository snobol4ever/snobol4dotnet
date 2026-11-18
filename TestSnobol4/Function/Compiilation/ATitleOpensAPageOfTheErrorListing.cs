using Test.TestLexer;

namespace Test.Compiilation;

/// <summary>
/// -TITLE (and -STITL) between two statements in error opens a new page under the new head before the second one's report, as sbl -bf
/// lists it: X = 1 +, -TITLE G360, Y = 2 +, END lists the first report, three blank lines, the head G360 ... page 2, and the second.
/// snobol4dotnet took the new page only for a line of the source listing (-l), so snolib's g360.spt, which sets -TITLE G360 after its
/// first error, broke its pages elsewhere.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ATitleOpensAPageOfTheErrorListing
{
    [TestMethod]
    public void TEST_ATitleOpensAPageOfTheErrorListing()
    {
        var output = SetupTests.RunWithInputDirectives("-bf", "        X = 1 +\n-TITLE G360\n        Y = 2 +\nEND\n").Replace("\r\n", "\n");
        var title = output.IndexOf("G360 ", System.StringComparison.Ordinal);
        var second = output.IndexOf("2               Y = 2 +", System.StringComparison.Ordinal);
        Assert.IsTrue(title > 0 && second > title, output);
        StringAssert.Contains(output, "page 2");
    }
}
