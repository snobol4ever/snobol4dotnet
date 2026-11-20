namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_INVALID_CHARACTER_001()
    {
        var s = "        `;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_002()
    {
        var s = " 12`;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_003()
    {
        var s = "    12.34`;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_004()
    {
        var s = "    12.34e-7`;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_005()
    {
        var s = "    ID`;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(6, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_006()
    {
        var s = "   F(`);end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_007()
    {
        var s = "   F(0)`;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_008()
    {
        var s = "   F2(0,`);end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_009()
    {
        var s = "    \"str\"`;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_010()
    {
        var s = "    A<`>;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(6, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_011()
    {
        var s = "    T[`];end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(6, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_012()
    {
        var s = "    A<0>`;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_INVALID_CHARACTER_013()
    {
        var s = "     T[0]`;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);

        Assert.AreEqual(230, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }
}