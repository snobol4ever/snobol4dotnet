using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SPITBOL lists a line and compiles it as it reads, so its errors stand among the statements in the order of the source, an
/// -INCLUDE is listed before the lines it brings in, and the lines of an included file carry the depth of the include in the sixth
/// column of the number field. A failed -INCLUDE stays open in its source stack: what follows lists at depth 1, its END is listed
/// only when a card follows it, and the run ends "No END statement found in source file(s)." on standard error. The expected texts are
/// what sbl -bf printed for the same files with the streams merged (2&gt;&amp;1), with the head's identity and clock masked; each case
/// runs from a folder of its own, the program named main.sno as given.
/// </summary>
[DoNotParallelize]
[TestClass]
public class IncludeInSourceOrder
{
    internal static string Run(Dictionary<string, string> files)
    {
        var dir = Path.Combine(Path.GetTempPath(), "iso_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        foreach (var (name, text) in files)
            File.WriteAllText(Path.Combine(dir, name), text);

        var oldOut = Console.Out;
        var oldError = Console.Error;
        var cwd = Directory.GetCurrentDirectory();
        using var both = new StringWriter();
        var merged = TextWriter.Synchronized(both);
        try
        {
            Directory.SetCurrentDirectory(dir);
            Console.SetOut(merged);
            Console.SetError(merged);
            Builder builder = new();
            builder.ParseCommandLine(["-bf", "main.sno"]);
            builder.Code.ReadAllFiles();
            builder.BuildMain();
        }
        finally
        {
            Console.SetOut(oldOut);
            Console.SetError(oldError);
            Directory.SetCurrentDirectory(cwd);
        }

        return SetupTests.MaskOracle(both.ToString()).TrimEnd();
    }

    [TestMethod]
    public void TEST_IncludeInSourceOrder_a_missing_include_lists_in_source_order_with_the_statements_around_it()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "        X = (1 +\n-INCLUDE 'nosuch.sno'\n        Y = (2 +\n        OUTPUT = 'x'\nEND\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X = (1 +\n1               X = (1 +\n                       !\n                       !\nmain.sno(1,16) : ERROR 221 -- syntax error: missing operand\nmain.sno(1,16) : ERROR 221 -- syntax error: missing operand\n\n\n        -INCLUDE 'nosuch.sno'\n        -INCLUDE 'nosuch.sno'\n                 !\n                 !\nmain.sno(2,10) : ERROR 285 -- include file cannot be opened\nmain.sno(2,10) : ERROR 285 -- include file cannot be opened\n\n\n2    1          Y = (2 +\n2    1          Y = (2 +\n                       !\n                       !\nmain.sno(3,16) : ERROR 221 -- syntax error: missing operand\nmain.sno(3,16) : ERROR 221 -- syntax error: missing operand\n\n\nNo END statement found in source file(s).", Run(files));
    }

    [TestMethod]
    public void TEST_IncludeInSourceOrder_a_missing_include_before_the_first_statement()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "-INCLUDE 'nosuch.sno'\n        X = (1 +\n        OUTPUT = 'x'\nEND\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -INCLUDE 'nosuch.sno'\n        -INCLUDE 'nosuch.sno'\n                 !\n                 !\nmain.sno(1,10) : ERROR 285 -- include file cannot be opened\nmain.sno(1,10) : ERROR 285 -- include file cannot be opened\n\n\n1    1          X = (1 +\n1    1          X = (1 +\n                       !\n                       !\nmain.sno(2,16) : ERROR 221 -- syntax error: missing operand\nmain.sno(2,16) : ERROR 221 -- syntax error: missing operand\n\n\nNo END statement found in source file(s).", Run(files));
    }

    [TestMethod]
    public void TEST_IncludeInSourceOrder_a_missing_include_under_list_stands_between_the_listed_lines()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "-LIST\n        X = 1\n-INCLUDE 'nosuch.sno'\n        Y = 2\n        OUTPUT = 'x'\nEND\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -LIST\n1               X = 1\n        -INCLUDE 'nosuch.sno'\n        -INCLUDE 'nosuch.sno'\n                 !\n                 !\nmain.sno(3,10) : ERROR 285 -- include file cannot be opened\nmain.sno(3,10) : ERROR 285 -- include file cannot be opened\n\n\n2    1          Y = 2\n3    1          OUTPUT = 'x'\nNo END statement found in source file(s).", Run(files));
    }

    // sbl lists a line when it reads the next: after a failed include the END that is the last line of the file is never listed, and an
    // END that has a card after it, a blank or a data line too, is, with its depth and its text (sbl -bf, streams merged)
    [TestMethod]
    public void TEST_IncludeInSourceOrder_an_end_with_a_card_after_it_is_listed_after_a_failed_include()
    {
        foreach (var after in new[] { "\n", "data\n" })
        {
            var files = new Dictionary<string, string> { ["main.sno"] = "-LIST\n-INCLUDE 'nosuch.sno'\n        X = 1\n        OUTPUT = 'x'\nEND\n" + after };
            Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -LIST\n        -INCLUDE 'nosuch.sno'\n        -INCLUDE 'nosuch.sno'\n                 !\n                 !\nmain.sno(2,10) : ERROR 285 -- include file cannot be opened\nmain.sno(2,10) : ERROR 285 -- include file cannot be opened\n\n\n1    1          X = 1\n2    1          OUTPUT = 'x'\n3    1  END", Run(files), after);
        }
    }

    [TestMethod]
    public void TEST_IncludeInSourceOrder_an_error_in_an_included_file_carries_the_include_depth()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "        X = (1 +\n-INCLUDE 'inc1.sno'\n        Z = (2 +\n        OUTPUT = 'x'\nEND\n", ["inc1.sno"] = "        Y = (3 +\n        OUTPUT = 'inc'\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X = (1 +\n1               X = (1 +\n                       !\n                       !\nmain.sno(1,16) : ERROR 221 -- syntax error: missing operand\nmain.sno(1,16) : ERROR 221 -- syntax error: missing operand\n\n\n2    1          Y = (3 +\n2    1          Y = (3 +\n                       !\n                       !\ninc1.sno(1,16) : ERROR 221 -- syntax error: missing operand\ninc1.sno(1,16) : ERROR 221 -- syntax error: missing operand\n\n\n4               Z = (2 +\n4               Z = (2 +\n                       !\n                       !\nmain.sno(3,16) : ERROR 221 -- syntax error: missing operand\nmain.sno(3,16) : ERROR 221 -- syntax error: missing operand", Run(files));
    }

    [TestMethod]
    public void TEST_IncludeInSourceOrder_list_lists_an_include_before_its_lines_and_numbers_them_with_the_depth()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "-LIST\n        X = 1\n-INCLUDE 'inc1.sno'\n        Y = 2\n        OUTPUT = 'done'\nEND\n", ["inc1.sno"] = "* a comment in the include\n        A = 1\n-NOLIST\n        B = 2\n-LIST\n        C = 3\n-INCLUDE 'inc2.sno'\n", ["inc2.sno"] = "        D = 4\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -LIST\n1               X = 1\n        -INCLUDE 'inc1.sno'\n     1  * a comment in the include\n2    1          A = 1\n     1  -LIST\n4    1          C = 3\n     1  -INCLUDE 'inc2.sno'\n5    2          D = 4\n6               Y = 2\n7               OUTPUT = 'done'\n8       END\ndone", Run(files));
    }
}
