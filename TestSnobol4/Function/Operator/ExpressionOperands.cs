using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// An expression (*5, or a variable that holds one) is not a number: E = *5 followed by E + 1, -E, LT(E, 6) or REMDR(E, 3) is the error
/// of a non-numeric operand, as in SPITBOL, and the statement fails. The console evaluated E and used the result, so that 2 + E was 7.
/// An expression is evaluated where SPITBOL evaluates one: in a pattern (LEN(*N)). The error numbers are what sbl -bf printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ExpressionOperands
{
    private static string Try(string expression) =>
        SetupTests.RunWithInput(
            $"        &ERRLIMIT = 100\n        E = *5\n        X = {expression}  :F(NO)\n        OUTPUT = 'val=' X  :(OUT)\nNO      OUTPUT = 'fail ' &ERRTYPE\nOUT     OUTPUT = '@b'\nEND")
            .Replace("\r\n", "\n");

    [TestMethod]
    public void TEST_ExpressionOperands_arithmetic_on_an_expression_is_an_error_of_the_operand()
    {
        foreach (var (expression, code) in new[]
                 {
                     ("E + 1", 1), ("E * 2", 26), ("E - 1", 32), ("E / 5", 12), ("E ^ 2", 16), ("-E", 10), ("+E", 4), ("REMDR(E,3)", 166),
                     ("REMDR(10,E)", 165)
                 })
            Assert.AreEqual($"fail {code}\n@b", Try(expression), expression);
    }

    [TestMethod]
    public void TEST_ExpressionOperands_a_comparison_of_an_expression_is_an_error_of_the_operand()
    {
        foreach (var (expression, code) in new[] { ("LT(E,6)", 147), ("LT(E,4)", 147), ("GT(E,4)", 111), ("GT(6,E)", 112), ("EQ(E,5)", 101), ("NE(E,5)", 149), ("LE(E,5)", 118) })
            Assert.AreEqual($"fail {code}\n@b", Try(expression), expression);
    }

    [TestMethod]
    public void TEST_ExpressionOperands_a_pattern_primitive_evaluates_an_expression_where_it_is_matched()
    {
        Assert.AreEqual("ab\nc", SetupTests.RunWithInput(
            "        N = 2\n        'abcd' ? LEN(*N) . X\n        OUTPUT = X\n        'abcd' ? POS(*N) LEN(1) . Y\n        OUTPUT = Y\nEND").Replace("\r\n", "\n"));
    }
}
