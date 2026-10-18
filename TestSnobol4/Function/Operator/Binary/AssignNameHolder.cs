using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// A variable that holds a NAME is itself the assignment target; a NAME an NRETURN hands back in the
/// same statement is assigned through. Expected results are SPITBOL's (sbl -b).
/// </summary>
[TestClass]
public class AssignNameHolder
{
    [TestMethod]
    public void TEST_AssignNameHolder_a_variable_holding_a_name_is_assigned_itself()
    {
        var build = SetupTests.SetupScript("-b", @"
        R = .ONE
        R = R ' ' .TWO
        A = ARRAY(2)
        N = .A<1>
        N = 'held'
        DEFINE('F()')     :(EF)
F       F = .X            :(NRETURN)
EF      F() = 'via nreturn'
        S = R ',' DATATYPE(R) ',' ONE ',' A<1> ',' N ',' X
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ONE TWO,STRING,,,held,via nreturn", ((StringVar)build.Execute!.IdentifierTable["S"]).Data);
    }
}
