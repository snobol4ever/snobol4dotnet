using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// EXIT as SPITBOL's does. EXIT(s) ends the run and has the operating system perform the command s, through the shell, with the run's
/// exit status the command's (this run cannot start a program of its own: it failed on Linux, which has no cmd.exe). EXIT(-3, file)
/// writes a save file and ends the run, EXIT(-4, file) writes it and goes on; the file here holds the program, not its state. Any
/// other first argument, a load module (EXIT(3)) included, is error 105. Every expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ExitFunction
{
    private static string TempPath() => Path.Combine(Path.GetTempPath(), "exit_" + Guid.NewGuid().ToString("N")[..8] + ".spx");

    [TestMethod]
    public void TEST_ExitFunction_minus_3_writes_a_save_file_and_ends_the_run()
    {
        var path = TempPath();
        var s = $"        OUTPUT = 'before'\n        EXIT(-3, '{path}')\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual("before", SetupTests.RunWithInput(s));
        Assert.IsTrue(File.Exists(path));
    }

    [TestMethod]
    public void TEST_ExitFunction_minus_4_writes_a_save_file_and_goes_on()
    {
        var path = TempPath();
        var s = $"        OUTPUT = 'before'\n        EXIT(-4, '{path}')\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual("before\nafter", SetupTests.RunWithInput(s));
        Assert.IsTrue(File.Exists(path));
    }

    [TestMethod]
    public void TEST_ExitFunction_other_first_arguments_are_error_105()
    {
        foreach (var call in new[] { "EXIT(3, 'x3.out')", "EXIT(5, 'z')", "EXIT(1.5)", "EXIT()" })
        {
            var s = $"        &ERRLIMIT = 5\n        OUTPUT = 'before'\n        {call}\n        OUTPUT = 'after ' &ERRTYPE\nEND";
            Assert.AreEqual("before\nafter 105", SetupTests.RunWithInput(s), call);
        }
    }

    [TestMethod]
    public void TEST_ExitFunction_a_command_is_run_by_the_shell_and_its_status_is_the_runs()
    {
        if (OperatingSystem.IsWindows())
            return;
        var s = "        OUTPUT = 'before'\n        EXIT('exit 7')\n        OUTPUT = 'after'\nEND";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(7, build.ExitStatus);
    }

    [TestMethod]
    public void TEST_ExitFunction_a_save_file_may_have_any_name()
    {
        // sbl -b writes a file named as given, however odd: a blank, a tab, a name that is all extension, a name with a NUL (cut at it)
        var folder = Path.Combine(Path.GetTempPath(), "exit_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(folder);
        try
        {
            foreach (var (name, written) in new[] { (" ", " "), ("  ", "  "), ("\t", "\t"), (".spx", ".spx"), (" lead", " lead"), ("a  b", "a  b") })
            {
                var s = $"        EXIT(-4, '{folder}/{name}')\n        OUTPUT = 'after'\nEND";
                Assert.AreEqual("after", SetupTests.RunWithInput(s), $"[{name}]");
                Assert.IsTrue(File.Exists(Path.Combine(folder, written)), $"[{name}] written");
            }

            var cut = $"        EXIT(-4, '{folder}/x' CHAR(0) 'y')\n        OUTPUT = 'after'\nEND";
            Assert.AreEqual("after", SetupTests.RunWithInput(cut));
            Assert.IsTrue(File.Exists(Path.Combine(folder, "x")));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [TestMethod]
    public void TEST_ExitFunction_an_unwritable_save_file_is_error_106_and_says_nothing_else()
    {
        // sbl -b: the report is all there is; the console added a warning on standard error of its own
        var s = "        EXIT(-4, '/nonexistent/dir/x')\n        OUTPUT = 'after'\nEND";
        var error = new StringWriter();
        var old = Console.Error;
        Console.SetError(error);
        string output;
        try
        {
            output = SetupTests.RunWithInput(s);
        }
        finally
        {
            Console.SetError(old);
        }

        StringAssert.Contains(output, "e.sno(1) : ERROR 106 -- exit action caused irrecoverable error".Replace("e.sno", "Test.sno"));
        Assert.IsFalse(error.ToString().Contains("could not write"), error.ToString());
    }
}
