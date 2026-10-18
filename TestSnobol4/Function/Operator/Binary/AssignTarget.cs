using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// X = V assigns the variable X even when X holds a table, array or DATA object that was earlier
/// read out of a collection element. Expected results are SPITBOL's (sbl -b; Budne's atn DUMP_PROP).
/// </summary>
[TestClass]
public class AssignTarget
{
    private static string Result(string body)
    {
        var build = SetupTests.SetupScript("-b", body + "\nend");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        return ((StringVar)build.Execute!.IdentifierTable["R"]).Data;
    }

    private const string Tables = @"
        P = TABLE()
        P<'a'> = TABLE()
        P<'a'><'k1'> = 'v1'
        P<'b'> = TABLE()
        P<'b'><'k2'> = 'v2'
        T = CONVERT(P,'ARRAY')";

    [TestMethod]
    public void TEST_AssignTarget_a_variable_holding_an_element_table_is_reassigned()
    {
        // The element still holds the first table, so the old mark was live as well as stale
        Assert.AreEqual("v2 v1", Result(Tables + @"
        X = T<1,2>
        X = T<2,2>
        R = X<'k2'> ' ' T<1,2><'k1'>"));
    }

    [TestMethod]
    public void TEST_AssignTarget_a_variable_holding_a_cleared_element_table_is_reassigned()
    {
        Assert.AreEqual("v2", Result(Tables + @"
        X = T<1,2>
        T<1,2> =
        X = T<2,2>
        R = X<'k2'>"));
    }

    [TestMethod]
    public void TEST_AssignTarget_an_element_is_still_assigned_through_its_index()
    {
        Assert.AreEqual("new v1", Result(Tables + @"
        X = T<1,2>
        T<1,2> = 'new'
        R = T<1,2> ' ' X<'k1'>"));
    }
}
