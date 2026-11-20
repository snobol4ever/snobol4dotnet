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

    // sbl's source stack is still inside the include that failed, and says it only when END ends the file: a line after END, a blank
    // one too, is something to read, and it says nothing (the file shapes were run under sbl -bf)
    [TestMethod]
    public void TEST_NoEndLast_a_failed_include_says_it_only_when_end_ends_the_file()
    {
        foreach (var (after, said) in new[] { ("", true), ("\n", false), ("\n\n", false), ("   \n", false), ("data line\n", false) })
        {
            var merged = new StringWriter();
            var (output, error) = (Console.Out, Console.Error);
            Console.SetOut(merged);
            Console.SetError(merged);
            try
            {
                SetupTests.SetupScript("-b", "-INCLUDE 'no-such-file.sno'\n        A = 1\nEND\n" + after);
            }
            finally
            {
                Console.SetOut(output);
                Console.SetError(error);
            }

            var count = merged.ToString().Split('\n').Count(line => line.Trim() == "No END statement found in source file(s).");
            Assert.AreEqual(said ? 1 : 0, count, $"after END: {after.Replace("\n", "\\n")}");
        }
    }

    private static string[] CompileWithoutEnd(string source, string directives = "-b")
    {
        var merged = new StringWriter();
        var (output, error) = (Console.Out, Console.Error);
        Console.SetOut(merged);
        Console.SetError(merged);
        try
        {
            SetupTests.SetupScript(directives, source);
        }
        finally
        {
            Console.SetOut(output);
            Console.SetError(error);
        }

        return merged.ToString().Replace("\r\n", "\n").Split('\n').Where(line => line.Trim() != "").ToArray();
    }

    // sbl lists a line when it reads the next, so the last line of a source with no END is never listed; the page of an error in it
    // prints it (sbl -b -l): a statement's first line with its number, a continuation line with the number field blank
    [TestMethod]
    public void TEST_NoEndLast_the_page_of_an_error_in_the_last_line_prints_the_line()
    {
        var lines = CompileWithoutEnd("        OUTPUT = 1\n        X = 1 )\n", "-b -l");
        CollectionAssert.AreEqual(new[] { "2               X = 1 )", "                      !" }, lines.Take(2).ToArray());
        lines = CompileWithoutEnd("        OUTPUT = 1\n        X = 1 +\n+ 2 )\n", "-b -l");
        CollectionAssert.AreEqual(new[] { "        + 2 )", "            !" }, lines.Take(2).ToArray());
        lines = CompileWithoutEnd("        OUTPUT = 1\n:END\n", "-b -l");
        Assert.AreEqual("2       :END", lines[0]);
        Assert.AreEqual(1, lines.Count(line => line.Contains(":END")));
    }
}
