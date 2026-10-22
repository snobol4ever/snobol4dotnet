using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// A source line is read to 1024 bytes (SPITBOL's -IN1024) and the rest of a longer one is lost, so that a string that runs past the
/// end is unmatched. The expectations are what sbl -b printed for the same programs.
/// </summary>
[DoNotParallelize]
[TestClass]
public class LineLength
{
    private static string Program(int length) =>
        "        X = '" + new string('a', length - "        X = ''".Length) + "'\n        OUTPUT = SIZE(X)\nEND";

    [TestMethod]
    public void TEST_LineLength_a_line_of_1023_characters_is_read_whole()
    {
        Assert.AreEqual("1009", SetupTests.RunWithInput(Program(1023)));
    }

    [TestMethod]
    public void TEST_LineLength_a_line_of_1024_characters_is_read_whole()
    {
        Assert.AreEqual("1010", SetupTests.RunWithInput(Program(1024)));
    }

    [TestMethod]
    public void TEST_LineLength_a_line_of_1025_characters_loses_its_last_and_the_quote_is_unmatched()
    {
        var build = SetupTests.SetupScript("-b", Program(1025));
        Assert.AreEqual(232, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_LineLength_the_length_is_in_bytes()
    {
        // 600 two-byte letters are 1200 bytes: the line is cut after 512 of them
        var line = "        X = '" + new string('é', 600) + "'";
        var build = SetupTests.SetupScript("-b", line + "\n        OUTPUT = SIZE(X)\nEND");
        Assert.AreEqual(232, build.ErrorCodeHistory[0]);
    }
}
