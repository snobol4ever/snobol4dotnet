using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A runtime error names the erring statement's own line, as sbl -b does (el.sno(4) : ERROR 001), not
/// the end of the source.
/// </summary>
[TestClass]
public class ErrorLocation
{
    [TestMethod]
    public void TEST_ErrorLocation_a_runtime_error_names_its_statement()
    {
        var build = SetupTests.SetupScript("-b", @"* a comment
        &ERRLIMIT = 1

        Y = 'a' + 1
        X = 1
end");
        Assert.AreEqual(1, build.ErrorCodeHistory[0]);
        StringAssert.Contains(build.Execute!.LastDiagnostic, "/4): error 1 --");
    }
}
