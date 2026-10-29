using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A CODE whose text does not compile defines the labels of the statements before the one in error, and no other, as SPITBOL's
/// compiler has defined them when it meets the error: the label on the statement in error and the labels after it keep what they
/// were. The console defined them all, so a goto to such a label ran the statements after the error (found by measuring the CODE
/// road of DESIGN-PLAN random-codegen-scope step 11, where tests share labels). Every expectation is what sbl -b printed for the same
/// program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CodeFailedLabels
{
    [TestMethod]
    public void TEST_CodeFailedLabels_no_label_from_the_statement_in_error_on()
    {
        Assert.AreEqual("D failed 314\nold L2\nback 1\nold L3\nback 2", SetupTests.RunWithInput(@"        &ERRLIMIT = 10
        C = CODE(' OUTPUT = ""old"" :(B1);L2 OUTPUT = ""old L2"" :(B1);L3 OUTPUT = ""old L3"" :(B2)')
        D = CODE(' X = 1;L2 Y = SQRT(-0.5); OUTPUT = ""after"";L3 OUTPUT = ""new L3"" :(B2)')    :S(D1)
        OUTPUT = 'D failed ' &ERRTYPE
D1      SETEXIT('ERR')
        :(L2)
B1      OUTPUT = 'back 1'
        :(L3)
B2      OUTPUT = 'back 2'                  :(END)
ERR     OUTPUT = 'ERROR ' &ERRTYPE
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_CodeFailedLabels_a_label_before_the_error_is_defined()
    {
        Assert.AreEqual("D failed 314\nnew L2\nback", SetupTests.RunWithInput(@"        &ERRLIMIT = 10
        C = CODE(' OUTPUT = ""old"" :(BACK);L2 OUTPUT = ""old L2"" :(BACK)')
        D = CODE(' X = 1;L2 OUTPUT = ""new L2"" :(BACK); Y = SQRT(-0.5)')    :S(D1)
        OUTPUT = 'D failed ' &ERRTYPE
D1      SETEXIT('ERR')
        :(L2)
BACK    OUTPUT = 'back'                    :(END)
ERR     OUTPUT = 'ERROR ' &ERRTYPE
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
