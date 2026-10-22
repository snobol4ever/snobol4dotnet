using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// A pattern replacement whose subject is OUTPUT assigns OUTPUT, so it writes the new text, as in SPITBOL: OUTPUT keeps the last text it
/// was given, and the replacement is made in it. The console lost OUTPUT's association at the replacement, and every OUTPUT after it
/// printed nothing. The expectations are what sbl -bf printed for the same programs.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ReplaceIntoOutput
{
    private static string Run(string statement) =>
        SetupTests.RunWithInput($"        OUTPUT = '@a'\n{statement}\n        OUTPUT = '@b'\nEND").Replace("\r\n", "\n");

    [TestMethod]
    public void TEST_ReplaceIntoOutput_a_null_replacement_writes_the_text_again()
    {
        Assert.AreEqual("@a\n@a\n@b", Run("        OUTPUT e = phrase"));
    }

    [TestMethod]
    public void TEST_ReplaceIntoOutput_a_replacement_of_text_writes_the_new_text()
    {
        Assert.AreEqual("@a\n@b\n@b", Run("        OUTPUT 'a' = 'b'"));
        Assert.AreEqual("@a\n@b\n@b", Run("        OUTPUT ? 'a' = 'b'"));
    }

    [TestMethod]
    public void TEST_ReplaceIntoOutput_text_put_before_the_old_text()
    {
        Assert.AreEqual("@a\nCurrent Directory Path: @a\n@b", Run("        OUTPUT X = 'Current Directory Path: ' sDirPath"));
    }

    [TestMethod]
    public void TEST_ReplaceIntoOutput_a_match_that_fails_writes_nothing_and_output_goes_on()
    {
        Assert.AreEqual("@a\n@b", Run("        OUTPUT 'z' = 'b'"));
    }
}
