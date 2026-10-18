using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// ORD(s), the SPITBOL x64 builtin: the code of s's first character as an integer; the null string fails
/// and a non-string is error 333. Each expectation is sbl -b's.
/// </summary>
[TestClass]
public class Ord
{
    [TestMethod]
    public void TEST_Ord_codes_failure_and_error()
    {
        var build = SetupTests.SetupScript("-b", @"
        R = ORD('AB') ' ' ORD(5) ' ' ORD(CHAR(200)) ' ' DATATYPE(ORD('A'))
        S = 'kept'
        S = ORD('')
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("65 53 200 INTEGER", build.Execute!.IdentifierTable["R"].ToString());
        Assert.AreEqual("kept", build.Execute!.IdentifierTable["S"].ToString());

        var error = SetupTests.SetupScript("-b", @"
        R = ORD(ARRAY(2))
end");
        Assert.AreEqual(333, error.ErrorCodeHistory[0]);
    }
}
