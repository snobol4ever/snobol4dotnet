using Snobol4.Common;

namespace Test.TestCommandLine;

/// <summary>
/// The words after the file that holds END are the program's arguments, not more source, and HOST answers
/// as SPITBOL does: HOST(0) the -u string or else those words, HOST(2,n) word n of the command line (failing
/// past the end), HOST(3) the index of the first word after the files, HOST(4,name) the environment
/// variable (failing when unset), HOST(1,cmd) the shell's wait status; any other selector is error 254.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ProgramArguments
{
    private static Builder Run(params string[] commandLine)
    {
        var builder = new Builder();
        builder.ParseCommandLine(commandLine);
        builder.Code.ReadAllFiles();   // as MainConsole does
        builder.BuildMain();
        return builder;
    }

    [TestMethod]
    public void TEST_ProgramArguments_and_host()
    {
        var dir = Path.Combine(Path.GetTempPath(), "snoargs_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "h.sno");
        File.WriteAllText(file, @"        H0 = HOST(0)
        H21 = HOST(2,1)
        H23 = HOST(2,3)
        H3 = HOST(3)
        H25 = HOST(2,5)                :S(BAD)
        HOME = HOST(4,'PATH')
        NONE = 'unset'
        NONE = HOST(4,'NO_SUCH_VARIABLE_XYZ')
        SHELL = HOST(1,'exit 3')
                                       :(END)
BAD     H25 = 'should fail'
END
");
        var oldCwd = Directory.GetCurrentDirectory();
        try
        {
            // sbl -b h.sno alpha beta: HOST(0) alpha beta, HOST(2,1) -b, HOST(2,3) alpha, HOST(3) 3,
            // HOST(2,5) fails, HOST(1,'exit 3') 768
            var plain = Run("-b", file, "alpha", "beta");
            var t = plain.Execute!.IdentifierTable;
            Assert.AreEqual(0, plain.ErrorCodeHistory.Count);
            Assert.AreEqual("alpha beta|-b|alpha|3|", $"{t["H0"]}|{t["H21"]}|{t["H23"]}|{t["H3"]}|{t["H25"]}");
            Assert.AreEqual(Environment.GetEnvironmentVariable("PATH"), t["HOME"].ToString());
            Assert.AreEqual("unset", t["NONE"].ToString());
            if (!OperatingSystem.IsWindows())
                Assert.AreEqual("768", t["SHELL"].ToString());

            // sbl -b -u ustr h.sno a1: HOST(0) ustr, HOST(3) 5
            var withU = Run("-b", "-u", "ustr", file, "a1");
            Assert.AreEqual("ustr|5", $"{withU.Execute!.IdentifierTable["H0"]}|{withU.Execute!.IdentifierTable["H3"]}");
        }
        finally
        {
            Directory.SetCurrentDirectory(oldCwd);
            Directory.Delete(dir, true);
        }
    }

    [TestMethod]
    public void TEST_ProgramArguments_relative_files_resolve_from_the_working_directory()
    {
        // sbl -b ../src/p.sno run from run/: the include and the output file both resolve in run/, not in the
        // source's directory
        var root = Path.Combine(Path.GetTempPath(), "snocwd_" + Guid.NewGuid().ToString("N"));
        var src = Path.Combine(root, "src");
        var run = Path.Combine(root, "run");
        Directory.CreateDirectory(src);
        Directory.CreateDirectory(run);
        File.WriteAllText(Path.Combine(src, "p.sno"), @"        OUTPUT('F', 7, 'out.txt')
        F = 'hello'
        ENDFILE(7)
-INCLUDE 'inc.inc'
END
");
        File.WriteAllText(Path.Combine(src, "inc.inc"), "        WHERE = 'src'\n");
        File.WriteAllText(Path.Combine(run, "inc.inc"), "        WHERE = 'run'\n");
        var oldCwd = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(run);
            var build = Run("-b", Path.Combine("..", "src", "p.sno"));
            Assert.AreEqual("run", build.Execute!.IdentifierTable["WHERE"].ToString());
            Assert.IsTrue(File.Exists(Path.Combine(run, "out.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(src, "out.txt")));
        }
        finally
        {
            Directory.SetCurrentDirectory(oldCwd);
            Directory.Delete(root, true);
        }
    }
}
