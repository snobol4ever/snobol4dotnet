using Test.TestLexer;

namespace Test.TestGoto;

/// <summary>
/// Under -f (no folding) the special labels are spelled in upper case alone: :(return), :(continue) and :(abort) name ordinary labels,
/// error 038 (goto undefined label) when the program defines none, as sbl -bf answers; :(RETURN) returns, and :(end) still reaches the
/// END line (sbl -bf, probed; rosetta-snobol4 short-circuit-evaluation, sieve-of-eratosthenes and detect-division-by-zero). snobol4dotnet
/// took the lower case spellings for the special labels in every mode.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SpecialLabelsUnderNoFolding
{
    private static string Run(string go) => SetupTests.RunWithInputDirectives("-bf", $@"        DEFINE('F()')   :(M)
F       OUTPUT = 'in f'   :({go})
M       F()
        OUTPUT = 'after'
END").Replace("\r\n", "\n");

    [TestMethod] public void TEST_SpecialLabelsUnderNoFolding_return_is_an_undefined_label() => StringAssert.Contains(Run("return"), "ERROR 038 -- goto undefined label");
    [TestMethod] public void TEST_SpecialLabelsUnderNoFolding_continue_is_an_undefined_label() => StringAssert.Contains(Run("continue"), "ERROR 038 -- goto undefined label");
    [TestMethod] public void TEST_SpecialLabelsUnderNoFolding_RETURN_returns() => Assert.AreEqual("in f\nafter", Run("RETURN").TrimEnd());
}
