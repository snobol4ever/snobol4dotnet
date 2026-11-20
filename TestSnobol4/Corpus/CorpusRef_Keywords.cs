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
}
