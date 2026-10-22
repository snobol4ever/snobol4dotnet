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

    // The keywords whose values never change (&ABORT &ALPHABET &ARB &BAL &FAIL &FENCE &LCASE &REM &SUCCEED &UCASE) are constants to
    // SPITBOL's compiler: -&FENCE ANY('') was compiled and failed at run time with ANY's 59, where sbl refuses it with 10. Found by the
    // infinite_snobol4 generator ('{vc}(g){d}' -&FENCE ANY(''))
    [TestMethod]
    public void TEST_PreEvaluation_a_keyword_whose_value_never_changes_is_a_constant()
    {
        var s = "        OUTPUT = 'before'\n        V = -&FENCE ANY('')\n        W = 1 + &ALPHABET\n        X = LEN(&LCASE)\n" +
                "        Y = &ARB | -&REM\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual(
            Head +
            "2               V = -&FENCE ANY('')\n\nTest.sno(2) : ERROR 010 -- negation operand is not numeric\n\n" +
            "3               W = 1 + &ALPHABET\n\nTest.sno(3) : ERROR 002 -- addition right operand is not numeric\n\n" +
            "4               X = LEN(&LCASE)\n\nTest.sno(4) : ERROR 120 -- len argument is not integer or expression\n\n" +
            "5               Y = &ARB | -&REM\n\nTest.sno(5) : ERROR 010 -- negation operand is not numeric",
            Run(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_a_constant_keyword_is_still_a_name_and_its_values_run_as_before()
    {
        var s = "        &ERRLIMIT = 10\n        &ALPHABET = 1\n" +
                "        OUTPUT = 'ran ' &ERRTYPE ' ' SIZE(&UCASE) ' ' SIZE(&ALPHABET) ' ' DATATYPE(&FENCE)\nEND";
        Assert.AreEqual("ran 209 26 256 PATTERN", SetupTests.RunWithInput(s));
    }

    // The predefined pattern variables (ABORT ARB BAL FAIL FENCE REM SUCCEED) are constants to SPITBOL's compiler as their keywords are, and
    // *X of a constant is a constant too, an expression: each was compiled and failed at run time, or did not fail at all (SIZE(ARB)), where
    // sbl refuses the statement. Found by the infinite_snobol4 generator (*+REM *p, ERROR 4; *'3.' ** '1,000' - -'v', ERROR 15 not 10)
    [TestMethod]
    public void TEST_PreEvaluation_a_pattern_variable_and_a_deferred_constant_are_constants()
    {
        var s = "        OUTPUT = 'before'\n        V = *+REM\n        W = *'x' + 1\n        X = SIZE(ARB)\n" +
                "        Y = *'3.' ** '1,000' - -'v'\n        Z = *SIZE('ab') + 'x'\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual(
            Head +
            "2               V = *+REM\n\nTest.sno(2) : ERROR 004 -- affirmation operand is not numeric\n\n" +
            "3               W = *'x' + 1\n\nTest.sno(3) : ERROR 001 -- addition left operand is not numeric\n\n" +
            "4               X = SIZE(ARB)\n\nTest.sno(4) : ERROR 189 -- size argument is not a string\n\n" +
            "5               Y = *'3.' ** '1,000' - -'v'\n\nTest.sno(5) : ERROR 015 -- exponentiation right operand is not numeric\n\n" +
            "6               Z = *SIZE('ab') + 'x'\n\nTest.sno(6) : ERROR 002 -- addition right operand is not numeric",
            Run(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_a_deferred_constant_and_a_pattern_variable_still_run_as_before()
    {
        var s = "        &ERRLIMIT = 10\n        V = *'x'\n" +
                "        OUTPUT = DATATYPE(V) ' ' DATATYPE(*SIZE('ab')) ' ' DATATYPE(REM) ' ' DATATYPE(LEN(*'x'))\n" +
                "        P = *X\n        OUTPUT = DATATYPE(P)\nEND";
        Assert.AreEqual("EXPRESSION EXPRESSION PATTERN PATTERN\nEXPRESSION", SetupTests.RunWithInput(s));
    }

    // A concatenation or an alternation of constants is a constant, and so is a capture of a constant into a fixed name (a variable, a
    // keyword, *X), as SPITBOL's compiler builds them: -('a' | 'b') and -ARBNO('a' | 'b') are 10 as they are compiled. A capture into an
    // element, an indirection or a call is not. Found by the infinite_snobol4 generator (u[a[1]] +m ? -ARBNO('a' | 'b'), 10 not 4)
    [TestMethod]
    public void TEST_PreEvaluation_a_pattern_built_of_constants_is_a_constant()
    {
        var s = "        OUTPUT = 'before'\n        V = -('a' | 'b')\n        W = ('a' 'b') + 1\n        X = -ARBNO('a' | 'b')\n" +
                "        Y = -('a' . Z)\n        Z = -(*X . Y)\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual(
            Head +
            "2               V = -('a' | 'b')\n\nTest.sno(2) : ERROR 010 -- negation operand is not numeric\n\n" +
            "3               W = ('a' 'b') + 1\n\nTest.sno(3) : ERROR 001 -- addition left operand is not numeric\n\n" +
            "4               X = -ARBNO('a' | 'b')\n\nTest.sno(4) : ERROR 010 -- negation operand is not numeric\n\n" +
            "5               Y = -('a' . Z)\n\nTest.sno(5) : ERROR 010 -- negation operand is not numeric\n\n" +
            "6               Z = -(*X . Y)\n\nTest.sno(6) : ERROR 010 -- negation operand is not numeric",
            Run(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_a_pattern_built_of_constants_still_runs_as_before()
    {
        var s = "        &ERRLIMIT = 10\n        V = ('1' '2') + 1\n        W = -('a' . A<1>)\n        OUTPUT = V ' ' &ERRTYPE\n" +
                "        X = -(X | 'b')\n        OUTPUT = &ERRTYPE ' ' DATATYPE('a' | 'b') ' ' SIZE('a' 'b')\nEND";
        Assert.AreEqual("13 235\n10 PATTERN 2", SetupTests.RunWithInput(s));
    }

    // ?C of a constant is the null string to SPITBOL's compiler (the interrogation of a value that is there), so ?')' / *X is 13 as it is
    // compiled; ~C and ?X are not folded. Found by the infinite_snobol4 generator ('' ?')' / *&LCASE -BREAK('ab'): 13 not 10)
    [TestMethod]
    public void TEST_PreEvaluation_an_interrogation_of_a_constant_is_the_null_string()
    {
        var s = "        OUTPUT = 'before'\n        V = ?')' / *X\n        W = ?')' + 'x'\n        Y = SIZE(?'a') + 'x'\n        OUTPUT = 'after'\nEND";
        Assert.AreEqual(
            Head +
            "2               V = ?')' / *X\n\nTest.sno(2) : ERROR 013 -- division right operand is not numeric\n\n" +
            "3               W = ?')' + 'x'\n\nTest.sno(3) : ERROR 002 -- addition right operand is not numeric\n\n" +
            "4               Y = SIZE(?'a') + 'x'\n\nTest.sno(4) : ERROR 002 -- addition right operand is not numeric",
            Run(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_an_interrogation_or_negation_still_runs_as_before()
    {
        var s = "        &ERRLIMIT = 10\n        V = (?'a') 'b'\n        W = ~')' + 'x'\n        OUTPUT = V ' ' &ERRTYPE ' [' W ']'\n" +
                "        X = ?X + 'x'\n        OUTPUT = &ERRTYPE\nEND";
        Assert.AreEqual("b 0 []\n2", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_PreEvaluation_constants_that_do_not_err_run_as_before()
    {
        var s = "        X = 2 ** -1\n        Y = 'ab' ? LEN(1) . Z\n" +
                "        OUTPUT = X ' ' Z ' ' SIZE('abc') ' ' REMDR(7, 3) ' ' CHAR(65) ' ' LPAD('a', 3, '.')\nEND";
        Assert.AreEqual("0.5 a 3 1 A ..a", SetupTests.RunWithInput(s));
    }
}
