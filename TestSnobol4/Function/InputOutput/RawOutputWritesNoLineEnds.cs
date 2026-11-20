using Test.Miscellaneous;

namespace Test.InputOutput;

/// <summary>
/// A file opened for output in raw mode (the R n, Q n or C option of its specification) is written with no line ends, as in SPITBOL:
/// OUTPUT('out', 1, 'g.txt[-r1]') then out = 'a' and out = 'bcdefgh' leaves the eight bytes abcdefgh (sbl -bf). snobol4dotnet ended
/// each record with a line end (rosetta-snobol4 loops-n-plus-one-half-2, graded by its dump, writes 1,2,...,10 so).
/// </summary>
[DoNotParallelize]
[TestClass]
public class RawOutputWritesNoLineEnds
{
    [TestMethod]
    public void TEST_Raw_output_to_a_file_writes_no_line_ends()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "        OUTPUT('out', 1, 'g.txt[-r1]')\n        out = 'a'\n        out = 'bcdefgh'\n        ENDFILE(1)\n        INPUT('in', 2, 'g.txt')\n        OUTPUT = in\nEND\n" };
        Assert.AreEqual("abcdefgh", IncludeInSourceOrder.Run(files));
    }

    [TestMethod]
    public void TEST_Raw_output_to_dash_writes_no_line_ends()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "        OUTPUT('out', 1, '-[-r1]')\n        out = 1\n        out = ','\n        out = 2\nEND\n" };
        Assert.AreEqual("1,2", IncludeInSourceOrder.Run(files));
    }
}
