using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// SUCCEED matches the null string and matches it again each time the match backs into it, so a pattern that fails after it tries it again (the match loops, as SPITBOL's does, until a condition in the pattern lets it through). It was a pattern that matched once, so SUCCEED *?(N = N + 1) *?GT(N, 5) failed after four tries where sbl matches after six. Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class Succeed
{
    [TestMethod]
    public void TEST_Succeed_backing_into_succeed()
    {
        var s = @"        N = 0
        'abc' ? SUCCEED *?(N = N + 1) *?GT(N, 5)   :S(OK)F(NO)
OK      OUTPUT = 'matched after ' N
        :(DONE)
NO      OUTPUT = 'failed after ' N
DONE    K = 0
        'abc' ? ARB $ X SUCCEED *?(K = K + 1) *?GT(K, 3)   :S(OK3)F(NO3)
OK3     OUTPUT = 'matched3 after ' K ' [' X ']'  :(D3)
NO3     OUTPUT = 'failed3 after ' K
D3      I = 0
        'abc' ? (SUCCEED | 'zz') *?(I = I + 1) *?GE(I, 2)   :S(OK5)F(NO5)
OK5     OUTPUT = 'matched5 after ' I  :(D5)
NO5     OUTPUT = 'failed5 after ' I
D5      H = 0
        'abc' ? LEN(1) . Y SUCCEED *?(H = H + 1) *?GT(H, 2) :S(OK6)F(NO6)
OK6     OUTPUT = 'matched6 after ' H ' [' Y ']'  :(D6)
NO6     OUTPUT = 'failed6 after ' H
D6
END";
        Assert.AreEqual(@"matched after 6
matched3 after 4 []
matched5 after 2
matched6 after 3 [a]", SetupTests.RunWithInput(s));
    }
}
