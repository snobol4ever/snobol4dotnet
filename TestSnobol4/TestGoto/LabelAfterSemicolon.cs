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
}
