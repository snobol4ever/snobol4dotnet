using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A traced call is traced before the branch to the function's entry label, so a call to a function whose entry label is undefined (error
/// 286) prints its CALL trace line first, as sbl does. The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CallTraceEntryLabel
{
    [TestMethod]
    public void TEST_CallTraceEntryLabel_a_call_to_an_undefined_entry_label_is_traced_first()
    {
        var program = @"        DEFINE('E()')
        TRACE(.E, 'CALL')
        &ERRLIMIT = 5
        &TRACE = 100
        E()
        OUTPUT = 'after ' &ERRTYPE
END";
        Assert.AreEqual(@"****5*******  E()
after 286", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_CallTraceEntryLabel_a_call_with_arguments_shows_them()
    {
        var program = @"        DEFINE('E(A,B)')
        TRACE(.E, 'CALL')
        &ERRLIMIT = 5
        &TRACE = 100
        E('x', 3)
        OUTPUT = 'after ' &ERRTYPE
END";
        Assert.AreEqual(@"****5*******  E('x',3)
after 286", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
