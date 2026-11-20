using Snobol4.Common;
using static Snobol4.Common.SnoconeLexer;

namespace Test.TestSnocone;

/// <summary>
/// Unit tests for SnoconeParser — Step 2: expression parser.
///
/// SnoconeParser.ParseExpression(tokens) takes the flat token list from
/// SnoconeLexer and returns a postfix (RPN) List of ScToken, exactly
/// as Parser.ShuntYardAlgorithm does for SNOBOL4 source.
///
/// Reduce condition from binop() in snocone.sc:
///   while existing_op.lp >= incoming_op.rp  → reduce
///
/// Precedence table (lp / rp from bconv in snocone.sc):
///   =    lp=1  rp=2   right-assoc
///   ?    lp=2  rp=2   left
///   |    lp=3  rp=3   left
///   ||   lp=4  rp=4   left
///   &&   lp=5  rp=5   left
///   comparisons lp=6 rp=6 left
///   + -  lp=7  rp=7   left
///   / * % lp=8 rp=8   left
///   ^    lp=9  rp=10  RIGHT-assoc
///   . $  lp=10 rp=10  left
/// </summary>
[TestClass]
public class TestSnoconeParser
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    private static List<ScToken> Parse(string src)
    {
        var tokens = Tokenize(src)
            .Where(t => t.Kind != ScKind.Newline && t.Kind != ScKind.Eof)
            .ToList();
        return SnoconeParser.ParseExpression(tokens);
    }

    private static List<ScKind> Kinds(string src) =>
        Parse(src).Select(t => t.Kind).ToList();

    // =========================================================================
    // 1. Single operands — pass through unchanged
    // =========================================================================

    [TestMethod]
    public void Single_Identifier() =>
        CollectionAssert.AreEqual(new[] { ScKind.Identifier }, Kinds("x"));

    [TestMethod]
    public void Single_Integer() =>
        CollectionAssert.AreEqual(new[] { ScKind.Integer }, Kinds("42"));

    [TestMethod]
    public void Single_String() =>
        CollectionAssert.AreEqual(new[] { ScKind.String }, Kinds("'hello'"));

    [TestMethod]
    public void Single_Real() =>
        CollectionAssert.AreEqual(new[] { ScKind.Real }, Kinds("3.14"));

    // =========================================================================
    // 2. Simple binary — postfix: a b op
    // =========================================================================

    [TestMethod]
    public void Binary_Add() =>
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpPlus },
            Kinds("a + b"));

    [TestMethod]
    public void Binary_Assign() =>
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpAssign },
            Kinds("x = y"));

    [TestMethod]
    public void Binary_Concat() =>
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpConcat },
            Kinds("a && b"));

    [TestMethod]
    public void Binary_Or() =>
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpOr },
            Kinds("a || b"));

    // =========================================================================
    // 3. Precedence
    // =========================================================================

    [TestMethod]
    public void Precedence_MulBeforeAdd() =>
        // a + b * c  →  a b c * +
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.Identifier,
                    ScKind.OpStar, ScKind.OpPlus },
            Kinds("a + b * c"));

    [TestMethod]
    public void Precedence_AddBeforeCompare() =>
        // a == b + c  →  a b c + ==
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.Identifier,
                    ScKind.OpPlus, ScKind.OpEq },
            Kinds("a == b + c"));

    [TestMethod]
    public void Precedence_ConcatBeforeOr() =>
        // a || b && c  →  a b c && ||
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.Identifier,
                    ScKind.OpConcat, ScKind.OpOr },
            Kinds("a || b && c"));

    [TestMethod]
    public void Precedence_DotHigherThanAdd() =>
        // a + b . c  →  a b c . +
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.Identifier,
                    ScKind.OpPeriod, ScKind.OpPlus },
            Kinds("a + b . c"));

    [TestMethod]
    public void Precedence_CaretHigherThanMul() =>
        // a * b ^ c  →  a b c ^ *
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.Identifier,
                    ScKind.OpCaret, ScKind.OpStar },
            Kinds("a * b ^ c"));

    [TestMethod]
    public void Precedence_CompareBeforeAssign() =>
        // x = a == b  →  x a b == =
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.Identifier,
                    ScKind.OpEq, ScKind.OpAssign },
            Kinds("x = a == b"));

    // =========================================================================
    // 4. Associativity
    //    Left  (lp==rp): a + b + c  →  a b + c +
    //    Right (lp < rp): a ^ b ^ c  →  a b c ^ ^
    // =========================================================================

    [TestMethod]
    public void Associativity_Add_Left() =>
        // a + b + c  →  a b + c +
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpPlus,
                    ScKind.Identifier, ScKind.OpPlus },
            Kinds("a + b + c"));

    [TestMethod]
    public void Associativity_Mul_Left() =>
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpStar,
                    ScKind.Identifier, ScKind.OpStar },
            Kinds("a * b * c"));

    [TestMethod]
    public void Associativity_Concat_Left() =>
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpConcat,
                    ScKind.Identifier, ScKind.OpConcat },
            Kinds("a && b && c"));

    [TestMethod]
    public void Associativity_Caret_Right() =>
        // a ^ b ^ c  →  a b c ^ ^   (lp=9 < rp=10, so second ^ does NOT reduce first)
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.Identifier,
                    ScKind.OpCaret, ScKind.OpCaret },
            Kinds("a ^ b ^ c"));

    [TestMethod]
    public void Associativity_Assign_Right() =>
        // a = b = c  →  a b c = =   (lp=1 < rp=2)
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.Identifier,
                    ScKind.OpAssign, ScKind.OpAssign },
            Kinds("a = b = c"));

    // =========================================================================
    // 5. Unary operators — postfix with IsUnary=true flag
    // =========================================================================

    [TestMethod]
    public void Unary_Minus()
    {
        var r = Parse("-x");
        Assert.AreEqual(2, r.Count);
        Assert.AreEqual(ScKind.Identifier, r[0].Kind);
        Assert.AreEqual(ScKind.OpMinus,    r[1].Kind);
        Assert.IsTrue(r[1].IsUnary);
    }

    [TestMethod]
    public void Unary_Tilde()
    {
        var r = Parse("~x");
        Assert.AreEqual(2, r.Count);
        Assert.AreEqual(ScKind.OpTilde, r[1].Kind);
        Assert.IsTrue(r[1].IsUnary);
    }

    [TestMethod]
    public void Unary_Star_UnevaluatedPattern()
    {
        var r = Parse("*p");
        Assert.AreEqual(2, r.Count);
        Assert.AreEqual(ScKind.OpStar, r[1].Kind);
        Assert.IsTrue(r[1].IsUnary);
    }

    [TestMethod]
    public void Unary_BindsTighterThanBinary()
    {
        // a + -b  →  a b unary- +
        var r = Parse("a + -b");
        Assert.AreEqual(4, r.Count);
        Assert.AreEqual(ScKind.Identifier, r[0].Kind);
        Assert.AreEqual(ScKind.Identifier, r[1].Kind);
        Assert.IsTrue(r[2].IsUnary);
        Assert.AreEqual(ScKind.OpMinus,    r[2].Kind);
        Assert.AreEqual(ScKind.OpPlus,     r[3].Kind);
        Assert.IsFalse(r[3].IsUnary);
    }

    // =========================================================================
    // 6. Parentheses — override precedence
    // =========================================================================

    [TestMethod]
    public void Parens_OverridePrecedence() =>
        // (a + b) * c  →  a b + c *
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpPlus,
                    ScKind.Identifier, ScKind.OpStar },
            Kinds("(a + b) * c"));

    [TestMethod]
    public void Parens_Nested() =>
        // (a + (b * c))  →  a b c * +
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.Identifier,
                    ScKind.OpStar, ScKind.OpPlus },
            Kinds("(a + (b * c))"));

    // =========================================================================
    // 7. Function calls and array refs
    // =========================================================================

    [TestMethod]
    public void FunctionCall_NoArgs()
    {
        var r = Parse("f()");
        Assert.AreEqual(ScKind.Identifier, r[0].Kind);
        Assert.AreEqual("f",               r[0].Text);
        Assert.AreEqual(ScKind.ScCall,     r[1].Kind);
        Assert.AreEqual(0,                 r[1].ArgCount);
    }

    [TestMethod]
    public void FunctionCall_OneArg()
    {
        var r = Parse("f(x)");
        Assert.AreEqual(ScKind.Identifier, r[0].Kind);
        Assert.AreEqual(ScKind.Identifier, r[1].Kind);
        Assert.AreEqual(ScKind.ScCall,     r[2].Kind);
        Assert.AreEqual(1,                 r[2].ArgCount);
    }

    [TestMethod]
    public void FunctionCall_TwoArgs()
    {
        var r = Parse("f(x, y)");
        Assert.AreEqual(ScKind.Identifier, r[0].Kind);
        Assert.AreEqual(ScKind.Identifier, r[1].Kind);
        Assert.AreEqual(ScKind.Identifier, r[2].Kind);
        Assert.AreEqual(ScKind.ScCall,     r[3].Kind);
        Assert.AreEqual(2,                 r[3].ArgCount);
    }

    [TestMethod]
    public void ArrayRef_OneIndex()
    {
        var r = Parse("arr[i]");
        Assert.AreEqual(ScKind.Identifier, r[0].Kind);
        Assert.AreEqual(ScKind.Identifier, r[1].Kind);
        Assert.AreEqual(ScKind.ScArrayRef, r[2].Kind);
        Assert.AreEqual(1,                 r[2].ArgCount);
    }

    [TestMethod]
    public void FunctionCall_ArgIsExpression()
    {
        // f(a + b)  →  f a b + CALL(1)
        var r = Parse("f(a + b)");
        Assert.AreEqual(ScKind.Identifier, r[0].Kind);
        Assert.AreEqual(ScKind.Identifier, r[1].Kind);
        Assert.AreEqual(ScKind.Identifier, r[2].Kind);
        Assert.AreEqual(ScKind.OpPlus,     r[3].Kind);
        Assert.AreEqual(ScKind.ScCall,     r[4].Kind);
        Assert.AreEqual(1,                 r[4].ArgCount);
    }

    // =========================================================================
    // 8. String comparison operators (all lp=6 rp=6)
    // =========================================================================

    [TestMethod]
    public void StringOp_StrEq() =>
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpStrEq },
            Kinds("a :==: b"));

    [TestMethod]
    public void StringOp_StrNe() =>
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpStrNe },
            Kinds("a :!=: b"));

    [TestMethod]
    public void StringOp_SamePrecAsNumericCompare() =>
        // a == b :==: c  →  a b == c :==:  (left-assoc, lp=rp=6)
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpEq,
                    ScKind.Identifier, ScKind.OpStrEq },
            Kinds("a == b :==: c"));

    // =========================================================================
    // 9. dotck — leading-dot float gets "0." prepended
    // =========================================================================

    [TestMethod]
    public void Dotck_LeadingDot_Rewritten()
    {
        var r = Parse(".5");
        Assert.AreEqual(1,            r.Count);
        Assert.AreEqual(ScKind.Real,  r[0].Kind);
        Assert.AreEqual("0.5",        r[0].Text);
    }

    [TestMethod]
    public void Dotck_NormalFloat_Unchanged()
    {
        var r = Parse("3.14");
        Assert.AreEqual("3.14", r[0].Text);
    }
}
