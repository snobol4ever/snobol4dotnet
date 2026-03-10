using Snobol4.Common;
using static Snobol4.Common.SnoconeLexer;

namespace Test.TestSnocone;

/// <summary>
/// Unit tests for <see cref="SnoconeParser"/> — Step 2: expression parser.
///
/// SnoconeParser.ParseExpression(tokens) takes the flat token list from
/// SnoconeLexer and returns a postfix (RPN) List&lt;ScToken&gt;, exactly
/// as Parser.ShuntYardAlgorithm does for SNOBOL4 source.
///
/// Precedence table (from bconv in snocone.sc, lp/rp fields):
///   =          lp=1  rp=2   right-assoc
///   ?          lp=2  rp=2   left
///   |          lp=3  rp=3   left
///   ||         lp=4  rp=4   left
///   &&         lp=5  rp=5   left
///   comparisons lp=6 rp=6   left
///   + -        lp=7  rp=7   left
///   / * %      lp=8  rp=8   left
///   ^          lp=9  rp=10  RIGHT-assoc
///   . $        lp=10 rp=10  left
///
/// Shunting-yard invariant (from binop() in snocone.sc):
///   reduce when  existing_op.lp >= incoming_op.rp
/// </summary>
[TestClass]
public class TestSnoconeParser
{
    // -------------------------------------------------------------------------
    // Helper: tokenize then parse one expression; strip Newline/Eof from result
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

    private static List<string> Texts(string src) =>
        Parse(src).Select(t => t.Text).ToList();

    // =========================================================================
    // 1. Single operands — pass through unchanged
    // =========================================================================

    [TestMethod]
    public void Single_Identifier_PassThrough()
    {
        var r = Kinds("x");
        Assert.AreEqual(1, r.Count);
        Assert.AreEqual(ScKind.Identifier, r[0]);
    }

    [TestMethod]
    public void Single_Integer_PassThrough()
    {
        var r = Kinds("42");
        Assert.AreEqual(1, r.Count);
        Assert.AreEqual(ScKind.Integer, r[0]);
    }

    [TestMethod]
    public void Single_String_PassThrough()
    {
        var r = Kinds("'hello'");
        Assert.AreEqual(1, r.Count);
        Assert.AreEqual(ScKind.String, r[0]);
    }

    [TestMethod]
    public void Single_Real_PassThrough()
    {
        var r = Kinds("3.14");
        Assert.AreEqual(1, r.Count);
        Assert.AreEqual(ScKind.Real, r[0]);
    }

    // =========================================================================
    // 2. Simple binary — two operands, one operator → postfix: a b op
    // =========================================================================

    [TestMethod]
    public void Binary_Addition_Postfix()
    {
        // a + b  →  a b +
        var r = Kinds("a + b");
        CollectionAssert.AreEqual(
            new[] { ScKind.Identifier, ScKind.Identifier, ScKind.OpPlus },
            r);
    }

    [TestMethod]
    public void Binary_Assign_Postfix()
    {