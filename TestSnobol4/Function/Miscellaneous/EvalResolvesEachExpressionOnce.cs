using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// Each EVAL of a text compiles one deferred expression and adds it to the list of every one the program has parsed, and every compile
/// resolved the whole list again, so the n-th EVAL cost n times the first: 10,000 EVALs of '1 + 2' took 31 seconds, 20,000 took 93,
/// where sbl takes 13 milliseconds for 20,000, and the SnoRungs entry that evaluates a sum 16 times a line for 3,000 lines ran past its
/// 600 seconds. The slot tables only grow, so an expression resolved once is resolved for good: each is resolved exactly once.
/// </summary>
[DoNotParallelize]
[TestClass]
public class EvalResolvesEachExpressionOnce
{
    [TestMethod]
    public void TEST_EvalResolvesEachExpressionOnce_every_expression_is_resolved_once()
    {
        var program = @"
        i = 0
loop    i = i + 1
        x = eval('1 + 2')
        y = eval('x * 2')
        lt(i, 300)                :s(loop)
end";
        var build = SetupTests.SetupScript("-b", program);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(6, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
        Assert.IsTrue(build.ParseExpression.Count >= 600);
        Assert.AreEqual((long)build.ParseExpression.Count, build.ExpressionsResolved);
    }

    [TestMethod]
    public void TEST_EvalResolvesEachExpressionOnce_a_name_first_met_in_an_evaluated_text_is_resolved()
    {
        var program = @"
        a = 3
        x = eval('a + b1')
        b1 = 4
        y = eval('a + b1')
        z = eval('a * (b1 + c1)')
        c1 = 5
        w = eval('a * (b1 + c1)')
        output = x '|' y '|' z '|' w
end";
        Assert.AreEqual("3|7|12|27", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
