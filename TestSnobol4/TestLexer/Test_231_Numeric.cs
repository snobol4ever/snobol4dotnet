namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_231_001()
    {
        var s = " 123D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_002()
    {
        var s = " 123.123D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_003()
    {
        var s = " 123e45D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_004()
    {
        var s = " 123e+45D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_005()
    {
        var s = " 123e-45D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_006()
    {
        var s = " 123.e45D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_007()
    {
        var s = " 123.e+45D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_008()
    {
        var s = " 123.e-45D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_009()
    {
        var s = " 123.456e78D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_010()
    {
        var s = " 123.456e+78D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_011()
    {
        var s = " 123.456e-78D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_012()
    {
        var s = " 123e+D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_013()
    {
        var s = " 123e-D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_014()
    {
        var s = " 123.eD;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_015()
    {
        var s = " 123.e+D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_016()
    {
        var s = " 123.e-D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_017()
    {
        var s = " 123.456eD;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_018()
    {
        var s = " 123.456e+D;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    

    [TestMethod]
    public void TEST_231_101()
    {
        var s = " 123';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_102()
    {
        var s = " 123.123';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_103()
    {
        var s = " 123e45';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_104()
    {
        var s = " 123e+45';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_105()
    {
        var s = " 123e-45';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_106()
    {
        var s = " 123.e45';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_107()
    {
        var s = " 123.e+45';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_108()
    {
        var s = " 123.e-45';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_109()
    {
        var s = " 123.456e78';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_110()
    {
        var s = " 123.456e+78';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_111()
    {
        var s = " 123.456e-78';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_112()
    {
        var s = " 123e+';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_113()
    {
        var s = " 123e-';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_114()
    {
        var s = " 123.e';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_115()
    {
        var s = " 123.e+';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_116()
    {
        var s = " 123.e-';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_117()
    {
        var s = " 123.456e';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_118()
    {
        var s = " 123.456e+';end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    
    [TestMethod]
    public void TEST_231_201()
    {
        var s = " 123\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_202()
    {
        var s = " 123.123\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_203()
    {
        var s = " 123e45\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(7, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_204()
    {
        var s = " 123e+45\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_205()
    {
        var s = " 123e-45\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_206()
    {
        var s = " 123.e45\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_207()
    {
        var s = " 123.e+45\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_208()
    {
        var s = " 123.e-45\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(9, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_209()
    {
        var s = " 123.456e78\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_210()
    {
        var s = " 123.456e+78\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_211()
    {
        var s = " 123.456e-78\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_212()
    {
        var s = " 123e+\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_213()
    {
        var s = " 123e-\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_214()
    {
        var s = " 123.e\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_215()
    {
        var s = " 123.e+\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_216()
    {
        var s = " 123.e-\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_217()
    {
        var s = " 123.456e\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_218()
    {
        var s = " 123.456e+\";end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    
    [TestMethod]
    public void TEST_231_312()
    {
        var s = " 123e++;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_313()
    {
        var s = " 123e-+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(4, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_314()
    {
        var s = " 123.e+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_315()
    {
        var s = " 123.e++;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_316()
    {
        var s = " 123.e-+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(5, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_317()
    {
        var s = " 123.456e+;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_318()
    {
        var s = " 123.456e++;end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(8, build.ColumnHistory[0]);
    }
}