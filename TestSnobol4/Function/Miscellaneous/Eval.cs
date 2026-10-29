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

    [TestMethod]
    public void TEST_Eval_a_goto_or_a_semicolon_is_no_expression()
    {
        // sbl -b: EVAL takes one expression, so a colon or a semicolon outside every bracket and quote ends it, and the text is
        // error 103, "eval argument is not expression": a quiet failure with &ERRTYPE 103 after an expression that compiles
        // ('4 :(L)', ' B = 4 :S(OK)F(NO);END', '4; 5'), its own error after one that does not ('4 + ; ' is 221), 226 inside
        // parentheses ('(4 :(L))'), and a run-time error 103 when the text ends in the colon or semicolon ('4 ;', ':'), whatever
        // comes before it. The console compiled the text inside parentheses and gave 226 or 217 (found by measuring the CODE road
        // of DESIGN-PLAN random-codegen-scope step 11)
        var build = SetupTests.SetupScript("-b", @"
        R = EVAL('4 :(L)') 'succ'                        :S(A)
        R = 'fail ' &ERRTYPE
A       S = EVAL(' B = 4 :S(OK)F(NO);END') 'succ'        :S(B)
        S = 'fail ' &ERRTYPE
B       T = EVAL('4; 5') 'succ'                          :S(C)
        T = 'fail ' &ERRTYPE
C       U = EVAL('4 + ; ') 'succ'                        :S(D)
        U = 'fail ' &ERRTYPE
D       V = EVAL('(4 :(L))') 'succ'                      :S(E)
        V = 'fail ' &ERRTYPE
E       W = EVAL(""'a:b;c'"")
        &ERRLIMIT = 10
        X = EVAL('4 ;') 'succ'                           :S(F)
        X = 'error ' &ERRTYPE
F       Y = EVAL(':') 'succ'                             :S(G)
        Y = 'error ' &ERRTYPE
G
end");
        Assert.AreEqual("fail 103|fail 103|fail 103|fail 221|fail 226|a:b;c|error 103|error 103",
            string.Join("|", new[] { "R", "S", "T", "U", "V", "W", "X", "Y" }.Select(n => build.Execute!.IdentifierTable[n].ToString())));
    }
}
