using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestGoto;

/// <summary>
/// A semicolon ends a statement and the next one begins as if in column 1, so a name
/// written right after the semicolon is that statement's label; a blank there means none.
/// Expected results are SPITBOL's (sbl -bf).
/// </summary>
[TestClass]
public class LabelAfterSemicolon
{
    [TestMethod]
    public void TEST_Label_right_after_a_semicolon_is_a_label()
    {
        var s = @"
        x = 'A'  :(en); x = 'skip';en y = 'B'
end";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("A", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data);
        Assert.AreEqual("B", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
    }

    [TestMethod]
    public void TEST_Blank_after_a_semicolon_is_no_label()
    {
        var s = @"
        a = 'aa' ; b = 'bb' ; c = 'cc'
end";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("aa", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("bb", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        Assert.AreEqual("cc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
    }

    // a label runs to the first blank, and a quote in it opens no string: in L'A X = 'one' ; Y = "two'" the
    // semicolon ends the first statement, as in SPITBOL, where reading 'A X = ' as a string hid it
    [TestMethod]
    public void TEST_A_quote_in_a_label_opens_no_string()
    {
        var s = "l'a x = 'one' ; y = \"two'\"" + Environment.NewLine
              + " w = 'a' ;m'b y2 = 'one' ; z = \"two'\"" + Environment.NewLine
              + "end";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("one", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data);
        Assert.AreEqual("two'", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
        Assert.AreEqual("one", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y2")]).Data);
        Assert.AreEqual("two'", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("z")]).Data);
    }

    // HERE="$(cd "$(dirname "${B}")"; MSG="x": the label is HERE="$(cd, so the semicolon stands outside every string
    // and the statement after it is in error too (233 twice, sbl -bf)
    [TestMethod]
    public void TEST_The_statement_after_a_label_with_a_quote_is_compiled()
    {
        var s = "HERE=\"$(cd \"$(dirname \"${B}\")\"; MSG=\"x\"" + Environment.NewLine + "end";
        var build = SetupTests.SetupScript("-bf", s);
        CollectionAssert.AreEqual(new[] { 233, 233 }, build.ErrorCodeHistory.ToArray());
    }

    // a bad label holds strings: in [X"a ; B = ( the string runs to the end of the line, so B = ( is no statement (214 alone, sbl -bf)
    [TestMethod]
    public void TEST_A_quote_after_a_bad_label_opens_a_string()
    {
        var build = SetupTests.SetupScript("-bf", "[X\"a ; B = (" + Environment.NewLine + "END");
        CollectionAssert.AreEqual(new[] { 214 }, build.ErrorCodeHistory.ToArray());
    }
}
