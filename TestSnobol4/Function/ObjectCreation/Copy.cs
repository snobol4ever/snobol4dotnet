using Test.TestLexer;

namespace Test.ObjectCreation;

[TestClass]
public class Copy
{
    [TestMethod]
    public void TEST_Copy_001()
    {
        var s = @"
        A = '123456'
        B = COPY(A)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreNotEqual(build.Execute!.IdentifierTable["A"].Uid, build.Execute!.IdentifierTable["B"].Uid);
    }

    [TestMethod]
    public void TEST_Copy_002()
    {
        var s = @"
        A = 123456
        B = COPY(A)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreNotEqual(build.Execute!.IdentifierTable["A"].Uid, build.Execute!.IdentifierTable["B"].Uid);
    }

    [TestMethod]
    public void TEST_Copy_003()
    {
        var s = @"
        A = 'a' | 'b'
        B = COPY(A)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreNotEqual(build.Execute!.IdentifierTable["A"].Uid, build.Execute!.IdentifierTable["B"].Uid);
    }

}