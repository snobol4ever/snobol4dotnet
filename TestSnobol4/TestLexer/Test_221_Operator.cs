namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_221_001()
    {
        var s = $" A ** :(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_221_002()
    {
        var s = $" A **:(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_221_003()
    {
        var s = $" A /{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_221_004()
    {
        var s = $" A % ${Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_221_005()
    {
        var s = $" B<A + >{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_221_006()
    {
        var s = $" B<A + >{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_221_007()
    {
        var s = $" B(A ~ ){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_221_008()
    {
        var s = $" B[A ? ]{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_221_009()
    {
        var s = $" (A ! ){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }
    [TestMethod]

    public void TEST_221_010()
    {
        var s = $"a = test{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(221, build.ErrorCodeHistory[0]);
        Assert.AreEqual(2, build.ColumnHistory[0]);
    }
}