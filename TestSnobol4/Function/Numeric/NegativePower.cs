using Snobol4.Common;
using Test.TestLexer;

namespace Test.Numeric;

/// <summary>
/// An integer to a negative integer power is a real, and a zero base to a negative power is 0., as
/// SPITBOL answers (sbl -b; Budne's pow).
/// </summary>
[TestClass]
public class NegativePower
{
    [TestMethod]
    public void TEST_NegativePower_answers_a_real()
    {
        var build = SetupTests.SetupScript("-b", @"
        Z = 0
        ZR = 0.0
        M = -8
        R = (9 ^ -1) ' ' (2 ** -2) ' ' (Z ^ -1) ' ' (ZR ^ -1) ' ' (M ^ -1) ' ' (1 ^ -5) ' ' (2 ^ 3)
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("0.111111111111111 0.25 0. 0. -0.125 1. 8", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}
