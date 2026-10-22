using Test.TestLexer;

namespace Test.Compilation;

/// <summary>
/// "No END statement found in source file(s)." is sbl's last word: it comes after the listing and the errors of the statements that were
/// compiled, not before them (it was written as soon as the missing END was seen).
/// </summary>
[DoNotParallelize]
[TestClass]
public class NoEndLast
{
    [TestMethod]
    public void TEST_NoEndLast_the_missing_end_is_reported_after_the_errors()
    {
        var merged = new StringWriter();
        var (output, error) = (Console.Out, Console.Error);
        Console.SetOut(merged);
        Console.SetError(merged);
        try
        {
            SetupTests.SetupScript("-b", "        A = 1\n        B = (\n        C = 2\n");
        }
        finally
        {
            Console.SetOut(output);
            Console.SetError(error);
        }

        var lines = merged.ToString().Replace("\r\n", "\n").Split('\n').Where(line => line.Trim() != "").ToArray();
        Assert.AreEqual("No END statement found in source file(s).", lines[^1]);
        Assert.IsTrue(lines.Any(line => line.Contains("ERROR 226")), "the error of the statement that was compiled is reported");
        Assert.AreEqual(1, lines.Count(line => line == "No END statement found in source file(s)."));
    }

    [TestMethod]
    public void TEST_NoEndLast_an_include_that_cannot_be_opened_and_a_missing_end_say_it_once()
    {
        var merged = new StringWriter();
        var (output, error) = (Console.Out, Console.Error);
        Console.SetOut(merged);
        Console.SetError(merged);
        try
        {
            SetupTests.SetupScript("-b", "-INCLUDE 'no-such-file.sno'\n        A = 1\n");
        }
        finally
        {
            Console.SetOut(output);
            Console.SetError(error);
        }

        var lines = merged.ToString().Replace("\r\n", "\n").Split('\n').Where(line => line.Trim() != "").ToArray();
        Assert.AreEqual("No END statement found in source file(s).", lines[^1]);
        Assert.AreEqual(1, lines.Count(line => line == "No END statement found in source file(s)."));
    }
}
