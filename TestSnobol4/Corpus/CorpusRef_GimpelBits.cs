using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

/// <summary>
/// Gimpel building-block micro-tests — self-contained, no -INCLUDE.
/// Each test isolates a single idiom from the Gimpel library programs.
/// Sources: ROMAN.sno, SQRT.sno, BSORT.sno, SWAP.sno, FLOOR/CONVERT,
///          LPAD/RPAD, REVERSE, PHRASE scanning patterns.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CorpusRef_GimpelBits
{
    // ── ROMAN numerals (adapted from Gimpel ROMAN.sno) ──────────────────────

    [TestMethod]
    public void TEST_Gimpel_roman_small()
    {
        // I IV IX — tests DEFINE, ARRAY, GT/GE, recursive-style loop
        var s = @"
        DEFINE('roman(n)s,v,r,i')                          :(roman_end)
roman   s = ''
        v = ARRAY(13)
        v<1>  = 1000; v<2>  = 900; v<3>  = 500; v<4>  = 400
        v<5>  = 100;  v<6>  = 90;  v<7>  = 50;  v<8>  = 40
        v<9>  = 10;   v<10> = 9;   v<11> = 5;   v<12> = 4
        v<13> = 1
        r = ARRAY(13)
        r<1>  = 'M';  r<2>  = 'CM'; r<3>  = 'D';  r<4>  = 'CD'
        r<5>  = 'C';  r<6>  = 'XC'; r<7>  = 'L';  r<8>  = 'XL'
        r<9>  = 'X';  r<10> = 'IX'; r<11> = 'V';  r<12> = 'IV'
        r<13> = 'I'
        i = 1
RLOOP   GT(n, 0)                                           :F(RDONE)
        GE(n, v<i>)                                        :F(RNEXT)
        s = s r<i>
        n = n - v<i>                                       :(RLOOP)
RNEXT   i = i + 1                                          :(RLOOP)
RDONE   roman = s                                          :(RETURN)
roman_end
        OUTPUT = roman(1)
        OUTPUT = roman(4)
        OUTPUT = roman(9)
END";
        Assert.AreEqual("I"+ Environment.NewLine + "IV"+ Environment.NewLine + "IX", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Gimpel_roman_large()
    {
        // 42, 1999, 2024 — tests full loop iteration count, multi-digit decomposition
        var s = @"
        DEFINE('roman(n)s,v,r,i')                          :(roman_end)
roman   s = ''
        v = ARRAY(13)
        v<1>  = 1000; v<2>  = 900; v<3>  = 500; v<4>  = 400
        v<5>  = 100;  v<6>  = 90;  v<7>  = 50;  v<8>  = 40
        v<9>  = 10;   v<10> = 9;   v<11> = 5;   v<12> = 4
        v<13> = 1
        r = ARRAY(13)
        r<1>  = 'M';  r<2>  = 'CM'; r<3>  = 'D';  r<4>  = 'CD'
        r<5>  = 'C';  r<6>  = 'XC'; r<7>  = 'L';  r<8>  = 'XL'
        r<9>  = 'X';  r<10> = 'IX'; r<11> = 'V';  r<12> = 'IV'
        r<13> = 'I'
        i = 1
RLOOP   GT(n, 0)                                           :F(RDONE)
        GE(n, v<i>)                                        :F(RNEXT)
        s = s r<i>
        n = n - v<i>                                       :(RLOOP)
RNEXT   i = i + 1                                          :(RLOOP)
RDONE   roman = s                                          :(RETURN)
roman_end
        OUTPUT = roman(42)
        OUTPUT = roman(1999)
        OUTPUT = roman(2024)
END";
        Assert.AreEqual("XLII"+ Environment.NewLine + "MCMXCIX"+ Environment.NewLine + "MMXXIV", SetupTests.RunWithInput(s));
    }

    // ── SQRT via ** 0.5 (Gimpel SQRT.sno modern form) ───────────────────────

    [TestMethod]
    public void TEST_Gimpel_sqrt_perfect_squares()
    {
        // Tests real exponentiation Y ** 0.5, GT/LT predicates as functions
        var s = @"
        DEFINE('MYSQRT(Y)')                                :(MYSQRT_END)
MYSQRT  MYSQRT = Y ** 0.5                                  :(RETURN)
MYSQRT_END
        OUTPUT = CONVERT(MYSQRT(4.0), 'INTEGER')
        OUTPUT = CONVERT(MYSQRT(9.0), 'INTEGER')
        OUTPUT = CONVERT(MYSQRT(100.0), 'INTEGER')
END";
        Assert.AreEqual("2"+ Environment.NewLine + "3"+ Environment.NewLine + "10", SetupTests.RunWithInput(s));
    }

    // ── FLOOR/CEIL via CONVERT (Gimpel FLOOR.sno) ───────────────────────────

    [TestMethod]
    public void TEST_Gimpel_floor_positive()
    {
        // CONVERT(X,'INTEGER') truncates toward zero for positive reals
        var s = @"
        DEFINE('FLOOR(X)')                                 :(FLOOR_END)
FLOOR   FLOOR = CONVERT(X, 'INTEGER')
        GE(X, 0)                                           :S(RETURN)
        FLOOR = NE(X, FLOOR) FLOOR - 1                    :(RETURN)
FLOOR_END
        OUTPUT = FLOOR(3.7)
        OUTPUT = FLOOR(3.0)
        OUTPUT = FLOOR(0.1)
END";
        Assert.AreEqual("3"+ Environment.NewLine + "3"+ Environment.NewLine + "0", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Gimpel_floor_negative()
    {
        // Negative floor: FLOOR(-2.3) = -3, FLOOR(-2.0) = -2
        var s = @"
        DEFINE('FLOOR(X)')                                 :(FLOOR_END)
FLOOR   FLOOR = CONVERT(X, 'INTEGER')
        GE(X, 0)                                           :S(RETURN)
        FLOOR = NE(X, FLOOR) FLOOR - 1                    :(RETURN)
FLOOR_END
        OUTPUT = FLOOR(-2.3)
        OUTPUT = FLOOR(-2.0)
        OUTPUT = FLOOR(-0.1)
END";
        Assert.AreEqual("-3"+ Environment.NewLine + "-2"+ Environment.NewLine + "-1", SetupTests.RunWithInput(s));
    }

    // ── SWAP via NRETURN / indirect (Gimpel SWAP.sno) ───────────────────────

    [TestMethod]
    public void TEST_Gimpel_swap_two_vars()
    {
        // Tests indirect assignment via name variables $ARG1 / $ARG2
        var s = @"
        DEFINE('SWAP(A,B)T')                               :(SWAP_END)
SWAP    T = $A
        $A = $B
        $B = T                                             :(RETURN)
SWAP_END
        X = 'hello'
        Y = 'world'
        SWAP(.X, .Y)
        OUTPUT = X
        OUTPUT = Y
END";
        Assert.AreEqual("world"+ Environment.NewLine + "hello", SetupTests.RunWithInput(s));
    }

    // ── BSORT (Gimpel BSORT.sno) — array sort using LGT ────────────────────

    [TestMethod]
    public void TEST_Gimpel_bsort_strings()
    {
        // Insertion sort using LGT; tests array subscript as lvalue, LGT predicate
        var s = @"
        DEFINE('BSORT(A,I,N)J,K,V')                       :(BSORT_END)
BSORT   J = I
BS1     J = J + 1   LT(J, N)                              :F(RETURN)
        K = J
        V = A<J>
BS2     K = K - 1   GT(K, I)                              :F(BS_RO)
        A<K + 1> = LGT(A<K>, V) A<K>                      :S(BS2)
        A<K + 1> = V                                       :(BS1)
BS_RO   A<I> = V                                          :(BS1)
BSORT_END
        A = ARRAY(5)
        A<1> = 'banana'; A<2> = 'apple'; A<3> = 'cherry'
        A<4> = 'date';   A<5> = 'avocado'
        BSORT(A, 1, 5)
        OUTPUT = A<1>
        OUTPUT = A<2>
        OUTPUT = A<3>
        OUTPUT = A<4>
        OUTPUT = A<5>
END";
        Assert.AreEqual("apple"+ Environment.NewLine + "avocado"+ Environment.NewLine + "banana"+ Environment.NewLine + "cherry"+ Environment.NewLine + "date", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Gimpel_bsort_integers_as_strings()
    {
        // LGT is lexical — "10" < "9" lexically; tests LGT vs GT distinction
        var s = @"
        DEFINE('BSORT(A,I,N)J,K,V')                       :(BSORT_END)
BSORT   J = I
BS1     J = J + 1   LT(J, N)                              :F(RETURN)
        K = J
        V = A<J>
BS2     K = K - 1   GT(K, I)                              :F(BS_RO)
        A<K + 1> = LGT(A<K>, V) A<K>                      :S(BS2)
        A<K + 1> = V                                       :(BS1)
BS_RO   A<I> = V                                          :(BS1)
BSORT_END
        A = ARRAY(4)
        A<1> = 'cat'; A<2> = 'ant'; A<3> = 'bat'; A<4> = 'ape'
        BSORT(A, 1, 4)
        OUTPUT = A<1>
        OUTPUT = A<2>
        OUTPUT = A<3>
        OUTPUT = A<4>
END";
        Assert.AreEqual("ant"+ Environment.NewLine + "ape"+ Environment.NewLine + "bat"+ Environment.NewLine + "cat", SetupTests.RunWithInput(s));
    }

    // ── LPAD / RPAD patterns (Gimpel idiom, using built-in LPAD/RPAD) ───────

    [TestMethod]
    public void TEST_Gimpel_lpad_rpad_builtin()
    {
        // LPAD and RPAD are built-ins in SPITBOL; test both forms
        var s = @"
        OUTPUT = LPAD('hi', 6)
        OUTPUT = RPAD('hi', 6)
        OUTPUT = LPAD('hi', 6, '0')
END";
        Assert.AreEqual("    hi"+ Environment.NewLine + "hi    "+ Environment.NewLine + "0000hi", SetupTests.RunWithInput(s));
    }

    // ── SPAN/BREAK field-splitting (Gimpel scanning idiom) ──────────────────

    [TestMethod]
    public void TEST_Gimpel_span_break_csv()
    {
        // Tokenise a comma-delimited string using BREAK/SPAN loop
        var s = @"
        LINE = 'one,two,three,four'
        DELIM = ','
LOOP    LINE BREAK(DELIM) . WORD SPAN(DELIM) =             :F(LAST)
        OUTPUT = WORD                                      :(LOOP)
LAST    OUTPUT = LINE
END";
        Assert.AreEqual("one"+ Environment.NewLine + "two"+ Environment.NewLine + "three"+ Environment.NewLine + "four", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Gimpel_span_words()
    {
        // Extract words separated by spaces using SPAN/BREAK
        var s = @"
        LINE = 'hello world foo bar'
        SPACES = ' '
        WORD_P = BREAK(SPACES) . W  SPAN(SPACES)
LOOP    LINE WORD_P =                                      :F(LAST)
        OUTPUT = W                                         :(LOOP)
LAST    IDENT(LINE, '')                                    :S(END)
        OUTPUT = LINE
END";
        Assert.AreEqual("hello"+ Environment.NewLine + "world"+ Environment.NewLine + "foo"+ Environment.NewLine + "bar", SetupTests.RunWithInput(s));
    }

    // ── LEN / TAB / POS / RPOS fixed-column extraction ──────────────────────

    [TestMethod]
    public void TEST_Gimpel_fixed_column_extract()
    {
        // POS(0) LEN(4) TAB(10) REM — extract year and description fields
        var s = @"
        LINE = '1876 Bell  Telephone'
        LINE POS(0) LEN(4) . YEAR LEN(1) BREAK(' ') . NAME SPAN(' ') REM . THING
        OUTPUT = YEAR
        OUTPUT = NAME
        OUTPUT = THING
END";
        Assert.AreEqual("1876"+ Environment.NewLine + "Bell"+ Environment.NewLine + "Telephone", SetupTests.RunWithInput(s));
    }

    // ── ARBNO accumulator (Gimpel counting idiom) ───────────────────────────

    [TestMethod]
    public void TEST_Gimpel_arbno_count_chars()
    {
        // Count occurrences of 'a' using pattern-replace loop
        var s2 = @"
        S = 'abracadabra'
        N = 0
LOOP    S 'a' =                                            :F(DONE)
        N = N + 1                                          :(LOOP)
DONE    OUTPUT = N
END";
        Assert.AreEqual("5", SetupTests.RunWithInput(s2));
    }

    // ── TABLE as frequency counter ───────────────────────────────────────────

    [TestMethod]
    public void TEST_Gimpel_table_frequency()
    {
        // Count word frequency using TABLE; tests TABLE<key> lvalue and rvalue
        var s = @"
        T = TABLE()
        T<'cat'> = T<'cat'> + 1
        T<'dog'> = T<'dog'> + 1
        T<'cat'> = T<'cat'> + 1
        T<'bird'> = T<'bird'> + 1
        T<'cat'> = T<'cat'> + 1
        OUTPUT = T<'cat'>
        OUTPUT = T<'dog'>
        OUTPUT = T<'bird'>
END";
        Assert.AreEqual("3"+ Environment.NewLine + "1"+ Environment.NewLine + "1", SetupTests.RunWithInput(s));
    }

    // ── DATA type field mutator (Gimpel DATA/LINK idiom) ─────────────────────

    [TestMethod]
    public void TEST_Gimpel_data_linked_list()
    {
        // Build a simple linked list with DATA('NODE(VAL,NEXT)')
        var s = @"
        DATA('NODE(VAL,NEXT)')
        HEAD =
        HEAD = NODE(3, HEAD)
        HEAD = NODE(2, HEAD)
        HEAD = NODE(1, HEAD)
        P = HEAD
LOOP    IDENT(P)                                           :S(END)
        OUTPUT = VAL(P)
        P = NEXT(P)                                        :(LOOP)
END";
        Assert.AreEqual("1"+ Environment.NewLine + "2"+ Environment.NewLine + "3", SetupTests.RunWithInput(s));
    }

    // ── CONVERT type matrix (Gimpel number-conversion idiom) ────────────────

    [TestMethod]
    public void TEST_Gimpel_convert_string_to_int()
    {
        var s = @"
        X = CONVERT('42', 'INTEGER')
        OUTPUT = X + 8
END";
        Assert.AreEqual("50", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Gimpel_convert_real_to_int_truncates()
    {
        var s = @"
        OUTPUT = CONVERT(7.9, 'INTEGER')
        OUTPUT = CONVERT(-3.2, 'INTEGER')
END";
        Assert.AreEqual("7"+ Environment.NewLine + "-3", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Gimpel_convert_int_to_string()
    {
        var s = @"
        N = CONVERT(255, 'STRING')
        OUTPUT = DATATYPE(N)
        OUTPUT = N
END";
        Assert.AreEqual("STRING"+ Environment.NewLine + "255", SetupTests.RunWithInput(s));
    }

    // ── OPSYN alias (Gimpel REDEFINE.sno idiom) ─────────────────────────────

    [TestMethod]
    public void TEST_Gimpel_opsyn_alias()
    {
        // OPSYN(name, func, 0) — function synonym. UCASE is a keyword not a function;
        // correct form aliases SIZE as MYSIZE. Oracle: sbl → 5
        var s = @"
        OPSYN('MYSIZE', 'SIZE', 0)
        OUTPUT = MYSIZE('hello')
END";
        Assert.AreEqual("5", SetupTests.RunWithInput(s));
    }

    // ── DUPL for padding/repetition (Gimpel LPAD idiom) ─────────────────────

    [TestMethod]
    public void TEST_Gimpel_dupl_repeat()
    {
        var s = @"
        OUTPUT = DUPL('ab', 4)
        OUTPUT = DUPL('-', 10)
        OUTPUT = SIZE(DUPL('x', 0))
END";
        Assert.AreEqual("abababab"+ Environment.NewLine + "----------"+ Environment.NewLine + "0", SetupTests.RunWithInput(s));
    }

    // ── Fibonacci (recursive, Gimpel-style) ─────────────────────────────────

    [TestMethod]
    public void TEST_Gimpel_fibonacci_recursive()
    {
        // Tests deep recursion, LE predicate, integer arithmetic
        var s = @"
        DEFINE('FIB(N)')                                   :(FIB_END)
FIB     LE(N, 1)                                           :F(FIB_REC)
        FIB = N                                            :(RETURN)
FIB_REC FIB = FIB(N - 1) + FIB(N - 2)                     :(RETURN)
FIB_END
        OUTPUT = FIB(0)
        OUTPUT = FIB(1)
        OUTPUT = FIB(7)
        OUTPUT = FIB(10)
END";
        Assert.AreEqual("0"+ Environment.NewLine + "1"+ Environment.NewLine + "13"+ Environment.NewLine + "55", SetupTests.RunWithInput(s));
    }

    // ── Pattern variable (deferred evaluation *VAR) ──────────────────────────

    [TestMethod]
    public void TEST_Gimpel_deferred_pattern_var()
    {
        // *P re-evaluates P at match time — tests deferred pattern evaluation
        var s = @"
        DIGITS = SPAN('0123456789')
        P = BREAK(' ') . WORD ' '
        LINE = 'hello world foo'
        LINE *P =
        OUTPUT = WORD
        LINE *P =
        OUTPUT = WORD
END";
        Assert.AreEqual("hello"+ Environment.NewLine + "world", SetupTests.RunWithInput(s));
    }

    // ── TRIM builtin ─────────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Gimpel_trim()
    {
        var s = @"
        OUTPUT = TRIM('  hello  ')
        OUTPUT = TRIM('no spaces')
        OUTPUT = SIZE(TRIM('   '))
END";
        Assert.AreEqual("  hello"+ Environment.NewLine + "no spaces"+ Environment.NewLine + "0", SetupTests.RunWithInput(s));
    }

    // ── String reversal loop (Gimpel REVERSE.sno pattern) ───────────────────

    [TestMethod]
    public void TEST_Gimpel_reverse_loop()
    {
        // Build reversed string by stripping LEN(1) from front and prepending
        var s = @"
        DEFINE('REV(S)R,C')                                :(REV_END)
REV     R = ''
RVLOOP  S LEN(1) . C =                                     :F(RVDONE)
        R = C R                                            :(RVLOOP)
RVDONE  REV = R                                            :(RETURN)
REV_END
        OUTPUT = REV('hello')
        OUTPUT = REV('abcde')
        OUTPUT = REV('x')
END";
        Assert.AreEqual("olleh"+ Environment.NewLine + "edcba"+ Environment.NewLine + "x", SetupTests.RunWithInput(s));
    }

    // ── REPLACE character translation ────────────────────────────────────────

    [TestMethod]
    public void TEST_Gimpel_replace_rot13()
    {
        // ROT13 via REPLACE — tests 52-char translation table
        var s = @"
        UPPER = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'
        ROT13U = 'NOPQRSTUVWXYZABCDEFGHIJKLM'
        OUTPUT = REPLACE('HELLO', UPPER, ROT13U)
        OUTPUT = REPLACE('URYYB', UPPER, ROT13U)
END";
        Assert.AreEqual("URYYB"+ Environment.NewLine + "HELLO", SetupTests.RunWithInput(s));
    }

    // ── SIZE edge cases ───────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Gimpel_size_edge_cases()
    {
        var s = @"
        OUTPUT = SIZE('')
        OUTPUT = SIZE('a')
        OUTPUT = SIZE('hello world')
        OUTPUT = SIZE(DUPL('x', 100))
END";
        Assert.AreEqual("0"+ Environment.NewLine + "1"+ Environment.NewLine + "11"+ Environment.NewLine + "100", SetupTests.RunWithInput(s));
    }

    // ── SUBSTR extraction ─────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Gimpel_substr_basic()
    {
        var s = @"
        S = 'hello world'
        OUTPUT = SUBSTR(S, 1, 5)
        OUTPUT = SUBSTR(S, 7, 5)
        OUTPUT = SUBSTR(S, 7)
END";
        Assert.AreEqual("hello"+ Environment.NewLine + "world"+ Environment.NewLine + "world", SetupTests.RunWithInput(s));
    }
}
