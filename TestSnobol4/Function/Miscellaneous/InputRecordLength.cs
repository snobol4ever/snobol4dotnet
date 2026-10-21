using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SPITBOL's line mode returns at most 1,024 characters of a record (the manual: "by default, SPITBOL provides a 1,024 character
/// record length for input") unless the L option of INPUT says otherwise, and ignores the rest of the line up to its end: the next
/// read is the next line. Every expected text is what sbl -bf printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class InputRecordLength
{
    private static string TempFile(string text)
    {
        var path = Path.Combine(Path.GetTempPath(), "irl_" + Guid.NewGuid().ToString("N")[..8] + ".txt");
        File.WriteAllText(path, text);
        return path;
    }

    private static string Records(string spec, string text)
    {
        var path = TempFile(text);
        var s = $"        INPUT(.W, 4, '{path}{spec}')\nL       Y = W                       :F(END)\n" +
                "        OUTPUT = SIZE(Y) ' ' SUBSTR(Y, 1, 3)   :(L)\nEND";
        return SetupTests.RunWithInput(s);
    }

    [TestMethod]
    public void TEST_InputRecordLength_a_record_of_standard_input_is_cut_at_1024_and_the_rest_is_ignored()
    {
        var s = "        X = INPUT\n        OUTPUT = SIZE(X)\n        Y = INPUT                    :S(TWO)\n        OUTPUT = 'eof'               :(END)\n" +
                "TWO     OUTPUT = 'second ' SIZE(Y)\nEND";
        Assert.AreEqual("1024\neof", SetupTests.RunWithInput(s, new string('z', 2048)));
    }

    [TestMethod]
    public void TEST_InputRecordLength_a_record_of_a_file_is_cut_at_1024_and_the_next_read_is_the_next_line()
    {
        Assert.AreEqual("1024 AAA\n5 sho\n1024 BBB",
            Records("", new string('A', 2048) + "\nshort\n" + new string('B', 1500) + "\n"));
    }

    [TestMethod]
    public void TEST_InputRecordLength_a_record_of_exactly_1024_is_whole()
    {
        Assert.AreEqual("1024 AAA\n3 xyz", Records("", new string('A', 1024) + "\nxyz\n"));
    }

    [TestMethod]
    public void TEST_InputRecordLength_the_L_option_sets_the_longest_record()
    {
        Assert.AreEqual("80 AAA\n5 sho\n30 BBB", Records("[-L80]", new string('A', 200) + "\nshort\n" + new string('B', 30) + "\n"));
    }

    [TestMethod]
    public void TEST_InputRecordLength_the_L_option_may_follow_a_blank_and_be_lower_case()
    {
        Assert.AreEqual("7 AAA\n5 sho", Records(" -l7", new string('A', 200) + "\nshort\n"));
    }
}
