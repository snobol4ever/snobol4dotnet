using Test.TestLexer;

namespace Test.Keywords;

/// <summary>
/// With case folding off (-f) names are spelled as written and SPITBOL's keywords are the upper-case names only: &amp;errlimit is error
/// 251, assigned or read; with folding (the default) it is the keyword. The reports are what sbl -bf and sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class KeywordCase
{
    [TestMethod]
    public void TEST_KeywordCase_a_lower_case_keyword_is_not_a_keyword_when_names_are_not_folded()
    {
        foreach (var statement in new[] { "&errlimit = 100", "OUTPUT = &anchor", "&Errlimit = 100" })
        {
            var output = SetupTests.RunWithInputDirectives("-bf", "        " + statement + "\n        OUTPUT = 'after'\nEND").Replace("\r\n", "\n");
            StringAssert.Contains(output, "Test.sno(1) : ERROR 251 -- keyword operand is not name of defined keyword", statement);
            Assert.IsFalse(output.Contains("after"), statement);
        }
    }

    [TestMethod]
    public void TEST_KeywordCase_an_upper_case_keyword_is_a_keyword_when_names_are_not_folded()
    {
        var output = SetupTests.RunWithInputDirectives("-bf", "        &ERRLIMIT = 100\n        OUTPUT = &ERRLIMIT\nEND").Replace("\r\n", "\n");
        Assert.AreEqual("100", output.Trim());
    }

    [TestMethod]
    public void TEST_KeywordCase_a_keyword_is_a_keyword_in_any_case_when_names_are_folded()
    {
        var output = SetupTests.RunWithInputDirectives("-b", "        &errlimit = 100\n        OUTPUT = &ErrLimit\n        OUTPUT = &ERRLIMIT\nEND").Replace("\r\n", "\n");
        Assert.AreEqual("100\n100", output.Trim());
    }
}
