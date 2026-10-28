using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// The text of CODE and of EVAL is one line, as in SPITBOL: a line feed or a carriage return in it is no statement separator (only
/// the semicolon is), so outside a literal it is an illegal character, error 230, or at the head of a statement a bad label, error
/// 214, and the call fails; inside a literal it is a character of the string. The console split the text into lines there, so
/// CODE(' OUTPUT = 1' CHAR(10) ' OUTPUT = 2') compiled two statements (found by measuring the CODE road of DESIGN-PLAN
/// random-codegen-scope step 11). Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CodeLineEnds
{
    [TestMethod]
    public void TEST_CodeLineEnds_a_line_end_in_CODE_or_EVAL_text_is_no_separator()
    {
        Assert.AreEqual("LF 230\nCR 230\nEVAL 230\ntrailing 214\nliteral 3", SetupTests.RunWithInput(@"        &ERRLIMIT = 10
        C = CODE(' OUTPUT = 1' CHAR(10) ' OUTPUT = 2')    :F(F1)
        OUTPUT = 'compiled'                               :<C>
F1      OUTPUT = 'LF ' &ERRTYPE
        C = CODE(' OUTPUT = 3' CHAR(13) ' OUTPUT = 4')    :F(F2)
        OUTPUT = 'compiled CR'                            :<C>
F2      OUTPUT = 'CR ' &ERRTYPE
        X = EVAL('1' CHAR(10) ' + 2')                     :F(F3)
        OUTPUT = 'eval ' X                                :(F4)
F3      OUTPUT = 'EVAL ' &ERRTYPE
F4      C = CODE(' OUTPUT = 6;' CHAR(10))                 :F(F5)
        OUTPUT = 'trailing compiled'                      :<C>
F5      OUTPUT = 'trailing ' &ERRTYPE
        X = EVAL(""'a"" CHAR(10) ""b'"")
        OUTPUT = 'literal ' SIZE(X)
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
