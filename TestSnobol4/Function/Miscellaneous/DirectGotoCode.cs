using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A direct goto takes CODE as its operand's value, as SPITBOL does: :<CODE('...')> with no variable
/// behind it transfers like :<C> with C holding the CODE.
/// </summary>
[DoNotParallelize]
[TestClass]
public class DirectGotoCode
{
    [TestMethod]
    public void TEST_DirectGoto_to_a_code_value()
    {
        // sbl -b answers in code / back / LP=lp / direct (Gimpel's LPROG returns through such a goto)
        Assert.AreEqual("in code\nback\nLP=lp\ndirect", SetupTests.RunWithInput(@"        C = CODE(' OUTPUT = ""in code"" :(BACK)')
        :<C>
BACK    OUTPUT = 'back'
        DEFINE('LP()')                 :(LP_END)
LP      :<CODE(' LP = ""lp"" :(RETURN)')>
LP_END  OUTPUT = 'LP=' LP()
        :<CODE(' OUTPUT = ""direct""')>
END").Replace("\r\n", "\n"));
    }
}
