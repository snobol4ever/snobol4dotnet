using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// The negation ~X gives a new value and never changes X's: it fails when X succeeds, and gives the null string when X fails (so ~~X is the
/// null string), and an error in X is an error, not a failure to negate, as in SPITBOL. It marked the very value X holds as failed, so X read
/// as failed after ~X (OUTPUT = X printed nothing, Y = X assigned the null string, a later ?FAIL gave a pattern), ~~X gave X, and
/// 'a' + ~$Q with Q null negated error 239 into success and reported 1. Found by the infinite_snobol4 generator with ~ ? $ . added to its
/// unary operators. Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NegationValue
{
    [TestMethod]
    public void TEST_NegationValue_is_new_and_errors_pass_through()
    {
        var program = @"        &ERRLIMIT = 10
        X = 'abc'
        V = ~X                                      :S(Y1)
        OUTPUT = '1 F'                              :(E1)
Y1      OUTPUT = '1 S'
E1      OUTPUT = '2 [' X ']'
        Y = X                                       :S(Y3)
        OUTPUT = '3 F'                              :(E3)
Y3      OUTPUT = '3 S [' Y ']'
E3      V = ?FAIL
        OUTPUT = '4 ' DATATYPE(V)
        V = ~FAIL                                   :S(Y5)
        OUTPUT = '5 F'                              :(E5)
Y5      OUTPUT = '5 S'
E5      V = ?FAIL                                   :S(Y6)
        OUTPUT = '6 F'                              :(E6)
Y6      OUTPUT = '6 S ' DATATYPE(V)
E6      V = 'q' X                                   :S(Y7)
        OUTPUT = '7 F'                              :(E7)
Y7      OUTPUT = '7 S [' V ']'
E7      V = EVAL('~~X')
        OUTPUT = '8 [' V ']'
        V = 'a' + ~$Q                               :S(Y9)
        OUTPUT = '9 F ' &ERRTYPE                    :(E9)
Y9      OUTPUT = '9 S'
E9      V = 'p' ~~LEN(0)
        OUTPUT = '10 ' DATATYPE(V) ' [' V ']'
        V = ~DIFFER(1, 1) 'z'
        OUTPUT = '11 [' V ']'
END";
        Assert.AreEqual(@"1 F
2 [abc]
3 S [abc]
4 STRING
5 F
6 S STRING
7 S [qabc]
8 []
9 F 1
10 STRING [p]
11 [z]", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
