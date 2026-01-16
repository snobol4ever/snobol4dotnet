using Test.TestLexer;

namespace Test.Miscellaneous;

//"size argument is not a string" /* 189 */,



[TestClass]
public class Size
{
    [TestMethod]
    public void TEST_Size_001()
    {
        var s = @"
        R = SIZE('123456')
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("6", build.Execute!.IdentifierTable["R"].ToString());
    }

    [TestMethod]
    public void TEST_Size_002()
    {
        var s = @"
        R = SIZE(123456)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("6", build.Execute!.IdentifierTable["R"].ToString());
    }

    [TestMethod]
    public void TEST_Size_003()
    {
        var s = @"

        T = TABLE(3)
        R = SIZE(T)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(1, build.ErrorCodeHistory.Count);
        Assert.AreEqual(189, build.ErrorCodeHistory[0]);
    }

}