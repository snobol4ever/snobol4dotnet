using Test.TestLexer;

namespace Test.StringSynthesis;

/// <summary>
/// A concatenation whose result is longer than &amp;MAXLNGTH is error 205, string length exceeds value of maxlngth keyword, as in SPITBOL
/// (sbl -bf, probed: with &amp;MAXLNGTH = 4096 a string doubled from 'a' fails at 4096, at the default 16777216 it fails at 16777216; a value
/// below 1024 is not taken). snobol4dotnet checked DUPL, LPAD and RPAD and never a concatenation, so mikebeer_snobol bench/recursion grew
/// a string until .NET ran out of memory where sbl stops at 205.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AConcatenationPastMaxlngthIs205
{
    [TestMethod]
    public void TEST_AConcatenationPastMaxlngthIs205()
    {
        Assert.AreEqual("failed at 4096 205", SetupTests.RunWithInputDirectives("-bf", @"        &ERRLIMIT = 5
        &MAXLNGTH = 4096
        X = 'a'
L       X = X X        :F(F1)
        LT(SIZE(X), 3000000)   :S(L)
        OUTPUT = 'grew to ' SIZE(X)   :(END)
F1      OUTPUT = 'failed at ' SIZE(X) ' ' &ERRTYPE
END").Replace("\r\n", "\n").TrimEnd());
    }
}
