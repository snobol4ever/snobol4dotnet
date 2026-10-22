using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

/// <summary>
/// Corpus keywords/ — builtin predicates, keywords, lexical comparison.
/// Tests not already covered in SimpleOutput_CaptureKeywords.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CorpusRef_Keywords
{
    [TestMethod]
    public void TEST_Corpus_076_builtin_ident()
    {
        var s = @"
        IDENT('abc', 'abc')                                         :S(YES)F(NO)
YES     OUTPUT = 'equal'
        :(NEXT)
NO      OUTPUT = 'not equal'
NEXT    IDENT('abc', 'xyz')                                         :S(YES2)F(NO2)
YES2    OUTPUT = 'equal'
        :(END)
NO2     OUTPUT = 'not equal'
END";
        Assert.AreEqual("equal"+ Environment.NewLine + "not equal", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_082_keyword_stcount()
    {
        var s = @"
        X = 1
        X = 2
        GT(&STNO, 1)                                                :S(YES)F(NO)
YES     OUTPUT = 'stno ok'
        :(END)
NO      OUTPUT = 'wrong'
END";
        Assert.AreEqual("stno ok", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_098_keyword_anchor()
    {
        var s = @"
        &ANCHOR = 1
        X = 'hello world'
        X 'hello'                                                   :S(YES)F(NO)
YES     OUTPUT = 'anchored match ok'
        X 'world'                                                   :S(YES2)F(NO2)
YES2    OUTPUT = 'should not reach'
        :(END)
NO      OUTPUT = 'wrong'
        :(END)
NO2     OUTPUT = 'anchor prevented mid-string match'
END";
        Assert.AreEqual("anchored match ok"+ Environment.NewLine + "anchor prevented mid-string match", SetupTests.RunWithInput(s));
    }

    [TestMethod, Ignore("M-NET-PAT-PRIMITIVES: &ANCHOR='0' string coercion (ASGNIC) throws error 208 — fix pending")]
    public void TEST_Corpus_099_keyword_rw()
    {
        var s = @"
        differ(&ANCHOR, 0)                         :F(e001)
        OUTPUT = 'FAIL 099/001: &ANCHOR default'   :(END)
e001
        &ANCHOR = 1
        X = 'hello world'
        X 'hello'                                  :S(e002ok)F(e002)
e002    OUTPUT = 'FAIL 099/002: anchor=1 int'      :(END)
e002ok  X 'world'                                  :S(e002b)F(e002bok)
e002b   OUTPUT = 'FAIL 099/002b: anchor blocked mid' :(END)
e002bok
        &ANCHOR = '0'
        X = 'hello world'
        X 'world'                                  :S(e003ok)F(e003)
e003    OUTPUT = 'FAIL 099/003: anchor=0 str coerce' :(END)
e003ok
        differ(DATATYPE(&STLIMIT), 'INTEGER')      :F(e004)
        OUTPUT = 'FAIL 099/004: STLIMIT datatype'  :(END)
e004
        &ANCHOR = 1
        differ(&ANCHOR, 1)                         :F(e005)
        OUTPUT = 'FAIL 099/005: ANCHOR round-trip' :(END)
e005
        &ANCHOR = 0
        OUTPUT = 'PASS 099_keyword_rw (5/5)'
END";
        Assert.AreEqual("PASS 099_keyword_rw (5/5)", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_100_roman_numeral()
    {
        var s = @"
        DEFINE('roman(n)s,v,r,i')                                     :(roman_end)
roman   s = ''
        v = ARRAY(13)
        v<1> = 1000
        v<2> = 900
        v<3> = 500
        v<4> = 400
        v<5> = 100
        v<6> = 90
        v<7> = 50
        v<8> = 40
        v<9> = 10
        v<10> = 9
        v<11> = 5
        v<12> = 4
        v<13> = 1
        r = ARRAY(13)
        r<1> = 'M'
        r<2> = 'CM'
        r<3> = 'D'
        r<4> = 'CD'
        r<5> = 'C'
        r<6> = 'XC'
        r<7> = 'L'
        r<8> = 'XL'
        r<9> = 'X'
        r<10> = 'IX'
        r<11> = 'V'
        r<12> = 'IV'
        r<13> = 'I'
        i = 1
RLOOP   GT(n, 0)                                                       :F(RDONE)
        GE(n, v<i>)                                                    :F(RNEXT)
        s = s r<i>
        n = n - v<i>                                                   :(RLOOP)
RNEXT   i = i + 1                                                      :(RLOOP)
RDONE   roman = s                                                      :(RETURN)
roman_end
        OUTPUT = roman(1)
        OUTPUT = roman(4)
        OUTPUT = roman(9)
        OUTPUT = roman(42)
        OUTPUT = roman(1999)
        OUTPUT = roman(2024)
END";
        Assert.AreEqual("I"+ Environment.NewLine + "IV"+ Environment.NewLine + "IX"+ Environment.NewLine + "XLII"+ Environment.NewLine + "MCMXCIX"+ Environment.NewLine + "MMXXIV", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_077_builtin_differ()
    {
        var s = @"
        DIFFER('abc', 'xyz')                                        :S(YES)F(NO)
YES     OUTPUT = 'different'
        :(END)
NO      OUTPUT = 'same'
END";
        Assert.AreEqual("different", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_078_builtin_gt()
    {
        var s = @"
        GT(5, 3)                                                    :S(YES)F(NO)
YES     OUTPUT = '5 > 3'
        :(NEXT)
NO      OUTPUT = 'wrong'
NEXT    GT(3, 5)                                                    :S(YES2)F(NO2)
YES2    OUTPUT = 'wrong'
        :(END)
NO2     OUTPUT = '3 not > 5'
END";
        Assert.AreEqual("5 > 3"+ Environment.NewLine + "3 not > 5", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_079_builtin_lt_le_ge()
    {
        var s = @"
        LT(3, 5)                                                    :S(A)F(END)
A       OUTPUT = '3 < 5'
        LE(5, 5)                                                    :S(B)F(END)
B       OUTPUT = '5 <= 5'
        GE(7, 5)                                                    :S(C)F(END)
C       OUTPUT = '7 >= 5'
END";
        Assert.AreEqual("3 < 5"+ Environment.NewLine + "5 <= 5"+ Environment.NewLine + "7 >= 5", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_080_builtin_eq_ne()
    {
        var s = @"
        EQ(42, 42)                                                  :S(YES)F(NO)
YES     OUTPUT = '42 = 42'
        :(NEXT)
NO      OUTPUT = 'wrong'
NEXT    NE(42, 99)                                                  :S(YES2)F(NO2)
YES2    OUTPUT = '42 != 99'
        :(END)
NO2     OUTPUT = 'wrong'
END";
        Assert.AreEqual("42 = 42"+ Environment.NewLine + "42 != 99", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_081_builtin_datatype()
    {
        var s = @"
        OUTPUT = REPLACE(DATATYPE('hello'), &LCASE, &UCASE)
        OUTPUT = REPLACE(DATATYPE(42),      &LCASE, &UCASE)
        OUTPUT = REPLACE(DATATYPE(3.14),    &LCASE, &UCASE)
END";
        Assert.AreEqual("STRING"+ Environment.NewLine + "INTEGER"+ Environment.NewLine + "REAL", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_099_lexical_compare()
    {
        var s = @"
        LGT('b', 'a')                                               :S(A)F(END)
A       OUTPUT = 'b > a'
        LLT('a', 'b')                                               :S(B)F(END)
B       OUTPUT = 'a < b'
        LEQ('cat', 'cat')                                           :S(C)F(END)
C       OUTPUT = 'cat = cat'
        LNE('cat', 'dog')                                           :S(D)F(END)
D       OUTPUT = 'cat != dog'
END";
        Assert.AreEqual("b > a"+ Environment.NewLine + "a < b"+ Environment.NewLine + "cat = cat"+ Environment.NewLine + "cat != dog", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_kw_fnclevel()
    {
        // &FNCLEVEL = 0 at top level; increments inside function calls
        var s = @"
        DIFFER(&FNCLEVEL, 0)                        :F(ok1)
        OUTPUT = 'FAIL: &FNCLEVEL at top level'     :(END)
ok1
        DEFINE('chk()')                             :(chk_end)
chk     DIFFER(&FNCLEVEL, 1)                        :F(ok2)
        OUTPUT = 'FAIL: &FNCLEVEL inside call'      :(RETURN)
ok2                                                 :(RETURN)
chk_end
        chk()
        OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_kw_rtntype()
    {
        // &RTNTYPE: 'RETURN' after normal return, 'FRETURN' after failure return
        var s = @"
        DEFINE('ok_fn()')                           :(ok_fn_end)
ok_fn                                               :(RETURN)
ok_fn_end
        DEFINE('fail_fn()')                         :(fail_fn_end)
fail_fn                                             :(FRETURN)
fail_fn_end
        ok_fn()
        DIFFER(&RTNTYPE, 'RETURN')                  :F(ok1)
        OUTPUT = 'FAIL: &RTNTYPE after RETURN'      :(END)
ok1     fail_fn()                                   :F(ok2)
        OUTPUT = 'wrong branch'                     :(END)
ok2     DIFFER(&RTNTYPE, 'FRETURN')                 :F(ok3)
        OUTPUT = 'FAIL: &RTNTYPE after FRETURN'     :(END)
ok3     OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_kw_fullscan()
    {
        // &FULLSCAN = 1 enables full scan (no heuristic shortcut); &FULLSCAN = 0 is error 274, and leaves it 1, as sbl does
        var s = @"
        &ERRLIMIT = 5
        &FULLSCAN = 1
        DIFFER(&FULLSCAN, 1)                        :F(ok)
        OUTPUT = 'FAIL: &FULLSCAN not set'          :(END)
ok      &FULLSCAN = 0
        DIFFER(&ERRTYPE, 274)                       :F(ok2)
        OUTPUT = 'FAIL: &FULLSCAN = 0 not refused'  :(END)
ok2     DIFFER(&FULLSCAN, 1)                        :F(ok3)
        OUTPUT = 'FAIL: &FULLSCAN changed'          :(END)
ok3     OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_kw_maxlngth()
    {
        // &MAXLNGTH — max string length; default is positive; can be read and set
        var s = @"
        GT(&MAXLNGTH, 0)                            :S(ok1)
        OUTPUT = 'FAIL: &MAXLNGTH not positive'     :(END)
ok1     OLD = &MAXLNGTH
        &MAXLNGTH = 2000
        DIFFER(&MAXLNGTH, 2000)                     :F(ok2)
        OUTPUT = 'FAIL: &MAXLNGTH not set'          :(END)
ok2     &MAXLNGTH = OLD
        OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_kw_date_nocrash()
    {
        // DATE() returns a string — just verify it's non-empty and no crash
        var s = @"
        D = DATE()
        GT(SIZE(D), 0)                              :S(ok)
        OUTPUT = 'FAIL: DATE() empty'               :(END)
ok      OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_kw_time_nocrash()
    {
        // TIME() returns integer milliseconds — verify INTEGER type
        var s = @"
        T = TIME()
        INTEGER(T)                                  :S(ok)
        OUTPUT = 'FAIL: TIME() not integer'         :(END)
ok      OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_kw_lastno()
    {
        // &LASTNO — statement number increments as statements execute
        var s = @"
        X = 1
        X = X + 1
        X = X + 1
        GT(&LASTNO, 0)                              :S(ok)
        OUTPUT = 'FAIL: &LASTNO not positive'       :(END)
ok      OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }
}
