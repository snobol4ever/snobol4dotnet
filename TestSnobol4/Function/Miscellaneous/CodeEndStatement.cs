using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// Compilation ends at the END statement, as in SPITBOL ("After reading the END statement, compilation ends"): in the text of CODE
/// nothing after an END is compiled, so a label after it is undefined (error 38) and a CODE that begins with END ends the program when
/// it is entered; on a source line nothing after END; is a statement either. The console compiled on past the END, so the label
/// was defined and the statements after it ran (found by measuring the CODE road of DESIGN-PLAN random-codegen-scope step 11:
/// CODE('END; :(inf_back)') entered by a direct goto went back to the evaluator instead of ending it). Every expectation is what
/// sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CodeEndStatement
{
    [TestMethod]
    public void TEST_CodeEndStatement_nothing_after_END_in_CODE_is_compiled()
    {
        Assert.AreEqual("ERROR 38 1\ncompiled CODE", SetupTests.RunWithInput(@"        &ERRLIMIT = 10
        SETEXIT('ERR')
        C = CODE(' X = 1 :(L2);END;L2 X = 2 :(BACK)')    :<C>
BACK    OUTPUT = 'back ' X                            :(NEXT)
ERR     OUTPUT = 'ERROR ' &ERRTYPE ' ' X
NEXT    D = CODE('END; OUTPUT = ""never""')
        OUTPUT = 'compiled ' DATATYPE(D)              :<D>
        OUTPUT = 'not reached'
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_CodeEndStatement_nothing_after_END_on_a_source_line_is_compiled()
    {
        Assert.AreEqual("one", SetupTests.RunWithInput(@"        OUTPUT = 'one'
END;L OUTPUT = 'two'").Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
