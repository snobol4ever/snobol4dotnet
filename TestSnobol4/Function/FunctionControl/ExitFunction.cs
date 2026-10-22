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

    [TestMethod]
    public void TEST_ExitFunction_every_negative_number_writes_a_save_file_and_only_minus_4_goes_on()
    {
        // sbl -b: any negative integer writes the file; -4 goes on, all the others end the run. A real is cut toward zero, and a
        // string that reads as a number is that number (a blank around it does not matter); zero and above are error 105
        var folder = Path.Combine(Path.GetTempPath(), "exit_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(folder);
        try
        {
            var cases = new (string First, string Output)[]
            {
                ("-1", ""), ("-2", ""), ("-5", ""), ("-100", ""), ("-4", "after"), ("-3.9", ""), ("-4.9", "after"), ("-4.0", "after"),
                ("'-3'", ""), ("'-4'", "after"), ("' -3'", ""), ("'-3 '", ""), ("'-3.0'", ""), ("'-4.9'", "after"), ("'-1e0'", ""),
                ("'3'", "105"), ("'+3'", "105"), ("'-0'", "105"), ("-0.5", "105"), ("'-.5'", "105"), ("0", "105")
            };
            foreach (var (first, expected) in cases)
            {
                var file = Path.Combine(folder, "f" + Guid.NewGuid().ToString("N")[..6]);
                var s = $"        &ERRLIMIT = 5\n        EXIT({first}, '{file}')\n        OUTPUT = IDENT(&ERRTYPE, 105) '105'\n        OUTPUT = DIFFER(&ERRTYPE, 105) 'after'\nEND";
                Assert.AreEqual(expected, SetupTests.RunWithInput(s), $"EXIT({first})");
                Assert.AreEqual(expected is "" or "after", File.Exists(file), $"EXIT({first}) file");
            }
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [TestMethod]
    public void TEST_ExitFunction_a_second_argument_that_is_no_string_is_error_288()
    {
        foreach (var second in new[] { "ARRAY(2)", "TABLE()", "ARB", "CODE(' X = 1')", "*X" })
        foreach (var first in new[] { "-4", "-3", "3", "'echo hi'", "'-3x'", "''" })
        {
            var s = $"        &ERRLIMIT = 5\n        EXIT({first}, {second})\n        OUTPUT = 'after ' &ERRTYPE\nEND";
            Assert.AreEqual("after 288", SetupTests.RunWithInput(s), $"{first}, {second}");
        }
    }

    [TestMethod]
    public void TEST_ExitFunction_an_exit_that_ends_the_run_prints_no_dump_and_no_statistics()
    {
        // sbl -b: EXIT(-3) and EXIT(command) end the run without the end-of-run dump or the statistics of -x; EXIT(-4) goes on to them
        var folder = Path.Combine(Path.GetTempPath(), "exit_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(folder);
        try
        {
            var ending = $"        &DUMP = 2\n        OUTPUT = 'before'\n        EXIT(-3, '{folder}/saved')\n        OUTPUT = 'after'\nEND";
            Assert.AreEqual("before", SetupTests.RunWithInput(ending));
            Assert.AreEqual("before", SetupTests.RunWithInputDirectives("-b -x", ending));
            if (!OperatingSystem.IsWindows())
                Assert.AreEqual("before", SetupTests.RunWithInput("        &DUMP = 2\n        OUTPUT = 'before'\n        EXIT('true')\n        OUTPUT = 'after'\nEND"));
            var going = $"        &DUMP = 1\n        OUTPUT = 'before'\n        EXIT(-4, '{folder}/saved')\n        OUTPUT = 'after'\nEND";
            StringAssert.Contains(SetupTests.RunWithInput(going).Replace("\r\n", "\n"), "before\nafter\n\n\n\ndump of natural variables");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
