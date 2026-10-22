using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A run that does not get as far as running the program exits as sbl does, which the console returns from Builder.ExitStatus:
/// 231 when execution is suppressed (a statement in error, or -n), and 1 when an -INCLUDE cannot be opened and END ends the file,
/// whatever -k says (a card after END lets the compile end there: 231, and -k runs the program).
/// A program that runs leaves the exit to &amp;CODE. Every status below is what sbl -bf exited with for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ExitStatus
{
    private static int? Status(string directives, string script)
    {
        var old = Console.Out;
        Console.SetOut(TextWriter.Null);
        try
        {
            return SetupTests.SetupScript(directives, script).ExitStatus;
        }
        finally
        {
            Console.SetOut(old);
        }
    }

    [TestMethod]
    public void TEST_ExitStatus_a_statement_in_error_exits_231()
    {
        Assert.AreEqual(231, Status("-b", "        X = (1 +\n        OUTPUT = 'x'\nEND"));
    }

    [TestMethod]
    public void TEST_ExitStatus_a_constant_that_errs_when_compiled_exits_231()
    {
        Assert.AreEqual(231, Status("-b", "        X = 1 / 0\nEND"));
    }

    [TestMethod]
    public void TEST_ExitStatus_no_execute_exits_231_though_nothing_is_in_error()
    {
        Assert.AreEqual(231, Status("-b -n", "        OUTPUT = 'x'\nEND"));
    }

    [TestMethod]
    public void TEST_ExitStatus_a_program_that_runs_leaves_the_exit_to_code()
    {
        Assert.IsNull(Status("-b", "        OUTPUT = 'x'\nEND"));
    }

    [TestMethod]
    public void TEST_ExitStatus_a_statement_in_error_run_under_k_leaves_the_exit_to_code()
    {
        Assert.IsNull(Status("-b -k", "        X = (1 +\n        OUTPUT = 'x'\nEND"));
    }

    [TestMethod]
    public void TEST_ExitStatus_a_missing_include_exits_1()
    {
        Assert.AreEqual(1, Status("-b", "-INCLUDE 'nosuchfile.sno'\n        OUTPUT = 'x'\nEND"));
    }

    [TestMethod]
    public void TEST_ExitStatus_a_missing_include_exits_1_under_k_too_and_does_not_run()
    {
        Assert.AreEqual(1, Status("-b -k", "-INCLUDE 'nosuchfile.sno'\n        OUTPUT = 'x'\nEND"));
    }

    // sbl's source stack, still inside the include that failed, ends the file at END only when END is its last line; a card after it, a
    // blank one too, is read, the compile ends at END, and the include is an error among the statements' (sbl -bf and -bf -k)
    [TestMethod]
    public void TEST_ExitStatus_a_missing_include_with_a_card_after_end_is_a_statement_in_error()
    {
        foreach (var after in new[] { "\n", "data\n", "   \n" })
        {
            var script = "-INCLUDE 'nosuchfile.sno'\n        OUTPUT = 'x'\nEND\n" + after;
            Assert.AreEqual(231, Status("-b", script), after);
            var written = new StringWriter();
            var old = Console.Out;
            Console.SetOut(written);
            try
            {
                Assert.IsNull(SetupTests.SetupScript("-b -k", script).ExitStatus, after);
            }
            finally
            {
                Console.SetOut(old);
            }

            StringAssert.Contains(written.ToString(), "x", after);
        }
    }
}
