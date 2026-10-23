using Test.TestLexer;

namespace Test.Operator.Unary;

/// <summary>
/// A unary * defers the whole unary expression after it, as in SPITBOL, when that expression holds a later * of its own: *$~*X is
/// *($(~(*X))), an EXPRESSION, and .*~*X and .*$*X are NAMEs. dotnet's lexer makes deferred expressions of the stars right to left, and
/// the first star met the expression the second had made and deferred nothing, so ~*X was evaluated at once (FAIL) and $*X was 239.
/// Found by the infinite_snobol4 generator under -b (.*$~*LLT('j', '1,000'), seed 413896). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AStarDefersAStarredExpression
{
    [TestMethod]
    public void TEST_AStarDefersAStarredExpression_the_whole_unary_expression_is_deferred()
    {
        var program = @"        &ERRLIMIT = 10
        X = 'x'
        Y = 'X'
        OUTPUT = '1 ' DATATYPE(*$~*X)
        OUTPUT = '2 ' DATATYPE(.*~*X)
        OUTPUT = '3 ' DATATYPE(.*$*X)
        E = *$~*X
        V = EVAL(E)                                     :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE                        :(E5)
Y5      OUTPUT = '5 S [' V ']'
E5      E = *$$*Y
        V = EVAL(E)                                     :S(Y6)
        OUTPUT = '6 F ' &ERRTYPE                        :(E6)
Y6      OUTPUT = '6 S [' V ']'
E6      'abc' ? LEN(1) . $(.*$*Y) LEN(1) . *$*Y
        OUTPUT = '7 ' X
END";
        Assert.AreEqual(@"1 EXPRESSION
2 NAME
3 NAME
5 F 0
6 F 239
7 x", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
