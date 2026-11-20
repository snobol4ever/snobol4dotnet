namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_233_001()
    {
        var s = " ID*;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_004()
    {
        var s = " 'test'*;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_005()
    {

        var s = " \"test\"*;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_006()
    {

        var s = " A(0)*;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_007()
    {
        var s = " A<0>*;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_008()
    {
        var s = " A[0]*;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_009()
    {
        var s = " ID* ID;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(3, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_012()
    {
        var s = " 'test'* ID;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_013()
    {
        var s = " \"test\"* ID;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_014()
    {
        var s = " A(0)* ID;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_015()
    {
        var s = " A<0>* ID;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_016()
    {
        var s = " A[0]* ID;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_301()
    {
        var s = " 123+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_302()
    {
        var s = " 123.123+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_303()
    {
        var s = " 123e45+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_304()
    {
        var s = " 123e+45+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_305()
    {
        var s = " 123e-45+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_306()
    {
        var s = " 123.e45+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_307()
    {
        var s = " 123.e+45+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_308()
    {
        var s = " 123.e-45+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_309()
    {
        var s = " 123.456e78+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_310()
    {
        var s = " 123.456e+78+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_233_311()
    {
        var s = " 123.456e-78+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(233, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }
}