namespace Test.TestLexer;

[TestClass]
public partial class TestLexer
{
    [TestMethod]
    public void TEST_214_001()
    {
        var s = $"£   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_002()
    {
        var s = $"£{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_003()
    {
        var s = $":   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_004()
    {
        var s = $")   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_005()
    {
        var s = $"[   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_006()
    {
        var s = $")]   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);

    }

    [TestMethod]
    public void TEST_214_007()
    {
        var s = $"<   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_008()
    {
        var s = $">   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_009()
    {
        var s = $"(   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_010()
    {
        var s = $"/   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_011()
    {
        var s = $"\"   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_012()
    {
        var s = $",   'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_013()
    {
        var s = $"\"test\"{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_214_014()
    {
        var s = $"'test'{Environment.NewLine}end{Environment.NewLine}";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(214, build.ErrorCodeHistory[0]);
        Assert.AreEqual(0, build.ColumnHistory[0]);
    }
}