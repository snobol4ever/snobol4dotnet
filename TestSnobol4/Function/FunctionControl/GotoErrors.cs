using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// A goto that cannot be performed ends the run with the error sbl reports, and says so: a goto to a label that is not defined is 38,
/// RETURN, FRETURN and NRETURN outside a function are 242, and a computed goto (to no label, to no code, or whose expression fails)
/// with &amp;ERRLIMIT above 0 is the error it is (38, 24, 20) and then the failure exit, the same goto, is error 332. With &amp;ERRLIMIT above 0
/// the run used to end without a word. The errors listed are the ones sbl -b reports for the same programs.
/// </summary>
[DoNotParallelize]
[TestClass]
public class GotoErrors
{
    private static List<int> Errors(string program) => SetupTests.SetupScript("-b", program).ErrorCodeHistory.ToList();

    [TestMethod]
    public void TEST_GotoErrors_return_outside_a_function_is_242()
    {
        CollectionAssert.AreEqual(new[] { 242 }, Errors("        A = 1\n        :(RETURN)\n        OUTPUT = 'after'\nEND"));
        CollectionAssert.AreEqual(new[] { 242 }, Errors("        &ERRLIMIT = 5\n        A = 1\n        :(RETURN)\n        OUTPUT = 'after'\nEND"));
        CollectionAssert.AreEqual(new[] { 242 }, Errors("        &ERRLIMIT = 5\n        A = 1\n        :(NRETURN)\n        OUTPUT = 'after'\nEND"));
    }

    [TestMethod]
    public void TEST_GotoErrors_a_goto_to_an_undefined_label_is_38_and_ends_the_run_whatever_errlimit_allows()
    {
        CollectionAssert.AreEqual(new[] { 38 }, Errors("        A = 1\n        :(NOWHERE)\n        OUTPUT = 'after'\nEND"));
        CollectionAssert.AreEqual(new[] { 38 }, Errors("        &ERRLIMIT = 5\n        A = 1\n        :(NOWHERE)\n        OUTPUT = 'after'\nEND"));
    }

    [TestMethod]
    public void TEST_GotoErrors_a_computed_goto_with_errlimit_is_followed_by_332()
    {
        // sbl: the error, then error 332 three times (the failure exit is the goto again; the third strike is fatal), as a handler that
        // takes each error and goes on shows: 38, 332, 332, then the fatal 332
        CollectionAssert.AreEqual(new[] { 38 }, Errors("        X = 'NOWHERE'\n        :($X)\n        OUTPUT = 'after'\nEND"));
        CollectionAssert.AreEqual(new[] { 38, 332, 332, 332 }, Errors("        &ERRLIMIT = 5\n        X = 'NOWHERE'\n        :($X)\n        OUTPUT = 'after'\nEND"));
        CollectionAssert.AreEqual(new[] { 38, 332, 332, 332 }, Errors("        &ERRLIMIT = 5\n        X = 5\n        :($X)\n        OUTPUT = 'after'\nEND"));
        CollectionAssert.AreEqual(new[] { 24, 332, 332, 332 }, Errors("        &ERRLIMIT = 5\n        X = 'abc'\n        :<X>\n        OUTPUT = 'after'\nEND"));
        // the expression of a computed goto that is no name: whatever it is, the failure exit is the same goto and ends the run with 332
        Assert.AreEqual(332, Errors("        &ERRLIMIT = 5\n        :($(FAIL))\n        OUTPUT = 'after'\nEND")[^1]);
    }
}
