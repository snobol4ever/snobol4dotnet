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

    [TestMethod]
    public void TEST_Assign_a_protected_value_leaves_the_target_unprotected()
    {
        // sbl -b answers 1 two three el: a variable or element that was given FAIL, ARB or REM can be
        // given another value (Gimpel's ONCE does $NAME = FAIL twice); snobol4dotnet refused with error 42
        var build = SetupTests.SetupScript("-b", @"
        X = FAIL
        X = 1
        Y = ARB
        Y = 'two'
        N = 'Z'
        $N = REM
        $N = 'three'
        A = ARRAY(1)
        A<1> = FAIL
        A<1> = 'el'
        R = X ' ' Y ' ' Z ' ' A<1>
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("1 two three el", build.Execute!.IdentifierTable["R"].ToString());
    }

    [TestMethod]
    public void TEST_Assign_expression_holder_is_the_target()
    {
        // sbl -b: a variable holding an expression is itself the target (V = *(X + 1) then V = *Y assigns V),
        // and so is E in $ E, while . *X assigns X; snobol4dotnet evaluated V's expression as the target (212)
        var build = SetupTests.SetupScript("-b", """
                 V = *(X + 1)
                 V = *Y
                 E = *X
                 'abc' LEN(1) . *X
                 R = X
                 'abc' LEN(2) $ E
                 R = R ' ' DATATYPE(V) ' ' E ' ' X
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("a EXPRESSION ab a", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data);
    }
}
