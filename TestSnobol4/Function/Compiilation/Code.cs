using Test.TestLexer;

namespace Test.Compilation;

[TestClass]
public class Code
{
    [TestMethod]
    public void TEST_CODE001()
    {
        var s = @"
        S = ""L  a = a ' ' N; N = LT(N,10) N + 1 :S(L)F(DONE)""
        X = CONVERT(S,'code') :F(end)
                    :(L)
DONE    a
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("  1 2 3 4 5 6 7 8 9 10", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }
}