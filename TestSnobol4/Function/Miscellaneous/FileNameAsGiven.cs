using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SPITBOL names a source file as it was named: &amp;FILE, the error line and "in file" of a fatal report carry the path as it was
/// given on the command line, not its last part (sbl -bf, run with a full path, prints the full path). The harness hands both engines
/// the same argument and compares the text. The tests run a real file through the command line.
/// </summary>
[DoNotParallelize]
[TestClass]
public class FileNameAsGiven
{
    private static string Run(string path, string source)
    {
        File.WriteAllText(path, source);
        var old = Console.Out;
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms) { AutoFlush = true };
        Console.SetOut(sw);
        var cwd = Directory.GetCurrentDirectory();
        try
        {
            Builder builder = new();
            builder.ParseCommandLine(["-bf", path]);
            builder.Code.ReadAllFiles();
            builder.BuildMain();
        }
        finally
        {
            Console.SetOut(old);
            Directory.SetCurrentDirectory(cwd);
        }

        ms.Position = 0;
        return SetupTests.MaskReport(new StreamReader(ms).ReadToEnd()).Replace("\r\n", "\n").TrimEnd();
    }

    // sbl hard-wraps every line of the report at 120 columns, mid-word
    private static string Wrap(string line)
    {
        var parts = new List<string>();
        for (; line.Length > 120; line = line[120..])
            parts.Add(line[..120]);
        parts.Add(line);
        return string.Join("\n", parts);
    }

    private static string TempProgram()
    {
        var dir = Path.Combine(Path.GetTempPath(), "fn_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "prog.sno");
    }

    [TestMethod]
    public void TEST_FileNameAsGiven_the_file_keyword_names_the_path_as_given()
    {
        var path = TempProgram();
        try
        {
            Assert.AreEqual(path, Run(path, "        OUTPUT = &FILE\nEND\n"));
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }

    [TestMethod]
    public void TEST_FileNameAsGiven_a_fatal_report_names_the_path_as_given()
    {
        var path = TempProgram();
        try
        {
            var expected =
                "\n\n\n" + Wrap($"{path}(1) : ERROR 022 -- undefined function called") + "\n\n\n\n\n\n" +
                Wrap($"{"in file",-21}{path}") + "\nin line              1\nin statement         1\nstmts executed       1\n" +
                "execution time msec  -\nREGENERATIONS        -\nmemory used (bytes)  -\nmemory left (bytes)  -";
            Assert.AreEqual(expected, Run(path, "        X = FOO(1)\nEND\n"));
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, true);
        }
    }
}
