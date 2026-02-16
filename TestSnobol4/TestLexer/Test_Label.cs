namespace Test.TestLexer;


public partial class TestLexer
{
    [TestMethod]
    public void TEST_LABEL_001()
    {
        var s = "ABC1223   'test';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(1, build.Code.SourceLines[0].LexBody.Count);
        Assert.AreEqual(s[..7], build.Code.SourceLines[0].Label);
    }

    [TestMethod]
    public void TEST_LABEL_001f()
    {
        var s = "ABC1223   'test';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(1, build.Code.SourceLines[0].LexBody.Count);
        Assert.AreEqual(s[..7], build.Code.SourceLines[0].Label);
    }

    [TestMethod]
    public void TEST_LABEL_002()
    {
        var s = "123abc   'test';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(1, build.Code.SourceLines[0].LexBody.Count);
        Assert.AreEqual(s[..6].ToUpper(), build.Code.SourceLines[0].Label);
    }

    [TestMethod]
    public void TEST_LABEL_002f()
    {
        var s = "123abc   'test';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(1, build.Code.SourceLines[0].LexBody.Count);
        Assert.AreEqual("123ABC", build.Code.SourceLines[0].Label);
    }

    [TestMethod]
    public void TEST_LABEL_003()
    {
        var s = "123??abc   'test';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(1, build.Code.SourceLines[0].LexBody.Count);
        Assert.AreEqual("123??ABC", build.Code.SourceLines[0].Label);
    }

    [TestMethod]
    public void TEST_LABEL_003f()
    {
        var s = "123??abc   'test';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(1, build.Code.SourceLines[0].LexBody.Count);
        Assert.AreEqual("123??ABC", build.Code.SourceLines[0].Label);
    }

    [TestMethod]
    public void TEST_LABEL_004()
    {
        var s = @"
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        //Assert.AreEqual(231, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_LABEL_005()
    {
        var s = $"test c = 1{Environment.NewLine}end test";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("test", build.Code.EntryLabel);
    }

    [TestMethod]
    public void TEST_LABEL_006()
    {
        var s = "end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", build.Code.EntryLabel);
    }

    [TestMethod]
    public void TEST_LABEL_007()
    {
        var s = "end   ";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

}

