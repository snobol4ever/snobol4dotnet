using Test.TestLexer;

namespace Test.InputOutput;

[TestClass]
public class Eject
{
    [TestMethod]
    public void TEST_Eject_001f()
    {
        var s = @"

        EJECT(1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        // a channel with nothing open on it is error 93 ("eject file does not exist"), as sbl says
        Assert.AreEqual(93, build.ErrorCodeHistory[0]);
    }
    [TestMethod]
    public void TEST_Eject_002()
    {
        // EJECT(0), a channel with nothing open on it, is error 93 as well
        var s = @"
        EJECT(0)
        result = 'ok'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(93, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Eject_003()
    {
        // EJECT with no arg
        var s = @"
        EJECT()
        result = 'ok'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", build.Execute!.IdentifierTable[build.FoldCase("result")].ToString());
    }

    [TestMethod]
    public void TEST_Eject_004()
    {
        // Multiple EJECT calls in sequence on a channel that is open are fine, and a form feed and a line end are written each time
        var s = @"
        OUTPUT(.F, 1, 'eject004.out')
        EJECT(1)
        EJECT(1)
        EJECT(1)
        ENDFILE(1)
        result = 'ok'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", build.Execute!.IdentifierTable[build.FoldCase("result")].ToString());
        Assert.AreEqual("\f\n\f\n\f\n", File.ReadAllText("eject004.out").Replace("\r\n", "\n"));
    }
}
