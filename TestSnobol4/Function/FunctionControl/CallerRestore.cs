using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// A return, of any kind, gives the caller back every variable the call bound as a parameter or
/// local, under its own name, even when the caller passed one of its own arrays as an argument.
/// Expected results are SPITBOL's (sbl -b).
/// </summary>
[TestClass]
public class CallerRestore
{
    private static string Result(string script)
    {
        var build = SetupTests.SetupScript("-b", script);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        return ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data;
    }

    [TestMethod]
    public void TEST_CallerRestore_a_local_array_passed_on_comes_back_after_return()
    {
        Assert.AreEqual("top,child", Result(@"
        DEFINE('G(B)C')                   :(EG)
G       C = COPY(B)
        C<1> = 'child'
        IDENT(B<1>, 'top')                :F(RETURN)
        G(C)
        R = B<1> ',' C<1>                 :(RETURN)
EG      A = ARRAY(2)
        A<1> = 'top'
        G(A)
end"));
    }

    [TestMethod]
    public void TEST_CallerRestore_a_local_array_passed_on_comes_back_after_freturn()
    {
        // Budne's sudoku: a failed try must leave the caller's board as it was
        Assert.AreEqual("top,child", Result(@"
        DEFINE('G(B)C')                   :(EG)
G       C = COPY(B)
        C<1> = 'child'
        IDENT(B<1>, 'top')                :F(FRETURN)
        G(C)
        R = B<1> ',' C<1>                 :(RETURN)
EG      A = ARRAY(2)
        A<1> = 'top'
        G(A)
end"));
    }

    [TestMethod]
    public void TEST_CallerRestore_a_name_that_is_both_parameter_and_local_comes_back()
    {
        Assert.AreEqual("outer", Result(@"
        DEFINE('H(R)R')                   :(EH)
H       R = 'inner'                       :(RETURN)
EH      R = 'outer'
        H('arg')
end"));
    }
}
