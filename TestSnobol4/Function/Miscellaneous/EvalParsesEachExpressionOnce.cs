using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// Each EVAL of a text adds one deferred expression to the list of every one the program has lexed, and the parse that follows looked
/// at the whole list for the ones not parsed yet, so after the slot resolution was made linear the n-th EVAL still cost n times the
/// first: 40,000 EVALs of '1 + 2' took 46 seconds where 20,000 took 11, and Parse was 68 percent of the profile. The parse now takes
/// up where its last call stopped, and an expression that failed to parse is looked at again, as before.
/// </summary>
[DoNotParallelize]
[TestClass]
public class EvalParsesEachExpressionOnce
{
    [TestMethod]
    public void TEST_EvalParsesEachExpressionOnce_every_expression_is_looked_at_once()
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
        Assert.IsTrue(build.ExpressionList.Count >= 600);
        Assert.AreEqual((long)build.ExpressionList.Count, build.ExpressionsExamined);
    }
}
