using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// After an error, &amp;ERRTEXT is the message alone, as SPITBOL holds it (sbl -b; Budne's err):
/// the file, numbers and source line belong to the printed diagnostic only.
/// </summary>
[TestClass]
public class ErrText
{
    [TestMethod]
    public void TEST_ErrText_is_the_message_alone()
    {
        var build = SetupTests.SetupScript("-b", @"
        &ERRLIMIT = 1
        A<1> =
        R = &ERRTYPE ' ' &ERRTEXT
end");
        Assert.AreEqual("235 subscripted operand is not table or array", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}
