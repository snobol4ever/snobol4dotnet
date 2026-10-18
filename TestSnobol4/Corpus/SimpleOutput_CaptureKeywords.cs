using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

/// <summary>
/// Corpus capture/ — dot/dollar capture, replacement, conditional.
/// Corpus keywords/ — IDENT, DIFFER, GT, LT, LE, GE, EQ, NE, DATATYPE,
///                    &STNO, &ALPHABET/&UCASE/&LCASE, &ANCHOR, lexical compare.
/// </summary>
[TestClass]
public class SimpleOutput_CaptureKeywords
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    // -----------------------------------------------------------------------
    // capture/
    // -----------------------------------------------------------------------

    [TestMethod]
    public void TEST_Corpus_058_capture_dot_immediate()
    {
        var s = @"
        X = 'hello world'
        X LEN(5) . V                                               :S(YES)
        RESULT = 'fail'
        :(END)
YES     RESULT = V
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("hello", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_059_capture_dollar_deferred()
    {
        var s = @"
        X = 'hello world'
        X LEN(5) $ V                                               :S(YES)
        RESULT = 'fail'
        :(END)
YES     RESULT = V
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("hello", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_060_capture_multiple()
    {
        var s = @"
        X = 'John Smith'
        X BREAK(' ') . FIRST LEN(1) REM . LAST                    :S(YES)
        RESULT = 'fail'
        :(END)
YES     RESULT = FIRST '/' LAST
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("John/Smith", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_061_capture_in_loop()
    {
        var s = @"
        X = 'aaa'
        N = 0
        COUNT = 0
LOOP    X POS(N) 'a' . V                                           :F(DONE)
        COUNT = COUNT + 1
        N = N + 1
        :(LOOP)
DONE    RESULT = COUNT
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(3L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_062_capture_replacement()
    {
        var s = @"
        X = 'hello world'
        X 'world' = 'there'
        RESULT = X
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("hello there", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_063_capture_null_replace()
    {
        var s = @"
        X = 'hello world'
        X ' world' =
        RESULT = X
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("hello", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_064_capture_conditional()
    {
        var s = @"
        X = 'hello'
        X 'hello'                                                   :F(END)
        RESULT = 'found'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("found", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    // -----------------------------------------------------------------------
    // keywords/
    // -----------------------------------------------------------------------

    [TestMethod]
    public void TEST_Corpus_076_builtin_ident_equal()
    {
        var s = @"
        IDENT('abc', 'abc')                                         :S(YES)F(NO)
YES     RESULT = 'equal'
        :(END)
NO      RESULT = 'not equal'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("equal", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_076_builtin_ident_unequal()
    {
        var s = @"
        IDENT('abc', 'xyz')                                         :S(YES)F(NO)
YES     RESULT = 'equal'
        :(END)
NO      RESULT = 'not equal'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not equal", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_077_builtin_differ()
    {
        var s = @"
        DIFFER('abc', 'xyz')                                        :S(YES)F(NO)
YES     RESULT = 'different'
        :(END)
NO      RESULT = 'same'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("different", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_078_builtin_gt()
    {
        var s = @"
        GT(5, 3)                                                    :S(YES)F(NO)
YES     R1 = '5 > 3'
        :(NEXT)
NO      R1 = 'wrong'
NEXT    GT(3, 5)                                                    :S(YES2)F(NO2)
YES2    R2 = 'wrong'
        :(END)
NO2     R2 = '3 not > 5'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("5 > 3",    ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("3 not > 5",((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_079_builtin_lt_le_ge()
    {
        var s = @"
        LT(3, 5)                                                    :S(A)F(END)
A       R1 = '3 < 5'
        LE(5, 5)                                                    :S(B)F(END)
B       R2 = '5 <= 5'
        GE(7, 5)                                                    :S(C)F(END)
C       R3 = '7 >= 5'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3 < 5",  ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("5 <= 5", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
        Assert.AreEqual("7 >= 5", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_080_builtin_eq_ne()
    {
        var s = @"
        EQ(42, 42)                                                  :S(YES)F(NO)
YES     R1 = '42 = 42'
        :(NEXT)
NO      R1 = 'wrong'
NEXT    NE(42, 99)                                                  :S(YES2)F(NO2)
YES2    R2 = '42 != 99'
        :(END)
NO2     R2 = 'wrong'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42 = 42",  ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("42 != 99", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_081_builtin_datatype()
    {
        var s = @"
        R1 = DATATYPE('hello')
        R2 = DATATYPE(42)
        R3 = DATATYPE(3.14)
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // DOTNET DATATYPE returns lowercase type names
        Assert.AreEqual("STRING",  ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("INTEGER", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
        Assert.AreEqual("REAL",    ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_082_keyword_stno()
    {
        var s = @"
        X = 1
        X = 2
        GT(&STNO, 1)                                                :S(YES)F(NO)
YES     RESULT = 'stno ok'
        :(END)
NO      RESULT = 'wrong'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("stno ok", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_097_keyword_alphabet()
    {
        var s = @"
        R1 = SIZE(&ALPHABET)
        R2 = SIZE(&UCASE)
        R3 = SIZE(&LCASE)
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var r1 = ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data;
        Assert.AreEqual(256L, r1, $"&ALPHABET SIZE expected 256, got {r1}");
        var r2 = ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data;
        Assert.IsTrue(r2 >= 26L, $"&UCASE SIZE expected >= 26, got {r2}");
        var r3 = ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data;
        Assert.IsTrue(r3 >= 26L, $"&LCASE SIZE expected >= 26, got {r3}");
    }

    [TestMethod]
    public void TEST_Corpus_098_keyword_anchor_on()
    {
        var s = @"
        &ANCHOR = 1
        X = 'hello world'
        X 'hello'                                                   :S(YES)F(NO)
YES     R1 = 'anchored match ok'
        X 'world'                                                   :S(YES2)F(NO2)
YES2    R2 = 'should not reach'
        :(END)
NO      R2 = 'wrong'
        :(END)
NO2     R2 = 'anchor prevented mid-string match'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("anchored match ok",              ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("anchor prevented mid-string match", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_099_lexical_compare()
    {
        var s = @"
        LGT('b', 'a')                                               :S(A)F(END)
A       R1 = 'b > a'
        LLT('a', 'b')                                               :S(B)F(END)
B       R2 = 'a < b'
        LEQ('cat', 'cat')                                           :S(C)F(END)
C       R3 = 'cat = cat'
        LNE('cat', 'dog')                                           :S(D)F(END)
D       R4 = 'cat != dog'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("b > a",     ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("a < b",     ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
        Assert.AreEqual("cat = cat", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data);
        Assert.AreEqual("cat != dog",((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R4")]).Data);
    }
}
