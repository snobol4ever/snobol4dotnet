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
    public void TEST_AssignNameHolder_an_element_or_field_holding_an_expression_is_assigned_itself()
    {
        // sbl -bf: an element, a table entry or a field that holds an expression is itself the target of = and of . and $ in a
        // pattern, as a variable that holds one is; only a deferred expression written as the target (. *N, $ *P) is evaluated for
        // its name. The console evaluated the expression an element or field held and assigned N through it, leaving the element as it
        // was (found by the infinite_snobol4 campaign at scope 12, seed0 20261031, batch 3 of kind=stmt, line 150)
        var build = SetupTests.SetupScript("-bf", @"        N = 'o'
        A = ARRAY(2)
        A[1] = *N
        A[1] = 'm'
        S = '1 N=' N ' A1=' DATATYPE(A[1]) ' ' A[1]
        T = TABLE()
        T['k'] = *N
        T['k'] = 'm'
        S = S ',2 N=' N ' Tk=' T['k']
        DATA('PT(U,V)')
        G = PT(CONVERT('N', 'EXPRESSION'), 1)
        U(G) = 'm'
        S = S ',3 N=' N ' U=' U(G)
        A[1] = *N
        'abc' ? LEN(1) . A[1]
        S = S ',4 N=' N ' A1=' A[1]
        A[1] = *N
        'abc' ? LEN(2) $ A[1]
        S = S ',5 N=' N ' A1=' A[1]
        'abc' ? LEN(1) . *N
        S = S ',6 N=' N
END");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("1 N=o A1=STRING m,2 N=o Tk=m,3 N=o U=m,4 N=o A1=a,5 N=o A1=ab,6 N=a",
            ((StringVar)build.Execute!.IdentifierTable["S"]).Data);
    }

    [TestMethod]
    public void TEST_AssignNameHolder_a_cursor_given_to_a_variable_holding_a_name_sets_the_variable()
    {
        // sbl -bf and CSNOBOL4: @A sets A when A holds a NAME, as = . and $ do, whether A is written, reached by $N, or handed back
        // by an NRETURN; the console assigned the cursor through the NAME A held, to I (found by the infinite_snobol4 campaign at
        // scope 12, round 7, seed 528243, batch 9 of kind=stmt, line 564)
        var build = SetupTests.SetupScript("-bf", @"        DEFINE('H()')                     :(H_END)
H       H = .A                            :(NRETURN)
H_END   A = .I
        'xyz' @A
        S = '1 ' DATATYPE(A) ' [' I ']'
        A = .I
        N = .A
        'xyz' LEN(1) @$N
        S = S ',2 ' DATATYPE(A) ' [' I ']'
        A = .I
        'xyz' LEN(2) @H()
        S = S ',3 ' DATATYPE(A) ' ' A ' [' I ']'
        P = LEN(1) @A
        A = .I
        'xyz' P
        S = S ',4 ' DATATYPE(A) ' [' I ']'
END");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("1 INTEGER [],2 INTEGER [],3 INTEGER 2 [],4 INTEGER []",
            ((StringVar)build.Execute!.IdentifierTable["S"]).Data);
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
