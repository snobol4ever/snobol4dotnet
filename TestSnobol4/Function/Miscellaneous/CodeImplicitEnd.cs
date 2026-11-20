using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SPITBOL ends each CODE block with an implicit statement numbered after its last, and control that runs off the block starts it
/// as it starts END: it is counted in &amp;STCOUNT, checked against &amp;STLIMIT, and the run ends with it as &amp;STNO. sbl -bf -D1 on
///   C = CODE(' X = 1') :&lt;C&gt; / END        dumps &amp;STNO = 4, &amp;STCOUNT = 3, &amp;LASTNO = 3
///   &amp;STLIMIT = 3 / C = CODE(' X = 1') :&lt;C&gt; / END   is error 244 in statement 5, &amp;STCOUNT = 3
/// snobol4dotnet ran off the block into the end of the run with the block's last statement current (rosetta-snobol4
/// runtime-evaluation-2, graded by its dump).
/// </summary>
[TestClass]
public class CodeImplicitEnd
{
    [TestMethod]
    public void TEST_CodeImplicitEnd_running_off_a_block_starts_its_implicit_end()
    {
        var build = SetupTests.SetupScript("-bf", "        C = CODE(' X = 1') :<C>\nEND");
        var x = build.Execute!;
        Assert.AreEqual(3L, x.AmpStatementCount, "&STCOUNT");
        Assert.AreEqual(4L, x.SourceStno[x.AmpCurrentLineNumber], "&STNO");
        Assert.AreEqual(3L, x.SourceStno[x.AmpLastLineNumber], "&LASTNO");
    }

    [TestMethod]
    public void TEST_CodeImplicitEnd_the_implicit_end_is_checked_against_stlimit()
    {
        var build = SetupTests.SetupScript("-bf", "        &STLIMIT = 3\n        C = CODE(' X = 1') :<C>\nEND");
        CollectionAssert.Contains(build.ErrorCodeHistory, 244);
        Assert.AreEqual(3L, build.Execute!.AmpStatementCount, "&STCOUNT");
    }

    [TestMethod]
    public void TEST_CodeImplicitEnd_a_block_with_its_own_END_has_none()
    {
        var build = SetupTests.SetupScript("-bf", "        C = CODE(' X = 1 ;END') :<C>\nEND");
        Assert.AreEqual(3L, build.Execute!.AmpStatementCount, "&STCOUNT");
    }
}
