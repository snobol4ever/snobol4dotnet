using Test.TestLexer;

namespace Test.ProgramControl;

/// <summary>
/// SETEXIT() cancels the handler, as in SPITBOL: it gives the label it replaces, and an error after it is no trap (with &amp;ERRLIMIT above
/// 0 the statement fails, in EVAL too), until SETEXIT names a label again. dotnet forgot the label but kept the number an error arms its
/// trap from, so the next error went to the cancelled handler. Found by the infinite_snobol4 generator under -b with keyword assignments
/// (change 19, seed 488935: after the evaluator's handler was cancelled, errors went to it and the run ended). Every expected line is
/// what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SetExitWithNoLabel
{
    [TestMethod]
    public void TEST_SetExitWithNoLabel_cancels_the_handler()
    {
        var program = @"        &ERRLIMIT = 10
        N = 0
        SETEXIT('HANDLER')
        P = SETEXIT()
        OUTPUT = '1 previous [' P ']'
        V = 1 / N                                       :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                        :(E2)
Y2      OUTPUT = '2 S'
E2      V = EVAL('H(1)')                                :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                        :(E3)
Y3      OUTPUT = '3 S'
E3      P = SETEXIT()
        OUTPUT = '4 previous [' P ']'
        SETEXIT('HANDLER')
        V = 1 / N
        OUTPUT = '5 not reached'                        :(END)
HANDLER OUTPUT = '6 handler ' &ERRTYPE
END";
        Assert.AreEqual(@"1 previous [HANDLER]
2 F 14
3 F 22
4 previous []
6 handler 14", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
