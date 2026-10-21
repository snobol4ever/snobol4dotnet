using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A runtime error names the erring statement's own line, as sbl -b does (el.sno(4) : ERROR 001), not
/// the end of the source. The operand is a variable: with constants for both operands sbl finds the error when it
/// compiles the statement (PreEvaluation).
/// </summary>
[TestClass]
public class ErrorLocation
{
    [TestMethod]
    public void TEST_ErrorLocation_a_runtime_error_names_its_statement()
    {
        var build = SetupTests.SetupScript("-b", @"* a comment
        &ERRLIMIT = 1
        A = 'a'
        Y = A + 1
        X = 1
end");
        Assert.AreEqual(1, build.ErrorCodeHistory[0]);
        StringAssert.Contains(build.Execute!.LastDiagnostic, "/4): error 1 --");
    }

    [TestMethod]
    public void TEST_ErrorLocation_goto_undefined_label_is_38()
    {
        // sbl -bf: a goto to a label that does not exist, direct (:(NOLAB), :F(NOLAB)) or through a name
        // (:($X), :($N) with N = 5), is error 38 "goto undefined label"; a null name is 239 (the dotnet
        // package's asgn1 takes :F(error) when words.txt is missing); snobol4dotnet reported 23
        foreach (var go in new[] { ":(NOLAB)", ":F(NOLAB)", ":($X)", ":($N)" })
        {
            var build = SetupTests.SetupScript("-bf", $"""
                         X = 'NOLAB'
                         N = 5
                         OUTPUT = DIFFER('') 'go'   {go}
                 END
                 """);
            CollectionAssert.AreEqual(new[] { 38 }, build.ErrorCodeHistory.ToArray(), go);
        }
    }
}
