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

    [TestMethod]
    public void TEST_SelectionAlternatives_the_run_goes_on_after_an_error_in_a_later_alternative()
    {
        Assert.AreEqual("@a\n@b", SetupTests.RunWithInput(
            "        &ERRLIMIT = 100\n        OUTPUT = '@a'\n        ?( ,F(X) G(X) )\n        OUTPUT = '@b'\nEND").Replace("\r\n", "\n"));
    }
}
