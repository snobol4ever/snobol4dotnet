using Test.TestLexer;

namespace Test.ProgramControl;

/// <summary>
/// The label SETEXIT reads from a string folds as &amp;CASE says at that moment, as the names a run reads from strings do, in SPITBOL:
/// under -b the label lab1 is compiled as LAB1, so with &amp;CASE 0 SETEXIT('lab1') finds no label (187) and SETEXIT('LAB1') does, and
/// with &amp;CASE 1 again SETEXIT('lab1') finds it. dotnet folded it by the compile switch, so SETEXIT('lab1') found LAB1 whatever
/// &amp;CASE said. Found by the infinite_snobol4 generator under -b with keyword assignments (change 19, seed 488935: after
/// 1000000 ? (&amp;CASE = 0) the evaluator's SETEXIT('inf_errh') is 187 in sbl). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SetExitLabelFoldsByCase
{
    [TestMethod]
    public void TEST_SetExitLabelFoldsByCase_as_case_says_at_that_moment()
    {
        var program = @"        &ERRLIMIT = 10
        SETEXIT('lab1')                                 :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                        :(E1)
Y1      OUTPUT = '1 S'
E1      SETEXIT()
        &CASE = 0
        SETEXIT('lab1')                                 :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                        :(E2)
Y2      OUTPUT = '2 S'
        SETEXIT()
E2      SETEXIT('LAB1')                                 :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                        :(E3)
Y3      OUTPUT = '3 S'
        SETEXIT()
E3      &CASE = 1
        SETEXIT('lab1')                                 :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                        :(E4)
Y4      OUTPUT = '4 S'
E4      SETEXIT()                                       :(END)
lab1    OUTPUT = 'handler'
END";
        Assert.AreEqual(@"1 S
2 F 187
3 S
4 S", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
