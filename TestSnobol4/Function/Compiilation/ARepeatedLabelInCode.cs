using Test.TestLexer;

namespace Test.Compiilation;

/// <summary>
/// A label repeated within one CODE text is no error, and the later statement is the label's, as sbl -bf answers (CODE('A OUTPUT = 1
/// :(E) ;A OUTPUT = 2 :(E)') then :(A) prints 2); in the program's source a repeated label is still error 217. snobol4dotnet refused the
/// CODE with 217, so snolib's DEXP of LOGB (LOGB LOGB = ... ;LOGB = ...) failed with 217 where sbl's fails with 221 at the missing operand.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ARepeatedLabelInCode
{
    [TestMethod]
    public void TEST_ARepeatedLabelInCode_the_later_statement_wins()
    {
        Assert.AreEqual("2\nend", SetupTests.RunWithInputDirectives("-bf", @"         C = CODE('A OUTPUT = 1 :(E) ;A OUTPUT = 2 :(E)')   :F(CF)
         :(A)
CF       OUTPUT = 'failed ' &ERRTYPE
E        OUTPUT = 'end'
END").Replace("\r\n", "\n").TrimEnd());
    }

    [TestMethod]
    public void TEST_ARepeatedLabelInCode_the_first_error_after_it_is_reported()
    {
        Assert.AreEqual("failed 221", SetupTests.RunWithInputDirectives("-bf", @"         C = CODE('LOGB LOGB = 1 ;LOGB = 2 ; :S(RETURN)F(FRETURN)')   :F(CF)
         OUTPUT = 'ok'   :(END)
CF       OUTPUT = 'failed ' &ERRTYPE
END").Replace("\r\n", "\n").TrimEnd());
    }
}
