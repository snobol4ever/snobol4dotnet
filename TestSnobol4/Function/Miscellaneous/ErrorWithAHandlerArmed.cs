using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// With a SETEXIT handler armed, an error goes to the handler and nothing after it in the statement is evaluated, as in SPITBOL: the
/// negation of an operand in error is not taken ('a' + ~$Q enters the handler with 239) and a choice does not go on to its next
/// alternative ((-A, (Y = 2)) leaves Y as it was). With no handler both go on, as sbl does (~$Q succeeds, the choice assigns Y).
/// dotnet went on either way, so a later error overwrote &amp;ERRTYPE (1) and the next alternative's assignment was made. Found by the
/// infinite_snobol4 generator, whose evaluator arms a handler for every test. Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ErrorWithAHandlerArmed
{
    [TestMethod]
    public void TEST_ErrorWithAHandlerArmed_ends_the_statements_evaluation()
    {
        var program = @"        &ERRLIMIT = 10
        A = 'f'
        V = ~$Q                                     :S(Y0)
        OUTPUT = '0 F ' &ERRTYPE                    :(E0)
Y0      OUTPUT = '0 S ' &ERRTYPE ' [' V ']'
E0      SETEXIT('H1')
        V = 'a' + ~$Q                               :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                    :(E1)
Y1      OUTPUT = '1 S'                              :(E1)
H1      OUTPUT = '1 H ' &ERRTYPE
E1      SETEXIT('H2')
        V = (-A, (Y = 2))                           :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE ' Y=' Y            :(E2)
Y2      OUTPUT = '2 S Y=' Y                         :(E2)
H2      OUTPUT = '2 H ' &ERRTYPE ' Y=' Y
E2      V = (-A, (Y = 3))                           :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE ' Y=' Y            :(E3)
Y3      OUTPUT = '3 S Y=' Y
E3
END";
        Assert.AreEqual(@"0 S 239 []
1 H 239
2 H 10 Y=
3 S Y=3", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
