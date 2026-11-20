namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_218_001()
    {
        var s = $"    :S(end)S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_002()
    {
        var s = $"    :S(end)F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_003()
    {
        var s = $"    :S(end)(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_004()
    {
        var s = $"    :F(end)S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_005()
    {
        var s = $"    :F(end)F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_006()
    {
        var s = $"    :F(end)(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_007()
    {
        var s = $"    :(end)S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_008()
    {
        var s = $"    :(end)F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_009()
    {
        var s = $"    :(end)(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_011()
    {
        var s = $"    :S<end>S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_012()
    {
        var s = @"
     e = code(' :(end)')
    :S<e>F(end)
end"; var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_013()
    {
        var s = $"    :S<end>(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_014()
    {
        var s = $"    :F<end>S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_015()
    {
        var s = $"    :F<end>F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_016()
    {
        var s = $"    :F<end>(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_017()
    {
        var s = $"    :<end>S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_018()
    {
        var s = $"    :<end>F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_019()
    {
        var s = $"    :<end>(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_021()
    {
        var s = $"    :S(end)S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_022()
    {
        var s = @"
     e = code(' :(end)')
    :S(end)F<e>
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_023()
    {
        var s = $"    :S(end)<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_024()
    {
        var s = @"
     e = code(' :(end)')
    :F(end)S<e>
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_025()
    {
        var s = $"    :F(end)F<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_026()
    {
        var s = $"    :F(end)<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_027()
    {
        var s = $"    :(end)S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_028()
    {
        var s = $"    :(end)F<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_029()
    {
        var s = $"    :(end)<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_031()
    {
        var s = $"    :S<end>S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_032()
    {
        var s = @"
     e = code(' :(end)')
    :S(end)F<e>
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_033()
    {
        var s = $"    :S<end><end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_034()
    {
        var s = @"
     e = code(' :(end)')
    :F(s)S<e>
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_035()
    {
        var s = $"    :F<end>F<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_036()
    {
        var s = $"    :F<end><end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_037()
    {
        var s = $"    :<end>S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_038()
    {
        var s = $"    :<end>F<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_039()
    {
        var s = $"    :<end><end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }


    [TestMethod]
    public void TEST_218_101()
    {
        var s = $"    :s(end)s(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_102()
    {
        var s = $"    :s(end)F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_103()
    {
        var s = $"    :s(end)(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_104()
    {
        var s = $"    :F(end)s(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_105()
    {
        var s = $"    :F(end)F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_106()
    {
        var s = $"    :F(end)(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_107()
    {
        var s = $"    :(end)s(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_108()
    {
        var s = $"    :(end)F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_109()
    {
        var s = $"    :(end)(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_111()
    {
        var s = $"    :s<end>S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_112()
    {
        var s = @"
     e = code(' :(end)')
    :s<e>F(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_113()
    {
        var s = $"    :s<end>(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_114()
    {
        var s = $"    :F<end>S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_115()
    {
        var s = $"    :F<end>F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_116()
    {
        var s = $"    :F<end>(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_117()
    {
        var s = $"    :<end>S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_118()
    {
        var s = $"    :<end>F(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_119()
    {
        var s = $"    :<end>(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_121()
    {
        var s = $"    :s(end)s<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_122()
    {
        var s = $"    :s(end)F<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_123()
    {
        var s = $"    :s(end)<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_124()
    {
        var s = @"
     e = code(' :(end)')
    :F(end)s<e>
end"; var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_125()
    {
        var s = $"    :F(end)F<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_126()
    {
        var s = $"    :F(end)<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_127()
    {
        var s = $"    :(end)s<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_128()
    {
        var s = $"    :(end)F<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_129()
    {
        var s = $"    :(end)<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_131()
    {
        var s = $"    :s<end>S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_132()
    {
        var s = @"
     e = code(' :(end)')
    :s<e>F<e>
end"; var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_133()
    {
        var s = $"    :s<end><end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_134()
    {
        var s = @"
     e = code(' :(end)')
    :F<e>S<e>
end"; var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_135()
    {
        var s = $"    :F<end>F<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_136()
    {
        var s = $"    :F<end><end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_137()
    {
        var s = $"    :<end>S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_138()
    {
        var s = $"    :<end>F<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_139()
    {
        var s = $"    :<end><end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_201()
    {
        var s = $"    :s(end)S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_202()
    {
        var s = $"    :s(end)f(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_203()
    {
        var s = $"    :s(end)(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_204()
    {
        var s = $"    :f(end)S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_205()
    {
        var s = $"    :f(end)f(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_206()
    {
        var s = $"    :f(end)(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_207()
    {
        var s = $"    :(end)S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_208()
    {
        var s = $"    :(end)f(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_209()
    {
        var s = $"    :(end)(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_211()
    {
        var s = $"    :s<end>S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_212()
    {
        var s = @"
     e = code(' :(end)')
    :s<e>f(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_213()
    {
        var s = $"    :s<end>(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_214()
    {
        var s = $"    :f<end>S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_215()
    {
        var s = $"    :f<end>f(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_216()
    {
        var s = $"    :f<end>(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_217()
    {
        var s = $"    :<end>S(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_218()
    {
        var s = $"    :<end>f(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_219()
    {
        var s = $"    :<end>(end){Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_221()
    {
        var s = $"    :s(end)S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_222()
    {
        var s = $"    :s(end)f<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_223()
    {
        var s = $"    :s(end)<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_224()
    {
        var s = @"
     e = code(' :(end)')
    :f(end)S<e>
end"; var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_225()
    {
        var s = $"    :f(end)f<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_226()
    {
        var s = $"    :f(end)<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_227()
    {
        var s = $"    :(end)S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_228()
    {
        var s = $"    :(end)f<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_229()
    {
        var s = $"    :(end)<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_231()
    {
        var s = $"    :s<end>S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_232()
    {
        var s = @"
     e = code(' :(end)')
    :s<e>f<e>
end"; var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_233()
    {
        var s = $"    :s<end><end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_234()
    {
        var s = @"
     e = code(' :(end)')
    :f<e>S<e>
end"; var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0, build.ColumnHistory.Count);
    }

    [TestMethod]
    public void TEST_218_235()
    {
        var s = $"    :f<end>f<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_236()
    {
        var s = $"    :f<end><end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(11, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_237()
    {
        var s = $"    :<end>S<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_238()
    {
        var s = $"    :<end>f<end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_218_239()
    {
        var s = $"    :<end><end>{Environment.NewLine}end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(218, build.ErrorCodeHistory[0]);
        Assert.AreEqual(10, build.ColumnHistory[0]);
    }
}
