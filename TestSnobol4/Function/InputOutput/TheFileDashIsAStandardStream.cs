using Test.Miscellaneous;
using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// A file named - is a standard stream, as in SPITBOL's osopen: INPUT reads standard input (fd 0) and OUTPUT writes standard error
/// (fd 2), through the channel's buffer of 1024 bytes, so what the program writes there appears when the buffer fills or the run ends,
/// after the termination dump (sbl -bf: OUTPUT('out', 1, '-') then out = 'a' and OUTPUT = 'std' prints std, then a). snobol4dotnet
/// opened a file named - in the current folder (rosetta-snobol4 loops-n-plus-one-half-2, graded by its dump).
/// </summary>
[DoNotParallelize]
[TestClass]
public class TheFileDashIsAStandardStream
{
    [TestMethod]
    public void TEST_Output_to_dash_writes_standard_error_when_the_run_ends()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "        OUTPUT('out', 1, '-')\n        out = 'a'\n        TERMINAL = 't'\n        out = 'b'\n        OUTPUT = 'std'\nEND\n" };
        Assert.AreEqual("t\nstd\na\nb", IncludeInSourceOrder.Run(files));
    }
}
