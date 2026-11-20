using Snobol4.Common;

namespace Test.Miscellaneous;

/// <summary>
/// A run whose source cannot be read ends before it starts, with one line on standard error and the status 1, as sbl -bf does:
/// "No END statement found in source file(s)." for source that holds no END, and "Can't open NAME" for a file that cannot be opened,
/// NAME as it was given on the command line. Nothing is written to standard output.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SourceNotRead
{
    private static (string Out, string Error, int? Status) Run(params string[] arguments)
    {
        var oldOut = Console.Out;
        var oldError = Console.Error;
        var cwd = Directory.GetCurrentDirectory();
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        Console.SetOut(stdout);
        Console.SetError(stderr);
        try
        {
            Builder builder = new();
            builder.ParseCommandLine(arguments);
            builder.Code.ReadAllFiles();
            builder.BuildMain();
            return (stdout.ToString(), stderr.ToString().Replace("\r\n", "\n"), builder.ExitStatus);
        }
        finally
        {
            Console.SetOut(oldOut);
            Console.SetError(oldError);
            Directory.SetCurrentDirectory(cwd);
        }
    }

    private static string TempProgram(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "snr_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "prog.sno");
        File.WriteAllText(path, source);
        return path;
    }

    [TestMethod]
    public void TEST_SourceNotRead_source_without_an_end_names_it_on_standard_error_and_exits_1()
    {
        var (output, error, status) = Run("-bf", TempProgram("        OUTPUT = 'x'\n"));
        Assert.AreEqual("", output);
        Assert.AreEqual("No END statement found in source file(s).\n", error);
        Assert.AreEqual(1, status);
    }

    [TestMethod]
    public void TEST_SourceNotRead_a_file_that_cannot_be_opened_is_named_as_given_and_exits_1()
    {
        var (output, error, status) = Run("-bf", "nosuch/dir/prog.sno");
        Assert.AreEqual("", output);
        Assert.AreEqual("Can't open nosuch/dir/prog.sno\n", error);
        Assert.AreEqual(1, status);
    }

    [TestMethod]
    public void TEST_SourceNotRead_a_second_file_that_cannot_be_opened_stops_the_run_too()
    {
        // the first file holds no END, so the next is read; sbl names it
        var (output, error, status) = Run("-bf", TempProgram("        OUTPUT = 'x'\n"), "nosuch2.sno");
        Assert.AreEqual("", output);
        Assert.AreEqual("Can't open nosuch2.sno\n", error);
        Assert.AreEqual(1, status);
    }

    [TestMethod]
    public void TEST_SourceNotRead_source_without_an_end_does_not_run_under_k()
    {
        var (output, _, status) = Run("-bfk", TempProgram("        OUTPUT = 'x'\n"));
        Assert.AreEqual("", output);
        Assert.AreEqual(1, status);
    }
}
