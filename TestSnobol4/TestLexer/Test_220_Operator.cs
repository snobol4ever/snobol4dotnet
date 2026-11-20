namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_220_001()
    {
        var s = $" F(0)1.23E4{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_002()
    {
        var s = $" \"str\"1.23E4 {Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(6, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_003()
    {
        var s = $" \"str\"1.23E4{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(6, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_004()
    {
        var s = $" A<0>1.23E4{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_005()
    {
        var s = $" T[0]1.23E4415{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }


    [TestMethod]
    public void TEST_220_006()
    {
        var s = $" A<0>'test'{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_007a()
    {
        var s = $" T[0]'test'{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_008()
    {
        var s = $" A<0>\"test\"{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_009a()
    {
        var s = $" T[0]\"test\"{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_010()
    {
        var s = $" F(0)ID{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_011()
    {
        var s = $" \"str\"ID{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(6, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_012()
    {
        var s = $" A<0>ID{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_013()
    {
        var s = $" T[0]ID{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_014()
    {
        var s = $"    :S(end()F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_015()
    {
        var s = $"    \"asd\"\"asd\"{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_016()
    {
        var s = $"    \'asd\'\"asd\"{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_017()
    {
        var s = $"    \"asd\"\'asd\'{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_220_018()
    {
        var s = $"    \'asd\'\'asd\'{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(220, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

}
