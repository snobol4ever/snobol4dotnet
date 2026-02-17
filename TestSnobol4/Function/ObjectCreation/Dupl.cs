using Test.TestLexer;

namespace Test.ObjectCreation;

[TestClass]
public class Dupl
{
    [TestMethod]
    public void TEST_Dupl_001()
    {
        var s = @"
        R = DUPL('123', 3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("123123123", build.Execute!.IdentifierTable[build.FoldCase("R")].ToString());
    }

    [TestMethod]
    public void TEST_Dupl_002()
    {
        var s = @"
        P = 'A' | '1'
        R = DUPL(P, 5)
        '11111' R . R1
        '1A1A1' R . R2
        'AAAAA' R . R3
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("11111", build.Execute!.IdentifierTable[build.FoldCase("R1")].ToString());
        Assert.AreEqual("1A1A1", build.Execute!.IdentifierTable[build.FoldCase("R2")].ToString());
        Assert.AreEqual("AAAAA", build.Execute!.IdentifierTable[build.FoldCase("R3")].ToString());
    }

}