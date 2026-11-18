using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A deferred expression that fails inside a match (*EQ(1,2)) leaves nothing of its evaluation behind, so the value of S ? P is the
/// substring matched, as in SPITBOL: sbl -bf answers 'xyab' ? (*EQ(1,2) | 'ab') with ab and '7[' ('ab' ? (*EQ(1,2) | 'b')) ']' with 7[b].
/// snobol4dotnet left the failed call's value on the stack, and the match's value was read from under it (the null string, or a failed
/// concatenation), so rosetta-snobol4 range-extraction, whose loop replaces d1 ' ' d2 by d1 ('~,' ? *EQ(...) '~' | ',') d2, never ended.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AFailedDeferredCallLeavesNothingOnTheStack
{
    [TestMethod]
    public void TEST_AFailedDeferredCallLeavesNothingOnTheStack()
    {
        Assert.AreEqual("[ab]\n7[b]", SetupTests.RunWithInputDirectives("-bf", @"        X = 'xyab' ? (*EQ(1,2) | 'ab')
        OUTPUT = '[' X ']'
        OUTPUT = '7[' ('ab' ? (*EQ(1,2) | 'b')) ']'
END").Replace("\r\n", "\n").TrimEnd());
    }
}
