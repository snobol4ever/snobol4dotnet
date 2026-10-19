using Test.TestLexer;

namespace Test.Miscellaneous;

[TestClass]
public class Eval
{
    // See Eval.cs under Compilation

    [TestMethod]
    public void TEST_Eval_a_compile_error_fails_quietly()
    {
        // sbl -b answers fail 231 / fail2 221 / 0.17e+309: EVAL of a string that does not compile fails with
        // &ERRTYPE set to the compile error, prints nothing and is no run-time error
        var build = SetupTests.SetupScript("-b", @"
        X = EVAL('1.8E+308')           :S(A)F(B)
A       R = 'succ ' X                  :(C)
B       R = 'fail ' &ERRTYPE
C       Y = EVAL('1 +')                :S(D)F(E)
D       S = 'succ2'                    :(F)
E       S = 'fail2 ' &ERRTYPE
F       Z = EVAL('1.7E+308')
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("fail 231|fail2 221|0.17e+309", $"{build.Execute!.IdentifierTable["R"]}|{build.Execute!.IdentifierTable["S"]}|{build.Execute!.IdentifierTable["Z"]}");
    }
}
