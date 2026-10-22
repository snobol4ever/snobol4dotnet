using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A conditional assignment whose target is a deferred expression calling a function that is not defined is an error (22) that leaves
/// nothing on the evaluator's stack: the match took one entry off it too many, the statement's own separator, and the run ended in an
/// unhandled ArgumentOutOfRangeException when &amp;ERRLIMIT let it go on. The expectation is what sbl -bf -k printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class DeferredAssigneeErrors
{
    [TestMethod]
    public void TEST_DeferredAssigneeErrors_the_run_goes_on_after_the_error()
    {
        Assert.AreEqual("@a\n@b", SetupTests.RunWithInput(
            "        &ERRLIMIT = 100\n        OUTPUT = '@a'\n        X ? REM . *F(*1)\n        OUTPUT = '@b'\nEND").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeErrors_with_a_captured_name_and_other_targets()
    {
        Assert.AreEqual("@a\n@b", SetupTests.RunWithInput(
            "        &ERRLIMIT = 100\n        OUTPUT = '@a'\n        X  POS(0) REM . S . *F(.T[S], *1)\n        OUTPUT = '@b'\nEND").Replace("\r\n", "\n"));
        Assert.AreEqual("@a\n@b", SetupTests.RunWithInput(
            "        &ERRLIMIT = 100\n        OUTPUT = '@a'\n        X ? REM . *F(*1) . *G(1)\n        OUTPUT = '@b'\nEND").Replace("\r\n", "\n"));
    }
}
