using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// The listing and statistics switches as SPITBOL's: -l (and -a, -p, -z) lists the source to the first source file's name with .lst
/// for its extension, or to the -o file, and leaves standard output to the program; -c and -x print their blocks on the listing
/// stream, in sbl's layout; -h leaves the identity and clock out of the head; -g sets the page length and -t the page width; -p
/// and -z break pages with form feeds; -e leaves an empty line before the pointer of an error; -k counts the statement in error as
/// executed. Every expected text is what sbl printed for the same file and switches, with the head's identity and clock, the run's
/// figures and the statistics-rate lines masked; each case runs from a folder of its own.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ListingSwitches
{
    private static readonly System.Text.RegularExpressions.Regex Identity = new(@"(snobol4dotnet|macro spitbol) version [0-9][0-9.a-z]*");
    private static readonly System.Text.RegularExpressions.Regex Clock = new(@"\S+  [A-Z][a-z]{2} [A-Z][a-z]{2} [ \d]\d \d{2}:\d{2}:\d{2} \d{4}");
    private static readonly System.Text.RegularExpressions.Regex Volatile = new(
        @"^(execution time msec|REGENERATIONS|memory used \(bytes\)|memory left \(bytes\)|comp time \(microsec\))( +)\d+",
        System.Text.RegularExpressions.RegexOptions.Multiline);
    private static readonly System.Text.RegularExpressions.Regex Rates = new(@"^stmt / (microsec|millisec|second) +\d+\n",
        System.Text.RegularExpressions.RegexOptions.Multiline);

    private static string Mask(string text) =>
        Rates.Replace(Volatile.Replace(Clock.Replace(Identity.Replace(text.Replace("\r\n", "\n"), "<identity>"), "<clock>"), "$1$2-"), "");

    // Runs the program in a folder of its own with the switches; the masked standard output, standard error and listing files
    private static (string Out, string Error, SortedDictionary<string, string> Files) Run(string program, string text, string switches)
    {
        var dir = Path.Combine(Path.GetTempPath(), "lsw_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, program), text);
        var oldOut = Console.Out;
        var oldError = Console.Error;
        var cwd = Directory.GetCurrentDirectory();
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        try
        {
            Directory.SetCurrentDirectory(dir);
            Console.SetOut(stdout);
            Console.SetError(stderr);
            Builder builder = new();
            builder.ParseCommandLine([.. switches.Split(' '), program]);
            builder.Code.ReadAllFiles();
            builder.BuildMain();
            builder.Dispose();
        }
        finally
        {
            Console.SetOut(oldOut);
            Console.SetError(oldError);
            Directory.SetCurrentDirectory(cwd);
        }

        var files = new SortedDictionary<string, string>();
        foreach (var file in Directory.GetFiles(dir, "*.lst"))
            files[Path.GetFileName(file)] = Mask(File.ReadAllText(file));
        return (Mask(stdout.ToString()), Mask(stderr.ToString()), files);
    }

    private static void Check(string program, string text, string switches, string expectedOut, string expectedError,
        params (string Name, string Text)[] expectedFiles)
    {
        var (output, error, files) = Run(program, text, switches);
        Assert.AreEqual(expectedOut, output, "standard output");
        Assert.AreEqual(expectedError, error, "standard error");
        CollectionAssert.AreEqual(expectedFiles.Select(f => f.Name).ToList(), files.Keys.ToList(), "listing files");
        foreach (var (name, expected) in expectedFiles)
            Assert.AreEqual(expected, files[name], name);
    }

    [TestMethod]
    public void TEST_ListingSwitches_l_lists_the_source_to_the_lst_file_and_leaves_standard_output_to_the_program()
    {
        Check("prog.sno", "        X = 1\n        OUTPUT = 'out ' X\nEND\n", "-bfl",
            "out 1\n",
            "",
            ("prog.lst", "\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X = 1\n2               OUTPUT = 'out ' X\n3       END\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_c_prints_the_compiler_statistics_on_the_listing_stream()
    {
        Check("prog.sno", "        X = 1\n        OUTPUT = 'out ' X\nEND\n", "-bfc",
            "\n\n\n<identity>\n<clock>\n\n\n\nmemory used (bytes)  -\nmemory left (bytes)  -\ncomp errors          0\nREGENERATIONS        -\ncomp time (microsec) -\n\n\n\nout 1\n",
            "");
    }

    [TestMethod]
    public void TEST_ListingSwitches_x_prints_the_end_of_run_statistics_after_the_program()
    {
        Check("prog.sno", "        X = 1\n        OUTPUT = 'out ' X\nEND\n", "-bfx",
            "out 1\n\n\n\nnormal end\nin file              prog.sno\nin line              3\nin statement         3\nstmts executed       3\nexecution time msec  -\nREGENERATIONS        -\nmemory used (bytes)  -\nmemory left (bytes)  -\n\n",
            "");
    }

    [TestMethod]
    public void TEST_ListingSwitches_a_is_l_c_and_x()
    {
        Check("prog.sno", "        X = 1\n        OUTPUT = 'out ' X\nEND\n", "-bfa",
            "out 1\n",
            "",
            ("prog.lst", "\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X = 1\n2               OUTPUT = 'out ' X\n3       END\n\n\n\nmemory used (bytes)  -\nmemory left (bytes)  -\ncomp errors          0\nREGENERATIONS        -\ncomp time (microsec) -\n\n\n\n\n\n\nnormal end\nin file              prog.sno\nin line              3\nin statement         3\nstmts executed       3\nexecution time msec  -\nREGENERATIONS        -\nmemory used (bytes)  -\nmemory left (bytes)  -\n\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_o_names_the_listing_file_for_l()
    {
        Check("prog.sno", "        X = 1\n        OUTPUT = 'out ' X\nEND\n", "-bfl -o=named.lst",
            "out 1\n",
            "",
            ("named.lst", "\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X = 1\n2               OUTPUT = 'out ' X\n3       END\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_h_leaves_the_identity_and_clock_out_of_the_head()
    {
        Check("prog.sno", "        X = 1\n        OUTPUT = 'out ' X\nEND\n", "-bfhl",
            "out 1\n",
            "",
            ("prog.lst", "\n\n\n                                                                                                                page 1\n\n1               X = 1\n2               OUTPUT = 'out ' X\n3       END\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_g_sets_the_page_length_and_pages_break_with_three_blank_lines()
    {
        Check("long.sno", "        X1 = 1\n        X2 = 2\n        X3 = 3\n        X4 = 4\n        X5 = 5\n        X6 = 6\n        X7 = 7\n        X8 = 8\n        X9 = 9\n        X10 = 10\n        X11 = 11\n        X12 = 12\n        X13 = 13\n        X14 = 14\n        X15 = 15\n        X16 = 16\n        X17 = 17\n        X18 = 18\n        X19 = 19\n        X20 = 20\n        X21 = 21\n        X22 = 22\n        X23 = 23\n        X24 = 24\n        X25 = 25\n        X26 = 26\n        X27 = 27\n        X28 = 28\n        X29 = 29\n        X30 = 30\n        OUTPUT = 'x'\nEND\n", "-bfl -g20",
            "x\n",
            "",
            ("long.lst", "\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X1 = 1\n2               X2 = 2\n3               X3 = 3\n4               X4 = 4\n5               X5 = 5\n6               X6 = 6\n7               X7 = 7\n8               X8 = 8\n9               X9 = 9\n10              X10 = 10\n11              X11 = 11\n\n\n\n                                                                                                                page 2\n\n12              X12 = 12\n13              X13 = 13\n14              X14 = 14\n15              X15 = 15\n16              X16 = 16\n17              X17 = 17\n18              X18 = 18\n19              X19 = 19\n20              X20 = 20\n21              X21 = 21\n22              X22 = 22\n23              X23 = 23\n24              X24 = 24\n25              X25 = 25\n26              X26 = 26\n\n\n\n                                                                                                                page 3\n\n27              X27 = 27\n28              X28 = 28\n29              X29 = 29\n30              X30 = 30\n31              OUTPUT = 'x'\n32      END\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_p_breaks_pages_with_form_feeds()
    {
        Check("long.sno", "        X1 = 1\n        X2 = 2\n        X3 = 3\n        X4 = 4\n        X5 = 5\n        X6 = 6\n        X7 = 7\n        X8 = 8\n        X9 = 9\n        X10 = 10\n        X11 = 11\n        X12 = 12\n        X13 = 13\n        X14 = 14\n        X15 = 15\n        X16 = 16\n        X17 = 17\n        X18 = 18\n        X19 = 19\n        X20 = 20\n        X21 = 21\n        X22 = 22\n        X23 = 23\n        X24 = 24\n        X25 = 25\n        X26 = 26\n        X27 = 27\n        X28 = 28\n        X29 = 29\n        X30 = 30\n        OUTPUT = 'x'\nEND\n", "-bfpl -g20",
            "x\n",
            "",
            ("long.lst", "\f<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X1 = 1\n2               X2 = 2\n3               X3 = 3\n4               X4 = 4\n5               X5 = 5\n6               X6 = 6\n7               X7 = 7\n8               X8 = 8\n9               X9 = 9\n10              X10 = 10\n11              X11 = 11\n12              X12 = 12\n13              X13 = 13\n14              X14 = 14\n\f                                                                                                                page 2\n\n15              X15 = 15\n16              X16 = 16\n17              X17 = 17\n18              X18 = 18\n19              X19 = 19\n20              X20 = 20\n21              X21 = 21\n22              X22 = 22\n23              X23 = 23\n24              X24 = 24\n25              X25 = 25\n26              X26 = 26\n27              X27 = 27\n28              X28 = 28\n29              X29 = 29\n30              X30 = 30\n31              OUTPUT = 'x'\n32      END\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_t_sets_the_page_width()
    {
        Check("prog.sno", "        X = 1\n        OUTPUT = 'out ' X\nEND\n", "-bfl -t60",
            "out 1\n",
            "",
            ("prog.lst", "\n\n\n<identity>\n<clock>\n\n\n\n                                                    page 1\n\n1               X = 1\n2               OUTPUT = 'out ' X\n3       END\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_a_statement_in_error_is_listed_with_its_error_and_nothing_runs()
    {
        Check("prog.sno", "        X = 1\n        Y = (1 +\n        OUTPUT = 'x'\nEND\n", "-bfl",
            "",
            "2               Y = (1 +\n                       !\nprog.sno(2,16) : ERROR 221 -- syntax error: missing operand\n\n",
            ("prog.lst", "\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X = 1\n2               Y = (1 +\n                       !\nprog.sno(2,16) : ERROR 221 -- syntax error: missing operand\n\n3               OUTPUT = 'x'\n4       END\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_e_leaves_an_empty_line_before_the_pointer()
    {
        Check("prog.sno", "        X = 1\n        Y = (1 +\n        OUTPUT = 'x'\nEND\n", "-bfel",
            "",
            "",
            ("prog.lst", "\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X = 1\n2               Y = (1 +\n\n                       !\nprog.sno(2,16) : ERROR 221 -- syntax error: missing operand\n\n3               OUTPUT = 'x'\n4       END\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_a_fatal_error_carries_its_summary_in_the_listing_and_x_adds_nothing()
    {
        Check("prog.sno", "        X = 1\n        Z = 0\n        Y = X / Z\n        OUTPUT = 'x'\nEND\n", "-bfa",
            "",
            "prog.sno(3) : ERROR 014 -- division caused integer overflow\n\n\nin file              prog.sno\nin line              3\nin statement         3\nstmts executed       3\nexecution time msec  -\nREGENERATIONS        -\n\n",
            ("prog.lst", "\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n1               X = 1\n2               Z = 0\n3               Y = X / Z\n4               OUTPUT = 'x'\n5       END\n\n\n\nmemory used (bytes)  -\nmemory left (bytes)  -\ncomp errors          0\nREGENERATIONS        -\ncomp time (microsec) -\n\n\n\n\n\n\nprog.sno(3) : ERROR 014 -- division caused integer overflow\n\n\n\n\n\nin file              prog.sno\nin line              3\nin statement         3\nstmts executed       3\nexecution time msec  -\nREGENERATIONS        -\nmemory used (bytes)  -\nmemory left (bytes)  -\n\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_a_misplaced_continuation_follows_its_listed_line()
    {
        Check("prog.sno", "* c\n.       OUTPUT = 'x'\n        Y = (1 +\nEND\n", "-bfl",
            "",
            "1       .       OUTPUT = 'x'\n\nprog.sno(2) : ERROR 214 -- bad label or misplaced continuation line\n\n2               Y = (1 +\n                       !\nprog.sno(3,16) : ERROR 221 -- syntax error: missing operand\n\n",
            ("prog.lst", "\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        * c\n1       .       OUTPUT = 'x'\n\nprog.sno(2) : ERROR 214 -- bad label or misplaced continuation line\n\n2               Y = (1 +\n                       !\nprog.sno(3,16) : ERROR 221 -- syntax error: missing operand\n\n3       END\n"));
    }

    [TestMethod]
    public void TEST_ListingSwitches_k_counts_the_statement_in_error_as_executed()
    {
        Check("prog.sno", "        X = 1\n        Y = (1 +\n        OUTPUT = 'x'\nEND\n", "-bfk",
            "\n\n\n<identity>\n<clock>\n\n\n\npage 1\n\n2               Y = (1 +\n                       !\nprog.sno(2,16) : ERROR 221 -- syntax error: missing operand\n\n\n\n\nprog.sno(2) : ERROR 007 -- compilation error encountered during execution\n\n\n\n\n\nin file              prog.sno\nin line              2\nin statement         2\nstmts executed       2\nexecution time msec  -\nREGENERATIONS        -\nmemory used (bytes)  -\nmemory left (bytes)  -\n\n",
            "2               Y = (1 +\n                       !\nprog.sno(2,16) : ERROR 221 -- syntax error: missing operand\n\nprog.sno(2) : ERROR 007 -- compilation error encountered during execution\n\n\nin file              prog.sno\nin line              2\nin statement         2\nstmts executed       2\nexecution time msec  -\nREGENERATIONS        -\n\n");
    }
}
