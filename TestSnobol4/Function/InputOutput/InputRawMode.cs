using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// INPUT's raw mode (the R, Q and C options, as in INPUT(.INPUT, 9, '[-f0 -r4194304]'), which the demos use to read all of standard
/// input at once): an input request reads exactly that many characters, line ends and all, except what is left when the input ends,
/// fails when none is left, and trims nothing. Every expected text is what sbl -b printed for the same program on the same input.
/// </summary>
[DoNotParallelize]
[TestClass]
public class InputRawMode
{
    [TestMethod]
    public void TEST_InputRawMode_standard_input_reads_n_characters_at_a_time_line_ends_included()
    {
        var s = "        INPUT(.INPUT, 9, '[-f0 -r10]')\nL       X = INPUT   :F(END)\n        OUTPUT = SIZE(X) ' ' REPLACE(X, CHAR(10), '|')   :(L)\nEND";
        Assert.AreEqual("10 abcdefghij\n10 klmnopqrst\n10 uvwxyz|sec\n9 ond line|", SetupTests.RunWithInput(s, "abcdefghijklmnopqrstuvwxyz\nsecond line"));
    }

    [TestMethod]
    public void TEST_InputRawMode_c_is_one_character_at_a_time()
    {
        var s = "        INPUT(.INPUT, 9, '[-f0 -c]')\n        X = INPUT\n        Y = INPUT\n        OUTPUT = X '|' Y\nEND";
        Assert.AreEqual("x|y", SetupTests.RunWithInput(s, "xyz"));
    }

    [TestMethod]
    public void TEST_InputRawMode_a_raw_record_is_not_trimmed()
    {
        var s = "        &TRIM = 1\n        INPUT(.INPUT, 9, '[-f0 -r6]')\nL       X = INPUT   :F(END)\n        OUTPUT = SIZE(X) ' ' REPLACE(X, CHAR(10), '|')   :(L)\nEND";
        Assert.AreEqual("6 ab  |c\n3 d	|", SetupTests.RunWithInput(s, "ab  \ncd	"));
    }

    [TestMethod]
    public void TEST_InputRawMode_the_whole_input_in_one_request()
    {
        var s = "        INPUT(.INPUT, 9, '[-f0 -r4194304]')\n        X = INPUT\n        OUTPUT = SIZE(X)\n        Y = INPUT   :S(END)\n        OUTPUT = 'then end'\nEND";
        Assert.AreEqual("19\nthen end", SetupTests.RunWithInput(s, "first\nsecond\nthird"));
    }

    [TestMethod]
    public void TEST_InputRawMode_a_file_reads_n_characters_at_a_time_too()
    {
        var path = Path.Combine(Path.GetTempPath(), "raw_" + Guid.NewGuid().ToString("N")[..8] + ".txt");
        File.WriteAllText(path, "A line one\nB line two\nC\n");
        var s = "        INPUT(.W, 4, '" + path + "[-r8]')\nL       X = W   :F(END)\n        OUTPUT = SIZE(X) ' ' REPLACE(X, CHAR(10), '|')   :(L)\nEND";
        Assert.AreEqual("8 A line o\n8 ne|B lin\n8 e two|C|", SetupTests.RunWithInput(s));
    }
}
