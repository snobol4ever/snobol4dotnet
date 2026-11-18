using Test.TestLexer;

namespace Test.Compiilation;

/// <summary>
/// Whether a program with compilation errors runs is decided error by error, as in SPITBOL: each error found while -NOERRORS is in
/// force (the default) inhibits execution (sbl.min erra3: add noxeq,cswer), and -ERRORS changes only what the errors after it do. sbl -bf:
/// an error, then -ERRORS, does not run (exit 231); -ERRORS, then an error, runs, and so does -ERRORS, an error, -NOERRORS. snobol4dotnet
/// ran a program that had an -ERRORS anywhere in it.
/// </summary>
[TestClass]
public class ErrorsCountsTheErrorsAfterIt
{
    private static Snobol4.Common.Builder Build(string program) => SetupTests.SetupScript("-bf", program);

    [TestMethod]
    public void TEST_ErrorsCountsTheErrorsAfterIt_an_error_before_it_inhibits()
    {
        Assert.AreEqual(231, Build("        X = 1 +\n-ERRORS\n        Y = 1\nEND\n").ExitStatus);
    }

    [TestMethod]
    public void TEST_ErrorsCountsTheErrorsAfterIt_an_error_after_it_runs()
    {
        Assert.AreNotEqual(231, Build("-ERRORS\n        X = 1 +\n        Y = 1\nEND\n").ExitStatus);
    }

    [TestMethod]
    public void TEST_ErrorsCountsTheErrorsAfterIt_noerrors_after_the_error_runs()
    {
        Assert.AreNotEqual(231, Build("-ERRORS\n        X = 1 +\n-NOERRORS\n        Y = 1\nEND\n").ExitStatus);
    }
}
