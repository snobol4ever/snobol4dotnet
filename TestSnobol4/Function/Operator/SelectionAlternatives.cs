using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// An empty alternative of a selection (A,B) is a null that succeeds, as in SPITBOL: (,'b') is the null string, and the alternatives
/// after it are not tried. The console gave the first, second and middle empty alternatives of a selection no value at all, so that
/// X = (,'b') ended the run in an unhandled ArgumentOutOfRangeException. Every expectation is what sbl -bf printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SelectionAlternatives
{
    private static string Run(string statement) =>
        SetupTests.RunWithInput($"        X = 'old'\n{statement}\n        OUTPUT = 'x=[' X ']'\n        OUTPUT = '@b'\nEND").Replace("\r\n", "\n");

    [TestMethod]
    public void TEST_SelectionAlternatives_an_empty_first_alternative_is_the_null_string()
    {
        Assert.AreEqual("x=[]\n@b", Run("        X = (,'b')"));
        Assert.AreEqual("x=[]\n@b", Run("        X = ( , 'b')"));
        Assert.AreEqual("x=[]\n@b", Run("        X = (,F())"));
    }

    [TestMethod]
    public void TEST_SelectionAlternatives_only_empty_alternatives_are_the_null_string()
    {
        Assert.AreEqual("x=[]\n@b", Run("        X = (,)"));
        Assert.AreEqual("x=[]\n@b", Run("        X = (,,)"));
    }

    [TestMethod]
    public void TEST_SelectionAlternatives_an_empty_middle_alternative_is_never_reached_after_a_value()
    {
        Assert.AreEqual("x=[a]\n@b", Run("        X = ('a',,'b')"));
        Assert.AreEqual("x=[a]\n@b", Run("        X = ('a',)"));
        Assert.AreEqual("x=[]\n@b", Run("        X = (A,,B)"));
    }

    [TestMethod]
    public void TEST_SelectionAlternatives_a_null_selection_assigned_to_output_writes_an_empty_line()
    {
        Assert.AreEqual("\n@b", SetupTests.RunWithInput("        OUTPUT = (,'b')\n        OUTPUT = '@b'\nEND").Replace("\r\n", "\n"));
    }

    // the statement with a failure goto, so that its success or failure shows: S or F after the value of X
    private static string RunGoto(string statement) =>
        SetupTests.RunWithInput($"        X = 'old'\n{statement}  :F(NO)\n        OUTPUT = 'x=[' X '] S'  :(END)\nNO      OUTPUT = 'x=[' X '] F'\nEND")
            .Replace("\r\n", "\n");

    /// <summary>
    /// A failure before a selection fails the whole expression, and the selection does not undo it, as in SPITBOL. The console took a
    /// failure that came before the selection began for the failure of its first alternative, so it cleared it, tried the next one and
    /// gave the statement success: X = LT(2, 1) (0, 1) :F(NO) went on as if it had succeeded (found by the random generator at scope 6).
    /// </summary>
    [TestMethod]
    public void TEST_SelectionAlternatives_a_failure_before_a_selection_fails_the_expression()
    {
        Assert.AreEqual("x=[old] F", RunGoto("        X = LT(2, 1) (0, 1)"));
        Assert.AreEqual("x=[old] F", RunGoto("        LT(2, 1) (0, 1)"));
        Assert.AreEqual("x=[old] F", RunGoto("        X = IDENT(1, 2) ('a', 'b') 'c'"));
        Assert.AreEqual("x=[old] F", RunGoto("        X = 'p' LPAD((LT(2, 1), 'q'), 2) LT(2, 1) (0, 1)"));
        Assert.AreEqual("x=[old] F", RunGoto("        X = LT(2, 1) ((OUTPUT = 'side'), 1)"));
    }

    /// <summary>
    /// A selection inside the first alternative of another ends at its own parenthesis, as in SPITBOL. The console's inner selection took
    /// the outer one's label at its close, so ('x', ('y', 'z') 'w') went from its value 'x' to the concatenation after the inner selection.
    /// </summary>
    [TestMethod]
    public void TEST_SelectionAlternatives_a_selection_inside_another_ends_at_its_own_parenthesis()
    {
        Assert.AreEqual("x=[x] S", RunGoto("        X = ('x', ('y', 'z') 'w')"));
        Assert.AreEqual("x=[xu] S", RunGoto("        X = ('x', ('y', 'z'), 'v') 'u'"));
        Assert.AreEqual("x=[yw] S", RunGoto("        X = (LT(2, 1), ('y', LT(2, 1)) 'w', 'v')"));
        Assert.AreEqual("x=[yw] S", RunGoto("        X = ((LT(2, 1), 'y'), 'z') 'w'"));
    }

    [TestMethod]
    public void TEST_SelectionAlternatives_the_run_goes_on_after_an_error_in_a_later_alternative()
    {
        Assert.AreEqual("@a\n@b", SetupTests.RunWithInput(
            "        &ERRLIMIT = 100\n        OUTPUT = '@a'\n        ?( ,F(X) G(X) )\n        OUTPUT = '@b'\nEND").Replace("\r\n", "\n"));
    }
}
