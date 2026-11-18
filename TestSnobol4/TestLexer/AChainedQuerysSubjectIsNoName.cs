using Test.TestLexer;

namespace Test.TestLexerChecks;

/// <summary>
/// A statement-level query chains left to right, so in Y ? 'l' ? '' = 'L' the subject of the replacement is (Y ? 'l'), a match, whose
/// value is no name: error 212, value used where name is required, at compile, as sbl -bf answers (gate_cases_snobol4
/// test_gate_sno_a_statement_level_query_takes_a_concatenated_subject_and_chains_left_to_right__refuse2). snobol4dotnet ran it and
/// printed 'ran Ll'. A query with one ? still replaces (Y ? 'l' = 'L' gives heLlo).
/// </summary>
[DoNotParallelize]
[TestClass]
public class AChainedQuerysSubjectIsNoName
{
    [TestMethod]
    public void TEST_AChainedQuerysSubjectIsNoName_is_212()
    {
        var build = SetupTests.SetupScript("-bf", @"        Y = 'hello'
        Y ? 'l' ? '' = 'L'
        OUTPUT = 'ran ' Y
END");
        CollectionAssert.Contains(build.ErrorCodeHistory, 212);
    }

    [TestMethod]
    public void TEST_AChainedQuerysSubjectIsNoName_one_query_still_replaces()
    {
        Assert.AreEqual("heLlo", SetupTests.RunWithInputDirectives("-bf", @"        Y = 'hello'
        Y ? 'l' = 'L'
        OUTPUT = Y
END").Replace("\r\n", "\n").TrimEnd());
    }
}
