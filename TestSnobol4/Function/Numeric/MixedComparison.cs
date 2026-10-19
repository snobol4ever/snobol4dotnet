using Test.TestLexer;

namespace Test.Numeric;

/// <summary>
/// An integer compared with a real is compared as a real, by its own value: EQ('12', 12.0) succeeds and
/// NE(12, 12.0) fails, as in SPITBOL.
/// </summary>
[TestClass]
public class MixedComparison
{
    [TestMethod]
    public void TEST_Comparison_of_an_integer_with_a_real()
    {
        // sbl -b answers b d f end (Budne's diag1 checks NE('12',12.0) at its statement 156)
        var build = SetupTests.SetupScript("-b", @"
        R = NE('12',12.0) 'a'
        R = R EQ('12',12.0) 'b'
        R = R NE(12,12.0) 'c'
        R = R GT(3,2.5) 'd'
        R = R LT(3,2.5) 'e'
        R = R GE(2,2.0) 'f'
        R = R LE(1,0.5) 'g'
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("bdf", build.Execute!.IdentifierTable["R"].ToString());
    }
}
