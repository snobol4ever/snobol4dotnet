using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// The target of a goto in parens is a name SPITBOL generates by name, as the target of an assignment is: a constant, an arithmetic
/// result, a concatenation or a name operator's value there is error 212, "value used where name is required", found when the
/// statement is compiled. A target in angle brackets is a value (a code) and is judged when it is taken. Every expectation is
/// what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class GotoTargets
{

    [TestMethod]
    public void TEST_GotoTargets_a_bracketed_assignment_is_212()
    {
        // sbl -b: a bracket just inside a goto's own groups what it holds, so :((X = 1)) is a goto to the value an assignment gives, 212,
        // in each goto (:S, :F, and inside another bracket); :($(X = 1)), :((L)) and a bracket in a direct goto compile. The console took
        // that bracket for the call's, with no function before it, and compiled them all (the master's ladder__rung05 entry
        // goto_a_value_operand_is_error_212..., read against a fresh sbl run)
        var build = SetupTests.SetupScript("-b -n", @"        Y = 1 :((X = 1))
        Y = 1 :((X = 'L'))
        Y = 1 :(((X = 1)))
        Y = 1 :S((X = 1))
        Y = 1 :F((X = 1))
        Y = 1 :($(X = 1))
        Y = 1 :((L))
        Y = 1 :S((L))F((L))
        Y = 1 :<(C)>
L
END");
        CollectionAssert.AreEqual(new[] { 212, 212, 212, 212, 212 }, build.ErrorCodeHistory.ToArray());
    }
}
