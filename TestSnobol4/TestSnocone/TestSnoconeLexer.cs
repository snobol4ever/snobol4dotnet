using Snobol4.Common;
using static Snobol4.Common.SnoconeLexer;

namespace Test.TestSnocone;

/// <summary>
/// Unit tests for <see cref="SnoconeLexer"/>.
///
/// Covers:
///  1. Helpers — StripComment, IsContinuation, SplitSemicolon
///  2. Literals — integers, reals (including leading-dot), strings
///  3. Keywords — all 12 reserved words are classified correctly
///  4. Operators — every operator in the bconv table, longest-match
///  5. Punctuation — () {} [] , ; :
///  6. Statement boundaries — newline tokens, continuation joining
///  7. Semicolon splitting
///  8. End-to-end snippets from the snocone.sc corpus
/// </summary>
[TestClass]
public class TestSnoconeLexer
{
    // -------------------------------------------------------------------------
    // Helper: collect kinds from a source string (strip EOF + Newlines)
    // -------------------------------------------------------------------------
    private static List<ScKind> Kinds(string src) =>
        Tokenize(src)
            .Where(t => t.Kind != ScKind.Eof && t.Kind != ScKind.Newline)
            .Select(t => t.Kind)
            .ToList();

    private static List<string> Texts(string src) =>
        Tokenize(src)
            .Where(t => t.Kind != ScKind.Eof && t.Kind != ScKind.Newline)
            .Select(t => t.Text)
            .ToList();

    private static List<ScToken> Tokens(string src) =>
        Tokenize(src).Where(t => t.Kind != ScKind.Eof).ToList();

    // =========================================================================
    // 1. Helpers
    // =========================================================================

    [TestMethod]
    public void StripComment_NoComment_ReturnsLine()
    {
        Assert.AreEqual("x = 1", StripComment("x = 1"));
    }

    [TestMethod]
    public void StripComment_TrailingHash_Stripped()
    {
        Assert.AreEqual("x = 1 ", StripComment("x = 1 # comment"));
    }

    [TestMethod]
    public void StripComment_HashInsideSingleQuote_NotStripped()
    {
        Assert.AreEqual("x = 'a#b'", StripComment("x = 'a#b'"));
    }

    [TestMethod]
    public void StripComment_HashInsideDoubleQuote_NotStripped()
    {
        Assert.AreEqual("x = \"a#b\"", StripComment("x = \"a#b\""));
    }

    [TestMethod]
    public void StripComment_LeadingHash_EmptyResult()
    {
        Assert.AreEqual("", StripComment("# full comment line"));
    }

    [TestMethod]
    public void IsContinuation_EndsWithPlus_True()
    {
        Assert.IsTrue(IsContinuation("x = y +"));
    }

    [TestMethod]
    public void IsContinuation_EndsWithColon_True()
    {
        Assert.IsTrue(IsContinuation("x :"));
    }

    [TestMethod]
    public void IsContinuation_EndsWithComma_True()
    {
        Assert.IsTrue(IsContinuation("f(a,"));
    }

    [TestMethod]
    public void IsContinuation_EndsWithIdentifier_False()
    {
        Assert.IsFalse(IsContinuation("x = y"));
    }

    [TestMethod]
    public void IsContinuation_EmptyLine_False()
    {
        Assert.IsFalse(IsContinuation(""));
    }

    [TestMethod]
    public void IsContinuation_WhitespaceAfterContinuationChar_True()
    {
        // Whitespace is trimmed before the check
        Assert.IsTrue(IsContinuation("x + "));
    }

    [TestMethod]
    public void SplitSemicolon_NoSemicolon_SingleSegment()
    {
        var r = SplitSemicolon("x = 1");
        Assert.AreEqual(1, r.Count);
        Assert.AreEqual("x = 1", r[0]);
    }

    [TestMethod]
    public void SplitSemicolon_TwoSegments()
    {
        var r = SplitSemicolon("x = 1; y = 2");
        Assert.AreEqual(2, r.Count);
        Assert.AreEqual("x = 1", r[0]);
        Assert.AreEqual(" y = 2", r[1]);
    }

    [TestMethod]
    public void SplitSemicolon_SemicolonInsideString_NotSplit()
    {
        var r = SplitSemicolon("x = 'a;b'");
        Assert.AreEqual(1, r.Count);
        Assert.AreEqual("x = 'a;b'", r[0]);
    }

    // =========================================================================
    // 2. Literals
    // =========================================================================

    [TestMethod]
    public void Literal_Integer()
    {
        var toks = Tokens("42");
        Assert.AreEqual(ScKind.Integer, toks[0].Kind);
        Assert.AreEqual("42", toks[0].Text);
    }

    [TestMethod]
    public void Literal_Real_WithDecimal()
    {
        var toks = Tokens("3.14");
        Assert.AreEqual(ScKind.Real, toks[0].Kind);
        Assert.AreEqual("3.14", toks[0].Text);
    }

    [TestMethod]
    public void Literal_Real_WithExponent_LowerE()
    {
        var toks = Tokens("1e10");
        Assert.AreEqual(ScKind.Real, toks[0].Kind);
        Assert.AreEqual("1e10", toks[0].Text);
    }

    [TestMethod]
    public void Literal_Real_WithExponent_UpperE()
    {
        var toks = Tokens("2.5E-3");
        Assert.AreEqual(ScKind.Real, toks[0].Kind);
        Assert.AreEqual("2.5E-3", toks[0].Text);
    }

    [TestMethod]
    public void Literal_Real_WithExponent_D()
    {
        var toks = Tokens("1D2");
        Assert.AreEqual(ScKind.Real, toks[0].Kind);
    }

    [TestMethod]
    public void Literal_Real_LeadingDot()
    {
        // .5 is valid Snocone; the dotck() rewrite happens in the parser
        var toks = Tokens(".5");
        Assert.AreEqual(ScKind.Real, toks[0].Kind);
        Assert.AreEqual(".5", toks[0].Text);
    }

    [TestMethod]
    public void Literal_String_SingleQuote()
    {
        var toks = Tokens("'hello'");
        Assert.AreEqual(ScKind.String, toks[0].Kind);
        Assert.AreEqual("'hello'", toks[0].Text);
    }

    [TestMethod]
    public void Literal_String_DoubleQuote()
    {
        var toks = Tokens("\"world\"");
        Assert.AreEqual(ScKind.String, toks[0].Kind);
        Assert.AreEqual("\"world\"", toks[0].Text);
    }

    [TestMethod]
    public void Literal_String_ContainsSemicolon()
    {
        var toks = Tokens("'a;b'");
        Assert.AreEqual(ScKind.String, toks[0].Kind);
        Assert.AreEqual("'a;b'", toks[0].Text);
    }

    [TestMethod]
    public void Literal_String_ContainsHash()
    {
        var toks = Tokens("'a#b'");
        Assert.AreEqual(ScKind.String, toks[0].Kind);
        Assert.AreEqual("'a#b'", toks[0].Text);
    }

    // =========================================================================
    // 3. Keywords
    // =========================================================================

    [TestMethod] public void Keyword_if()        => Assert.AreEqual(ScKind.KwIf,        Kinds("if")[0]);
    [TestMethod] public void Keyword_else()      => Assert.AreEqual(ScKind.KwElse,      Kinds("else")[0]);
    [TestMethod] public void Keyword_while()     => Assert.AreEqual(ScKind.KwWhile,     Kinds("while")[0]);
    [TestMethod] public void Keyword_do()        => Assert.AreEqual(ScKind.KwDo,        Kinds("do")[0]);
    [TestMethod] public void Keyword_for()       => Assert.AreEqual(ScKind.KwFor,       Kinds("for")[0]);
    [TestMethod] public void Keyword_return()    => Assert.AreEqual(ScKind.KwReturn,    Kinds("return")[0]);
    [TestMethod] public void Keyword_freturn()   => Assert.AreEqual(ScKind.KwFreturn,   Kinds("freturn")[0]);
    [TestMethod] public void Keyword_nreturn()   => Assert.AreEqual(ScKind.KwNreturn,   Kinds("nreturn")[0]);
    [TestMethod] public void Keyword_go()        => Assert.AreEqual(ScKind.KwGo,        Kinds("go")[0]);
    [TestMethod] public void Keyword_to()        => Assert.AreEqual(ScKind.KwTo,        Kinds("to")[0]);
    [TestMethod] public void Keyword_procedure() => Assert.AreEqual(ScKind.KwProcedure, Kinds("procedure")[0]);
    [TestMethod] public void Keyword_struct()    => Assert.AreEqual(ScKind.KwStruct,    Kinds("struct")[0]);

    [TestMethod]
    public void Keyword_NotReserved_IfPartOfLongerIdent()
    {
        // "iffy" is an identifier, not the keyword "if" + "fy"
        Assert.AreEqual(ScKind.Identifier, Kinds("iffy")[0]);
    }

    [TestMethod]
    public void Keyword_CaseSensitive_UpperNotKeyword()
    {
        // Snocone is case-sensitive; "IF" is an identifier
        Assert.AreEqual(ScKind.Identifier, Kinds("IF")[0]);
    }

    // =========================================================================
    // 4. Operators — every bconv entry
    // =========================================================================

    [TestMethod] public void Op_Assign()    => Assert.AreEqual(ScKind.OpAssign,   Kinds("=")[0]);
    [TestMethod] public void Op_Question()  => Assert.AreEqual(ScKind.OpQuestion, Kinds("?")[0]);
    [TestMethod] public void Op_Pipe()      => Assert.AreEqual(ScKind.OpPipe,     Kinds("|")[0]);
    [TestMethod] public void Op_Or()        => Assert.AreEqual(ScKind.OpOr,       Kinds("||")[0]);
    [TestMethod] public void Op_Concat()    => Assert.AreEqual(ScKind.OpConcat,   Kinds("&&")[0]);
    [TestMethod] public void Op_EqNum()     => Assert.AreEqual(ScKind.OpEq,       Kinds("==")[0]);
    [TestMethod] public void Op_Ne()        => Assert.AreEqual(ScKind.OpNe,       Kinds("!=")[0]);
    [TestMethod] public void Op_Lt()        => Assert.AreEqual(ScKind.OpLt,       Kinds("<")[0]);
    [TestMethod] public void Op_Gt()        => Assert.AreEqual(ScKind.OpGt,       Kinds(">")[0]);
    [TestMethod] public void Op_Le()        => Assert.AreEqual(ScKind.OpLe,       Kinds("<=")[0]);
    [TestMethod] public void Op_Ge()        => Assert.AreEqual(ScKind.OpGe,       Kinds(">=")[0]);
    [TestMethod] public void Op_StrIdent()  => Assert.AreEqual(ScKind.OpStrIdent, Kinds("::")[0]);
    [TestMethod] public void Op_StrDiffer() => Assert.AreEqual(ScKind.OpStrDiffer,Kinds(":!:")[0]);
    [TestMethod] public void Op_StrLt()     => Assert.AreEqual(ScKind.OpStrLt,    Kinds(":<:")[0]);
    [TestMethod] public void Op_StrGt()     => Assert.AreEqual(ScKind.OpStrGt,    Kinds(":>:")[0]);
    [TestMethod] public void Op_StrLe()     => Assert.AreEqual(ScKind.OpStrLe,    Kinds(":<=:")[0]);
    [TestMethod] public void Op_StrGe()     => Assert.AreEqual(ScKind.OpStrGe,    Kinds(":>=:")[0]);
    [TestMethod] public void Op_StrEq()     => Assert.AreEqual(ScKind.OpStrEq,    Kinds(":==:")[0]);
    [TestMethod] public void Op_StrNe()     => Assert.AreEqual(ScKind.OpStrNe,    Kinds(":!=:")[0]);
    [TestMethod] public void Op_Plus()      => Assert.AreEqual(ScKind.OpPlus,     Kinds("+")[0]);
    [TestMethod] public void Op_Minus()     => Assert.AreEqual(ScKind.OpMinus,    Kinds("-")[0]);
    [TestMethod] public void Op_Slash()     => Assert.AreEqual(ScKind.OpSlash,    Kinds("/")[0]);
    [TestMethod] public void Op_Star()      => Assert.AreEqual(ScKind.OpStar,     Kinds("*")[0]);
    [TestMethod] public void Op_Percent()   => Assert.AreEqual(ScKind.OpPercent,  Kinds("%")[0]);
    [TestMethod] public void Op_Caret()     => Assert.AreEqual(ScKind.OpCaret,    Kinds("^")[0]);
    [TestMethod] public void Op_Period()    => Assert.AreEqual(ScKind.OpPeriod,   Kinds(".")[0]);
    [TestMethod] public void Op_Dollar()    => Assert.AreEqual(ScKind.OpDollar,   Kinds("$")[0]);
    [TestMethod] public void Op_Tilde()     => Assert.AreEqual(ScKind.OpTilde,    Kinds("~")[0]);
    [TestMethod] public void Op_At()        => Assert.AreEqual(ScKind.OpAt,       Kinds("@")[0]);
    [TestMethod] public void Op_Ampersand() => Assert.AreEqual(ScKind.OpAmpersand,Kinds("&")[0]);

    [TestMethod]
    public void Op_LongestMatch_StrNe_NotColonBangColon()
    {
        // :!=: must be matched as a single 4-char token, not : then != then :
        var toks = Tokens(":!=:");
        Assert.AreEqual(1, toks.Count(t => t.Kind != ScKind.Newline));
        Assert.AreEqual(ScKind.OpStrNe, toks[0].Kind);
    }

    [TestMethod]
    public void Op_LongestMatch_Or_NotTwoPipes()
    {
        // || must be one token
        var toks = Tokens("||");
        Assert.AreEqual(1, toks.Count(t => t.Kind != ScKind.Newline));
        Assert.AreEqual(ScKind.OpOr, toks[0].Kind);
    }

    [TestMethod]
    public void Op_LongestMatch_Concat_NotTwoAmps()
    {
        var toks = Tokens("&&");
        Assert.AreEqual(1, toks.Count(t => t.Kind != ScKind.Newline));
        Assert.AreEqual(ScKind.OpConcat, toks[0].Kind);
    }

    [TestMethod]
    public void Op_DoubleStar_IsCaret()
    {
        Assert.AreEqual(ScKind.OpCaret, Kinds("**")[0]);
    }

    // =========================================================================
    // 5. Punctuation
    // =========================================================================

    [TestMethod] public void Punct_LParen()   => Assert.AreEqual(ScKind.LParen,   Kinds("(")[0]);
    [TestMethod] public void Punct_RParen()   => Assert.AreEqual(ScKind.RParen,   Kinds(")")[0]);
    [TestMethod] public void Punct_LBrace()   => Assert.AreEqual(ScKind.LBrace,   Kinds("{")[0]);
    [TestMethod] public void Punct_RBrace()   => Assert.AreEqual(ScKind.RBrace,   Kinds("}")[0]);
    [TestMethod] public void Punct_LBracket() => Assert.AreEqual(ScKind.LBracket, Kinds("[")[0]);
    [TestMethod] public void Punct_RBracket() => Assert.AreEqual(ScKind.RBracket, Kinds("]")[0]);
    [TestMethod] public void Punct_Comma()    => Assert.AreEqual(ScKind.Comma,    Kinds(",")[0]);
    [TestMethod] public void Punct_Colon()    => Assert.AreEqual(ScKind.Colon,    Kinds(":")[0]);

    // =========================================================================
    // 6. Statement boundaries
    // =========================================================================

    [TestMethod]
    public void Boundary_NewlineAfterStatement()
    {
        var toks = Tokenize("x = 1");
        Assert.IsTrue(toks.Any(t => t.Kind == ScKind.Newline));
    }

    [TestMethod]
    public void Boundary_TwoStatements_TwoNewlines()
    {
        var toks = Tokenize("x = 1\ny = 2");
        Assert.AreEqual(2, toks.Count(t => t.Kind == ScKind.Newline));
    }

    [TestMethod]
    public void Boundary_ContinuationLine_JoinedIntoOneStatement()
    {
        // "x = 1 +" continues; "2" completes it → one Newline
        var toks = Tokenize("x = 1 +\n2");
        Assert.AreEqual(1, toks.Count(t => t.Kind == ScKind.Newline));
        // Token stream: x = 1 + 2 Newline EOF
        var kinds = toks.Where(t => t.Kind != ScKind.Eof && t.Kind != ScKind.Newline)
                        .Select(t => t.Kind).ToList();
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.OpAssign, ScKind.Integer, ScKind.OpPlus, ScKind.Integer },
            kinds);
    }

    [TestMethod]
    public void Boundary_ContinuationOnComma()
    {
        var toks = Tokenize("f(a,\nb)");
        Assert.AreEqual(1, toks.Count(t => t.Kind == ScKind.Newline));
    }

    [TestMethod]
    public void Boundary_CommentStripBeforeContinuationCheck()
    {
        // "x + # note" — after stripping comment → "x + ", last real char is '+'
        var toks = Tokenize("x +  # note\n2");
        Assert.AreEqual(1, toks.Count(t => t.Kind == ScKind.Newline));
    }

    [TestMethod]
    public void Boundary_BlankLineSkipped()
    {
        var toks = Tokenize("x = 1\n\ny = 2");
        Assert.AreEqual(2, toks.Count(t => t.Kind == ScKind.Newline));
    }

    [TestMethod]
    public void Boundary_CommentOnlyLineSkipped()
    {
        var toks = Tokenize("x = 1\n# comment\ny = 2");
        Assert.AreEqual(2, toks.Count(t => t.Kind == ScKind.Newline));
    }

    // =========================================================================
    // 7. Semicolons
    // =========================================================================

    [TestMethod]
    public void Semicolon_SplitsIntoTwoStatements()
    {
        var toks = Tokenize("x = 1; y = 2");
        Assert.AreEqual(2, toks.Count(t => t.Kind == ScKind.Newline));
    }

    [TestMethod]
    public void Semicolon_InsideString_NotSplit()
    {
        var toks = Tokenize("x = 'a;b'");
        Assert.AreEqual(1, toks.Count(t => t.Kind == ScKind.Newline));
    }

    // =========================================================================
    // 8. Line numbers
    // =========================================================================

    [TestMethod]
    public void LineNumbers_FirstTokenOnLine1()
    {
        var toks = Tokens("x = 1");
        Assert.AreEqual(1, toks[0].Line);
    }

    [TestMethod]
    public void LineNumbers_SecondStatementLine2()
    {
        var toks = Tokenize("x = 1\ny = 2")
            .Where(t => t.Kind != ScKind.Eof && t.Kind != ScKind.Newline)
            .ToList();
        var y = toks.First(t => t.Text == "y");
        Assert.AreEqual(2, y.Line);
    }

    [TestMethod]
    public void LineNumbers_ContinuedStatement_UsesFirstLine()
    {
        // Statement starts on line 1 (the +), tokens for "2" should use line 1
        var toks = Tokenize("x = 1 +\n2")
            .Where(t => t.Kind != ScKind.Eof && t.Kind != ScKind.Newline)
            .ToList();
        Assert.IsTrue(toks.All(t => t.Line == 1));
    }

    // =========================================================================
    // 9. End-to-end snocone.sc snippets
    // =========================================================================

    [TestMethod]
    public void E2E_IfStatement()
    {
        // if (x == 0) { y = 1 }
        var kinds = Kinds("if (x == 0) { y = 1 }");
        CollectionAssert.AreEqual(new[]
        {
            ScKind.KwIf,
            ScKind.LParen,
            ScKind.Identifier,
            ScKind.OpEq,
            ScKind.Integer,
            ScKind.RParen,
            ScKind.LBrace,
            ScKind.Identifier,
            ScKind.OpAssign,
            ScKind.Integer,
            ScKind.RBrace,
        }, kinds);
    }

    [TestMethod]
    public void E2E_ProcedureHeader()
    {
        // procedure foo(a, b) {
        var kinds = Kinds("procedure foo(a, b) {");
        Assert.AreEqual(ScKind.KwProcedure, kinds[0]);
        Assert.AreEqual(ScKind.Identifier,  kinds[1]);
        Assert.AreEqual(ScKind.LParen,      kinds[2]);
        Assert.AreEqual(ScKind.Identifier,  kinds[3]);
        Assert.AreEqual(ScKind.Comma,       kinds[4]);
        Assert.AreEqual(ScKind.Identifier,  kinds[5]);
        Assert.AreEqual(ScKind.RParen,      kinds[6]);
        Assert.AreEqual(ScKind.LBrace,      kinds[7]);
    }

    [TestMethod]
    public void E2E_WhileLoop()
    {
        // while (i > 0) {
        var kinds = Kinds("while (i > 0) {");
        Assert.AreEqual(ScKind.KwWhile, kinds[0]);
        Assert.AreEqual(ScKind.LParen,  kinds[1]);
        Assert.AreEqual(ScKind.Identifier, kinds[2]);
        Assert.AreEqual(ScKind.OpGt,    kinds[3]);
        Assert.AreEqual(ScKind.Integer, kinds[4]);
        Assert.AreEqual(ScKind.RParen,  kinds[5]);
        Assert.AreEqual(ScKind.LBrace,  kinds[6]);
    }

    [TestMethod]
    public void E2E_ForLoop()
    {
        // for (i = 1, i <= n, i = i + 1)
        var kinds = Kinds("for (i = 1, i <= n, i = i + 1)");
        Assert.AreEqual(ScKind.KwFor,   kinds[0]);
        Assert.AreEqual(ScKind.LParen,  kinds[1]);
        Assert.AreEqual(ScKind.OpAssign,kinds[3]);
        // second comma at some position
        var commas = kinds.Where(k => k == ScKind.Comma).Count();
        Assert.AreEqual(2, commas);
    }

    [TestMethod]
    public void E2E_StringComparisonOps()
    {
        // a :==: b, a :!=: b, a :<: b, a :>: b
        var kinds = Kinds("a :==: b");
        Assert.AreEqual(ScKind.OpStrEq, kinds[1]);

        kinds = Kinds("a :!=: b");
        Assert.AreEqual(ScKind.OpStrNe, kinds[1]);

        kinds = Kinds("a :<: b");
        Assert.AreEqual(ScKind.OpStrLt, kinds[1]);

        kinds = Kinds("a :>: b");
        Assert.AreEqual(ScKind.OpStrGt, kinds[1]);
    }

    [TestMethod]
    public void E2E_ReturnWithValue()
    {
        var kinds = Kinds("return x + 1");
        Assert.AreEqual(ScKind.KwReturn, kinds[0]);
        Assert.AreEqual(ScKind.Identifier, kinds[1]);
        Assert.AreEqual(ScKind.OpPlus, kinds[2]);
        Assert.AreEqual(ScKind.Integer, kinds[3]);
    }

    [TestMethod]
    public void E2E_GotoStatement()
    {
        var kinds = Kinds("go to END");
        Assert.AreEqual(ScKind.KwGo, kinds[0]);
        Assert.AreEqual(ScKind.KwTo, kinds[1]);
        Assert.AreEqual(ScKind.Identifier, kinds[2]);
    }

    [TestMethod]
    public void E2E_StructDecl()
    {
        var kinds = Kinds("struct point { x, y }");
        Assert.AreEqual(ScKind.KwStruct, kinds[0]);
        Assert.AreEqual(ScKind.Identifier, kinds[1]);
        Assert.AreEqual(ScKind.LBrace, kinds[2]);
        Assert.AreEqual(ScKind.Identifier, kinds[3]);
        Assert.AreEqual(ScKind.Comma, kinds[4]);
        Assert.AreEqual(ScKind.Identifier, kinds[5]);
        Assert.AreEqual(ScKind.RBrace, kinds[6]);
    }

    [TestMethod]
    public void E2E_ArraySubscript()
    {
        // arr[i]
        var kinds = Kinds("arr[i]");
        Assert.AreEqual(ScKind.Identifier, kinds[0]);
        Assert.AreEqual(ScKind.LBracket,   kinds[1]);
        Assert.AreEqual(ScKind.Identifier, kinds[2]);
        Assert.AreEqual(ScKind.RBracket,   kinds[3]);
    }

    [TestMethod]
    public void E2E_ConcatOperator()
    {
        // x && y  (Snocone explicit concat → SNOBOL4 blank concat)
        var kinds = Kinds("x && y");
        Assert.AreEqual(ScKind.Identifier, kinds[0]);
        Assert.AreEqual(ScKind.OpConcat,   kinds[1]);
        Assert.AreEqual(ScKind.Identifier, kinds[2]);
    }

    [TestMethod]
    public void E2E_MultilineWithContinuation()
    {
        const string src =
            "x = a +\n" +
            "b +\n" +
            "c\n";
        var toks = Tokenize(src);
        // One logical statement → one Newline
        Assert.AreEqual(1, toks.Count(t => t.Kind == ScKind.Newline));
        var texts = toks.Where(t => t.Kind != ScKind.Eof && t.Kind != ScKind.Newline)
                        .Select(t => t.Text).ToList();
        CollectionAssert.AreEqual(new[] { "x", "=", "a", "+", "b", "+", "c" }, texts);
    }

    [TestMethod]
    public void E2E_EofToken()
    {
        var toks = Tokenize("x = 1");
        Assert.AreEqual(ScKind.Eof, toks[^1].Kind);
    }

    [TestMethod]
    public void E2E_EmptySource_OnlyEof()
    {
        var toks = Tokenize("");
        Assert.AreEqual(1, toks.Count);
        Assert.AreEqual(ScKind.Eof, toks[0].Kind);
    }

    [TestMethod]
    public void E2E_CommentOnlySource_OnlyEof()
    {
        var toks = Tokenize("# nothing here\n# more nothing");
        Assert.AreEqual(1, toks.Count);
        Assert.AreEqual(ScKind.Eof, toks[0].Kind);
    }
}
