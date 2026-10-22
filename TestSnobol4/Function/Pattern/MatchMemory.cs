using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A match runs over one copy of each pattern it enters by *X or ARBNO, however often and however deep it enters it, so
/// its memory follows its backtracking state and not the length of the subject, as SPITBOL's does.
/// </summary>
[TestClass]
public class MatchMemory
{
    // The calculator demo's grammar (corpus demos/snobol4/calculator), which sbl -b matches against these lines.  Each *X,
    // *T, *F and ARBNO repetition appended a fresh copy of its pattern's nodes, so the nodes grew with the subject: 2 GB
    // for the demo's 32 KB input, out of memory under its declared 128 MB heap, where sbl peaks at 7 MB.
    [TestMethod]
    public void TEST_MatchMemory_a_recursive_pattern_builds_its_nodes_once_however_long_the_subject()
    {
        var build = SetupTests.SetupScript("-b", @"
        I = SPAN('0123456789')
        V = ANY(&LCASE)
        A = I | V | '(' *X ')'
        F = '+' *F | '-' *F | A
        T = F '*' *T | F '/' *T | F
        X = T '+' *X | T '-' *X | T
        C = POS(0) ARBNO(X CHAR(10)) RPOS(0)
end");
        var pattern = ((PatternVar)build.Execute!.IdentifierTable[build.FoldCase("C")]).Data;

        int Nodes(int lines)
        {
            var scanner = new Scanner(build.Execute!);
            var subject = string.Concat(Enumerable.Repeat("(1+x)*-(2/y)-3\n", lines));
            Assert.IsTrue(scanner.PatternMatch(subject, pattern, 0, false).IsSuccess);
            return scanner.NodeCount;
        }

        Assert.AreEqual(Nodes(10), Nodes(400));
    }
}
