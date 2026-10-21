using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SPITBOL compiles an arithmetic operator, or a call of a function it pre-evaluates (LEN, TAB, CHAR, REMDR, REPLACE and the
/// other functions its sources mark svpre), whose operands are all constants by evaluating it at once. An error in it is an
/// error in the statement being compiled: it is reported on the listing page with no column, and nothing runs. Every expected
/// text below is what sbl -b printed for the same program saved as Test.sno, with the head's identity and clock lines masked.
/// </summary>
[DoNotParallelize]
[TestClass]
public class PreEvaluation
{
    private static readonly string Head = "\n\n\n<identity>\n<clock>\n\n\n\n" + new string(' ', 112) + "page 1\n\n";

    private static string Run(string script) => SetupTests.MaskListingHead(SetupTests.RunWithInput(script));

    [TestMethod]
    public void TEST_PreEvaluation_a_division_by_a_constant_zero_is_a_compile_error_and_nothing_runs()
    {
        var s = "        OUTPUT = 'before'\n        X = 1 / 0\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual(
            Head + "2               X = 1 / 0\n\nTest.sno(2) : ERROR 014 -- division caused integer overflow",
            Run(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_a_constant_that_a_constant_makes_is_pre_evaluated_too()
    {
        var s = "        OUTPUT = 'before'\n        X = LEN(2 - 3)\nEND";
        Assert.AreEqual(
            Head + "2               X = LEN(2 - 3)\n\nTest.sno(2) : ERROR 121 -- len argument is negative or too large",
            Run(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_every_erring_statement_is_reported_and_the_others_are_not_listed()
    {
        var s = "        Y = 'a' + 1\n        Z = -'b'\n        OUTPUT = 'ok'\n        W = 3 ** 'c'\nEND";
        Assert.AreEqual(
            Head +
            "1               Y = 'a' + 1\n\nTest.sno(1) : ERROR 001 -- addition left operand is not numeric\n\n" +
            "2               Z = -'b'\n\nTest.sno(2) : ERROR 010 -- negation operand is not numeric\n\n" +
            "4               W = 3 ** 'c'\n\nTest.sno(4) : ERROR 015 -- exponentiation right operand is not numeric",
            Run(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_a_deferred_expression_is_pre_evaluated_where_it_stands()
    {
        var s = "        P = *(1 / 0)\n        OUTPUT = 'x'\nEND";
        Assert.AreEqual(
            Head + "1               P = *(1 / 0)\n\nTest.sno(1) : ERROR 014 -- division caused integer overflow",
            Run(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_a_subscript_is_pre_evaluated()
    {
        var s = "        A = ARRAY(3)\n        A<1 / 0> = 1\n        OUTPUT = 'x'\nEND";
        Assert.AreEqual(
            Head + "2               A<1 / 0> = 1\n\nTest.sno(2) : ERROR 014 -- division caused integer overflow",
            Run(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_a_variable_operand_is_not_a_constant_and_errs_when_the_statement_runs()
    {
        var s = "        &ERRLIMIT = 10\n        N = -1\n        X = LEN(N)\n        OUTPUT = 'ran ' &ERRTYPE\nEND";
        Assert.AreEqual("ran 121", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_eval_and_code_of_a_constant_error_fail_with_its_number()
    {
        var s = "        &ERRLIMIT = 10\n        X = EVAL('1 / 0')\n        OUTPUT = 'eval ' &ERRTYPE\n" +
                "        &ERRTYPE = 0\n        C = CODE(' Y = LEN(-1)')\n        OUTPUT = 'code ' &ERRTYPE\n" +
                "        &ERRTYPE = 0\n        W = EVAL('2 - 3')\n        OUTPUT = 'w ' W\nEND";
        Assert.AreEqual("eval 14\ncode 121\nw -1", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_constants_that_do_not_err_run_as_before()
    {
        var s = "        X = 2 ** -1\n        Y = 'ab' ? LEN(1) . Z\n" +
                "        OUTPUT = X ' ' Z ' ' SIZE('abc') ' ' REMDR(7, 3) ' ' CHAR(65) ' ' LPAD('a', 3, '.')\nEND";
        Assert.AreEqual("0.5 a 3 1 A ..a", SetupTests.RunWithInput(s));
    }
}
