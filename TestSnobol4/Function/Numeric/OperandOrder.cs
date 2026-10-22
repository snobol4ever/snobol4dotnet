using Test.TestLexer;

namespace Test.Numeric;

/// <summary>
/// Which operand SPITBOL finds wrong when both are, the error numbers of LE, and the real results sbl gives for 0.0 / 0.0 and
/// REMDR. Every expected line is what sbl -b printed for the same program (the operands are variables, since constants are
/// folded, and an error in them reported, at compile time).
/// </summary>
[DoNotParallelize]
[TestClass]
public class OperandOrder
{
    private const string Header = "        &ERRLIMIT = 20\n        A = 'a'\n        B = 'b'\n        ONE = 1\n        Z = 0.\n        P = 12\n        Q = 3.7\n        N = -12\n";

    private static string Run(string body) => SetupTests.RunWithInput(Header + body + "END");

    [TestMethod]
    public void TEST_OperandOrder_addition_reports_the_right_operand_first()
    {
        Assert.AreEqual("add 2", Run("        X = A + B\n        OUTPUT = 'add ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_OperandOrder_division_reports_the_right_operand_first()
    {
        Assert.AreEqual("div 13", Run("        X = ONE / B\n        OUTPUT = 'div ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_OperandOrder_remdr_reports_the_second_argument_first()
    {
        Assert.AreEqual("remdr 165", Run("        X = REMDR(A, B)\n        OUTPUT = 'remdr ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_OperandOrder_eq_reports_the_second_argument_first()
    {
        Assert.AreEqual("eq 102", Run("        EQ(A, B)\n        OUTPUT = 'eq ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_OperandOrder_lt_reports_the_second_argument_first()
    {
        Assert.AreEqual("lt 148", Run("        LT(A, B)\n        OUTPUT = 'lt ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_OperandOrder_le_errors_are_118_and_119()
    {
        Assert.AreEqual("le 119\nle1 118\nle2 119",
            Run("        LE(A, B)\n        OUTPUT = 'le ' &ERRTYPE\n        LE(A, ONE)\n        OUTPUT = 'le1 ' &ERRTYPE\n        LE(ONE, B)\n        OUTPUT = 'le2 ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_OperandOrder_zero_real_divided_by_zero_real_is_zero()
    {
        Assert.AreEqual("zero 0.\nzr 0.", Run("        X = Z / Z\n        OUTPUT = 'zero ' X\n        X = REMDR(Z, Z)\n        OUTPUT = 'zr ' X\n"));
    }

    [TestMethod]
    public void TEST_OperandOrder_real_remdr_is_n1_minus_chop_n1_over_n2_times_n2()
    {
        Assert.AreEqual("rem 0.899999999999999\nremn -0.899999999999999",
            Run("        X = REMDR(P, Q)\n        OUTPUT = 'rem ' X\n        X = REMDR(N, Q)\n        OUTPUT = 'remn ' X\n"));
    }
}
