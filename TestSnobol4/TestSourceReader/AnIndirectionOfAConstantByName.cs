using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// Under unary . and @, which SPITBOL's compiler generates by name, an indirection of a constant that is no string, number or name
/// has no name and is error 239 as the statement is compiled: .$ARB, .$LEN(1), .$*X, .$('a' | 'b'), .$FAIL, .$&amp;FENCE, @$ARB and
/// .$ARB + 1. The indirection of a string, a number or a name (.$1, .$'X', .$.X, .$('a' 'b'), .$(1 + 2)) names its variable, and the
/// indirection of a constant pattern anywhere else ($ARB = 1, P . $ARB, :($ARB)) is found as the program runs. dotnet found .$ARB only
/// as it ran and the others not at all. Found by the infinite_snobol4 generator (.$ARB * ... / -FENCE('a' | 'b'), seed 355681: sbl 239,
/// dotnet the fold error 10 to its right). Every expected number is what sbl -b listed for the same statement.
/// </summary>
[TestClass]
public class AnIndirectionOfAConstantByName
{
    [TestMethod]
    public void TEST_AnIndirectionOfAConstantByName_is_239_as_it_is_compiled()
    {
        var build = SetupTests.SetupScript("-b -n", @"        Y = .$ARB
        Y = .$LEN(1)
        Y = .$*X
        Y = .$('a' | 'b')
        Y = .$FAIL
        Y = .$1
        Y = .$'X'
        Y = .$&FENCE
        Y = .$.X
        Y = @$ARB
        Y = P . $ARB
        Y = P $ $ARB
        $ARB = 1
        Y = 1 :($ARB)
        Y = .$('a' 'b')
        Y = .$(1 + 2)
        Y = .$-1
        Y = .$X
        Y = .$$ARB
        Y = .$ARB + 1
END");
        CollectionAssert.AreEqual(new[] { 239, 239, 239, 239, 239, 239, 239, 239 },
            build.ErrorCodeHistory.ToArray());
    }

    [TestMethod]
    public void TEST_AnIndirectionOfAConstantByName_as_the_target_of_a_constant_capture_is_239()
    {
        // sbl -b: a capture of a constant pattern is built as the statement is compiled and names its target then, so a target that is
        // an indirection of a constant pattern (the primitive patterns, which SPITBOL lets no program alter, or LEN(1)) is 239 there:
        // LEN(1) . $ARB, LEN(1) $ $ARB, LEN(1) . $LEN(1), 'a' . $ARB, ANY(1) . $BAL, LEN(1) . $FENCE and (LEN(1) . $REM) 'x'. A
        // variable's capture (Q . $ARB), a target found as it runs ($X, $'ARB') or deferred (*$ARB) compiles. dotnet compiled them all
        // (the master's code_eval_arb_len_replace_branch_1, read against a fresh sbl run)
        var build = SetupTests.SetupScript("-b -n", @"        Y = LEN(1) . $ARB
        Y = LEN(1) $ $ARB
        Y = LEN(1) . $X
        Y = LEN(1) . $LEN(1)
        Y = Q . $ARB
        Y = LEN(1) . *$ARB
        Y = 'a' . $ARB
        Y = ANY(1) . $BAL
        Y = LEN(1) . $FENCE
        Y = LEN(1) . $'ARB'
        Y = (LEN(1) . $REM) 'x'
END");
        CollectionAssert.AreEqual(new[] { 239, 239, 239, 239, 239, 239, 239 }, build.ErrorCodeHistory.ToArray());
    }
}
