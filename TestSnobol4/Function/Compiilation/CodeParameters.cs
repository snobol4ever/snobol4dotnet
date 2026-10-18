using Test.TestLexer;

namespace Test.Compilation;

/// <summary>
/// A function whose body CODE compiled sees its parameters even when their names first appear in
/// the CODE text, as in SPITBOL (sbl -b; Budne's atn compiles its parsers with CODE).
/// </summary>
[DoNotParallelize]
[TestClass]
public class CodeParameters
{
    [TestMethod]
    public void TEST_CodeParameters_a_parameter_first_named_in_code_text_receives_its_argument()
    {
        Assert.AreEqual("F sees [lit]\nF sees [lit2]", SetupTests.RunWithInput(@"
        CODE(""F OUTPUT = 'F sees [' P ']' :(RETURN) ;"")
        DEFINE('F(P)X')
        F('lit')
        F('lit2')
end").Replace("\r\n", "\n"));
    }
}
