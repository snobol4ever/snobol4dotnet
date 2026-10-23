using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// An error inside an evaluated expression unwinds, in SPITBOL, to the frame of the innermost program-defined function call or of the
/// EVAL, whichever it meets first, and the statement of a function shares its frame with the call (err04: flprt, gtcef): an EVAL that
/// a function's statement makes goes on from an error in its expression as a statement does, so EVAL("~$''") is the null string and
/// EVAL("(1 / N, 'd')") is d there, at any depth of calls and in a SETEXIT handler that runs inside a function, while in the main
/// program, or inside another evaluated expression at the same function level, the EVAL fails as a whole (EVAL('~(1 / N)') in
/// EVAL("~EVAL('~(1 / N)')") fails, and the outer negation succeeds). dotnet ended the evaluation at every level since 94356f2.
/// Found by the infinite_snobol4 generator under -b with keyword assignments (change 19, seed 488935: after the evaluator's handler ran
/// inside the function g, sbl negated an error that dotnet let end the EVAL). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AnEvalInAFunctionsStatement
{
    [TestMethod]
    public void TEST_AnEvalInAFunctionsStatement_goes_on_from_an_error_at_each_level()
    {
        var program = @"        &ERRLIMIT = 100
        DEFINE('T()')                                   :(TE)
T       T = EVAL(""~$''"")                                :S(RETURN)F(FRETURN)
TE      DEFINE('U()')                                   :(UE)
U       U = T()                                         :S(RETURN)F(FRETURN)
UE      R = EVAL(""~$''"")                                :S(Y1)
        OUTPUT = '1 level 0: F ' &ERRTYPE               :(E1)
Y1      OUTPUT = '1 level 0: S'
E1      R = T()                                         :S(Y2)
        OUTPUT = '2 in a function: F ' &ERRTYPE         :(E2)
Y2      OUTPUT = '2 in a function: S'
E2      R = U()                                         :S(Y3)
        OUTPUT = '3 two levels down: F ' &ERRTYPE       :(E3)
Y3      OUTPUT = '3 two levels down: S'
E3      R = EVAL('T()')                                 :S(Y4)
        OUTPUT = '4 EVAL of a function that EVALs: F ' &ERRTYPE   :(E4)
Y4      OUTPUT = '4 EVAL of a function that EVALs: S'
E4
END";
        Assert.AreEqual(@"1 level 0: F 239
2 in a function: S
3 two levels down: S
4 EVAL of a function that EVALs: S", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_AnEvalInAFunctionsStatement_choices_and_expression_objects()
    {
        var program = @"        &ERRLIMIT = 100
        N = 0
        DEFINE('T(S)')                                  :(TE)
T       T = EVAL(S)                                     :S(RETURN)F(FRETURN)
TE      DEFINE('TX(E)')                                 :(TXE)
TX      TX = EVAL(E)                                    :S(RETURN)F(FRETURN)
TXE     R = T(""(1 / N, 'd')"")                           :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                        :(E1)
Y1      OUTPUT = '1 S [' R ']'
E1      R = TX(*((1 / N), 'e'))                         :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                        :(E2)
Y2      OUTPUT = '2 S [' R ']'
E2      R = T(""(EVAL('1 / N'), 'g')"")                   :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                        :(E3)
Y3      OUTPUT = '3 S [' R ']'
E3      R = T(""EVAL('~(1 / N)')"")                       :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                        :(E4)
Y4      OUTPUT = '4 S [' R ']'
E4      &ERRLIMIT = 0
        R = T('~(1 / N)')                               :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE                        :(E5)
Y5      OUTPUT = '5 S [' R ']'
E5      R = T(""(1 / N, 'f')"")                           :S(Y6)
        OUTPUT = '6 F ' &ERRTYPE                        :(E6)
Y6      OUTPUT = '6 S [' R ']'
E6      R = EVAL(""T('~(1 / N)')"")                       :S(Y7)
        OUTPUT = '7 F ' &ERRTYPE                        :(E7)
Y7      OUTPUT = '7 S [' R ']'
E7      OUTPUT = 'end ' &ERRLIMIT
END";
        Assert.AreEqual(@"1 S [d]
2 S [e]
3 S [g]
4 F 14
5 S []
6 S [f]
7 S []
end 0", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_AnEvalInAFunctionsStatement_nested_in_another_evaluation_fails_whole()
    {
        var program = @"        &ERRLIMIT = 100
        N = 0
        DEFINE('T3()')                                  :(T3E)
T3      T3 = 'p' EVAL(""(1 / N, 'x')"")                   :S(RETURN)F(FRETURN)
T3E     DEFINE('T4()')                                  :(T4E)
T4      T4 = SIZE(EVAL(""(1 / N, 'xy')""))                :S(RETURN)F(FRETURN)
T4E     DEFINE('T6()')                                  :(T6E)
T6      T6 = EVAL(""~EVAL('~(1 / N)')"")                  :S(RETURN)F(FRETURN)
T6E     OUTPUT = '3 ' (T3(), 'F')
        OUTPUT = '4 ' (T4(), 'F')
        OUTPUT = '6 ' (T6() 'S', 'F')
END";
        Assert.AreEqual(@"3 px
4 2
6 S", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_AnEvalInAFunctionsStatement_in_a_handler_run_inside_a_function()
    {
        var program = @"        &ERRLIMIT = 100
        DEFINE('G(X)')                                  :(GE)
G       G = A[X]                                        :(RETURN)
GE      OUTPUT = '0 level ' &FNCLEVEL
        SETEXIT('H')
        R = EVAL('G(1)')
        OUTPUT = 'not trapped'                          :(END)
H       OUTPUT = '1 handler level ' &FNCLEVEL ' type ' &ERRTYPE
        R = EVAL(""~$''"")                                :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                        :(E2)
Y2      OUTPUT = '2 S [' R ']'
E2      R = ~$''                                        :S(Y3)
        OUTPUT = '3 F'                                  :(E3)
Y3      OUTPUT = '3 S'
E3      OUTPUT = '4 level ' &FNCLEVEL
END";
        Assert.AreEqual(@"0 level 0
1 handler level 1 type 235
2 S []
3 S
4 level 1", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
