using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// A variable with an input or output association is a target like any other: a cursor position given to OUTPUT by @OUTPUT is written,
/// as an assignment is, and OUTPUT stays associated (the console lost the association and wrote nothing, then or after); and the name
/// after . or $ is wanted and not its value, so X . TERMINAL reads no line from the terminal (it read one, took it for a name and
/// failed with error 212). Every expectation is what sbl -bf printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AssociatedTargets
{
    private static string Run(string statement, string? input = null) =>
        SetupTests.RunWithInput($"        OUTPUT = '@a'\n{statement}\n        OUTPUT = '@b'\nEND", input).Replace("\r\n", "\n");

    [TestMethod]
    public void TEST_AssociatedTargets_a_cursor_given_to_output_is_written()
    {
        Assert.AreEqual("@a\n0\n@b", Run("        'abc' ? @OUTPUT"));
        Assert.AreEqual("@a\n1\n@b", Run("        'abc' ? LEN(1) @OUTPUT"));
        Assert.AreEqual("@a\n1\n2\n@b", Run("        'abc' ? LEN(1) @OUTPUT LEN(1) @OUTPUT"));
    }

    [TestMethod]
    public void TEST_AssociatedTargets_a_cursor_is_written_at_each_try_of_a_backtracking_match()
    {
        Assert.AreEqual("@a\n0\n1\n2\n3\n1\n2\n3\n2\n3\n3\n@b", Run("        'abc' ? ARB @OUTPUT FAIL"));
        Assert.AreEqual("@a\n1\n2\n3\n@b", Run("        'abc' ? (LEN(1) @OUTPUT) FAIL"));
    }

    [TestMethod]
    public void TEST_AssociatedTargets_a_cursor_is_not_written_while_output_is_suspended()
    {
        Assert.AreEqual("@a\n@b", Run("        &OUTPUT = 0\n        'abc' ? LEN(1) @OUTPUT\n        &OUTPUT = 1"));
    }

    [TestMethod]
    public void TEST_AssociatedTargets_the_target_of_a_conditional_or_immediate_assignment_reads_no_line()
    {
        Assert.AreEqual("@a\n@b\ninput=line1", SetupTests.RunWithInput(
            "        OUTPUT = '@a'\n        'abc' ? LEN(1) . TERMINAL\n        OUTPUT = '@b'\n        X = INPUT\n        OUTPUT = 'input=' X\nEND",
            "line1\nline2").Replace("\r\n", "\n"));
        Assert.AreEqual("@a\n@b\ninput=line1", SetupTests.RunWithInput(
            "        OUTPUT = '@a'\n        'abc' ? LEN(1) $ TERMINAL\n        OUTPUT = '@b'\n        X = INPUT\n        OUTPUT = 'input=' X\nEND",
            "line1\nline2").Replace("\r\n", "\n"));
    }
}
