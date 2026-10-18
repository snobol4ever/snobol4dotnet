using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestCommandLine;

/// <summary>
/// -CASE N acts from its own line on (N absent is 0); a control in lower case is a control only while
/// names fold; an unknown control is ignored. Expected results are SPITBOL's (sbl -b; Budne's case1).
/// </summary>
[TestClass]
public class CaseControl
{
    [TestMethod]
    public void TEST_CaseControl_acts_from_its_own_line()
    {
        var build = SetupTests.SetupScript("-b", @"
        abc = 'lower'
-case
        abc = 'L2'
        R = ABC ',' abc
-case 1
        R = R ',' abc
-CASE 1
        R = R ',' abc
-HIDE
END");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("lower,L2,L2,lower", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}
