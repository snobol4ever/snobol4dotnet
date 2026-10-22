using Test.TestLexer;

namespace Test.StringSynthesis;

/// <summary>
/// How DUPL, SUBSTR, LPAD, RPAD and REPLACE take their arguments, as sbl does: the order the arguments are looked at (and so the
/// error that is reported when several are wrong), what a negative or too-large count means, and the results at the edges.
/// Every expected line is what sbl -b printed for the same program. The operands are variables, since constants are folded, and
/// an error in them reported, at compile time.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ArgumentRules
{
    private const string Header = "        &ERRLIMIT = 20\n        &TRIM = 0\n        S = 'abcdef'\n        AB = 'ab'\n        E = ''\n        NEG = -2\n        BAD = 'x'\n        BIG = 16777217\n        MAX = 16777216\n        HUGE = 9223372036854775807\n        P = LEN(2)\n";

    private static string Run(string body) => SetupTests.RunWithInput(Header + body + "END");

    #region SUBSTR

    [TestMethod]
    public void TEST_Substr_start_just_past_the_end_without_a_length_is_null()
    {
        Assert.AreEqual("[] 0", Run("        X = SUBSTR(S, 7)\n        OUTPUT = '[' X '] ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Substr_start_two_past_the_end_fails()
    {
        Assert.AreEqual("fails", Run("        X = SUBSTR(S, 8)   :F(NO)\n        OUTPUT = 'ok'   :(END)\nNO      OUTPUT = 'fails'\n"));
    }

    [TestMethod]
    public void TEST_Substr_a_negative_length_fails_before_the_start_is_looked_at()
    {
        Assert.AreEqual("fails 0", Run("        X = SUBSTR(S, BAD, NEG)   :F(NO)\n        OUTPUT = 'ok'   :(END)\nNO      OUTPUT = 'fails ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Substr_the_third_argument_is_the_first_one_checked_192()
    {
        Assert.AreEqual("192", Run("        X = SUBSTR(BAD, BAD, BAD)\n        OUTPUT = &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Substr_the_second_argument_is_checked_before_the_first_193()
    {
        Assert.AreEqual("193", Run("        X = SUBSTR(.V, BAD, 1)\n        OUTPUT = &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Substr_a_huge_start_fails()
    {
        Assert.AreEqual("fails 0", Run("        X = SUBSTR(S, HUGE, 1)   :F(NO)\n        OUTPUT = 'ok'   :(END)\nNO      OUTPUT = 'fails ' &ERRTYPE\n"));
    }

    #endregion

    #region DUPL

    [TestMethod]
    public void TEST_Dupl_a_negative_count_fails()
    {
        Assert.AreEqual("fails 0", Run("        X = DUPL(AB, NEG)   :F(NO)\n        OUTPUT = 'ok'   :(END)\nNO      OUTPUT = 'fails ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Dupl_a_count_above_the_largest_object_fails_even_for_a_null_string()
    {
        Assert.AreEqual("fails 0\nfails 0", Run(
            "        X = DUPL(E, HUGE)   :F(NO1)\n        OUTPUT = 'ok'   :(N1)\nNO1     OUTPUT = 'fails ' &ERRTYPE\n" +
            "N1      X = DUPL(AB, BIG)   :F(NO2)\n        OUTPUT = 'ok'   :(END)\nNO2     OUTPUT = 'fails ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Dupl_a_null_string_at_the_largest_count_is_null()
    {
        Assert.AreEqual("[] 0", Run("        X = DUPL(E, MAX)\n        OUTPUT = '[' X '] ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Dupl_a_result_longer_than_maxlngth_is_205()
    {
        Assert.AreEqual("fails 205", Run("        X = DUPL(AB, MAX)   :F(NO)\n        OUTPUT = 'ok'   :(END)\nNO      OUTPUT = 'fails ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Dupl_a_count_that_is_not_an_integer_is_90_for_a_string_and_for_a_pattern()
    {
        Assert.AreEqual("90\n90", Run("        X = DUPL(AB, BAD)\n        OUTPUT = &ERRTYPE\n        &ERRTYPE = 0\n        X = DUPL(P, BAD)\n        OUTPUT = &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Dupl_a_pattern_is_duplicated_and_zero_times_is_the_null_pattern()
    {
        Assert.AreEqual("PATTERN\n[abcd]\nPATTERN\n[]", Run(
            "        X = DUPL(P, 2)\n        OUTPUT = DATATYPE(X)\n        'abcd' ? X . R\n        OUTPUT = '[' R ']'\n" +
            "        X = DUPL(P, 0)\n        OUTPUT = DATATYPE(X)\n        'abcd' ? X . R\n        OUTPUT = '[' R ']'\n"));
    }

    [TestMethod]
    public void TEST_Dupl_an_unevaluated_expression_is_a_pattern()
    {
        Assert.AreEqual("PATTERN 0", Run("        EX = *S\n        X = DUPL(EX, 2)\n        OUTPUT = DATATYPE(X) ' ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Dupl_a_table_is_91()
    {
        Assert.AreEqual("91", Run("        T = TABLE()\n        X = DUPL(T, 2)\n        OUTPUT = &ERRTYPE\n"));
    }

    #endregion

    #region LPAD and RPAD

    [TestMethod]
    public void TEST_Lpad_a_negative_or_too_large_length_returns_the_string_unchanged()
    {
        Assert.AreEqual("[abcdef]\n[abcdef]\n6\n6", Run(
            "        OUTPUT = '[' LPAD(S, NEG) ']'\n        OUTPUT = '[' LPAD(S, NEG, '*') ']'\n        OUTPUT = SIZE(LPAD(S, BIG))\n        OUTPUT = SIZE(LPAD(S, HUGE))\n"));
    }

    [TestMethod]
    public void TEST_Rpad_a_negative_or_too_large_length_returns_the_string_unchanged()
    {
        Assert.AreEqual("[abcdef]\n6\n6", Run(
            "        OUTPUT = '[' RPAD(S, NEG) ']'\n        OUTPUT = SIZE(RPAD(S, BIG))\n        OUTPUT = SIZE(RPAD(S, HUGE))\n"));
    }

    [TestMethod]
    public void TEST_Lpad_the_third_argument_is_checked_first_then_the_second_then_the_first()
    {
        Assert.AreEqual("145\n144\n146", Run(
            "        EX = *S\n        X = LPAD(BAD, BAD, BAD)\n        OUTPUT = &ERRTYPE\n        &ERRTYPE = 0\n" +
            "        X = LPAD(S, 5, EX)\n        OUTPUT = &ERRTYPE\n        &ERRTYPE = 0\n        X = LPAD(EX, 5)\n        OUTPUT = &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Rpad_the_third_argument_is_checked_first_then_the_second_then_the_first()
    {
        Assert.AreEqual("179\n178\n180", Run(
            "        EX = *S\n        X = RPAD(BAD, BAD, BAD)\n        OUTPUT = &ERRTYPE\n        &ERRTYPE = 0\n" +
            "        X = RPAD(S, 5, EX)\n        OUTPUT = &ERRTYPE\n        &ERRTYPE = 0\n        X = RPAD(EX, 5)\n        OUTPUT = &ERRTYPE\n"));
    }

    #endregion

    #region REPLACE

    [TestMethod]
    public void TEST_Replace_a_character_given_twice_takes_its_last_replacement()
    {
        Assert.AreEqual("[bynyny]\n[yznznz]", Run(
            "        OUTPUT = '[' REPLACE('banana', 'aa', 'xy') ']'\n        OUTPUT = '[' REPLACE('banana', 'aba', 'xyz') ']'\n"));
    }

    [TestMethod]
    public void TEST_Replace_unequal_lengths_are_171_before_the_first_argument_is_looked_at()
    {
        Assert.AreEqual("171", Run("        EX = *S\n        X = REPLACE(EX, E, E)\n        OUTPUT = &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Replace_the_third_argument_is_checked_first_then_the_second_then_the_first()
    {
        Assert.AreEqual("170\n169\n168", Run(
            "        EX = *S\n        CD = 'cd'\n        X = REPLACE(EX, AB, CD)\n        OUTPUT = &ERRTYPE\n        &ERRTYPE = 0\n" +
            "        X = REPLACE(EX, EX, CD)\n        OUTPUT = &ERRTYPE\n        &ERRTYPE = 0\n        X = REPLACE(EX, AB, EX)\n        OUTPUT = &ERRTYPE\n"));
    }

    #endregion
}
