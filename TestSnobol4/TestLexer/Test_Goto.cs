namespace Test.TestLexer;

public partial class TestLexer
{
    [TestMethod]
    public void TEST_GOTO_001()
    {
        var s = @"
        s = 'test1  a = 42   :(end)'
        t = code(s) :S(test1)F(test)
test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_002()
    {
        var s = @"
        S = 'TEST1   A = 42   :(end)'
        T = CODE(S)
        :F(TEST)S(TEST1)
TEST
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("A")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_003()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :S<test>F(test);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_004()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :F<test>S(test1);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_005()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :S(test1)F<test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_006()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :F(test)S<test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_007()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :S<test>F<test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_008()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :F<test>S<test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_009()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :S(test1);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }


    [TestMethod]
    public void TEST_GOTO_010()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :S<test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_011()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :F(test);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_GOTO_012()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :F<test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_GOTO_013()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :F(test);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }


    [TestMethod]
    public void TEST_GOTO_014()
    {
        var s = @"
        s = 'test1   a = 42   :(end)'
        test = code(s)
        :F<test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }







    [TestMethod]
    public void TEST_GOTO_101()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  s  (test1)  f  (test);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_102()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  f  (test)  s  (test1);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_103()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  s  <test>  f  (test);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_104()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  f  <test>  s  (test1);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_105()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  s  (test1)  f  <test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_106()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  f  (test)  s  <test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_107()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  s  <test>  f  <test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_108()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  f  <test>  s  <test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_109()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  s  <test>  f  <test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }


    [TestMethod]
    public void TEST_GOTO_110()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  s  <test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("42", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_GOTO_111()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  f  (test);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_GOTO_112()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  f  <test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_GOTO_113()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  f  (test);test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }


    [TestMethod]
    public void TEST_GOTO_114()
    {
        var s = @"
          s   = 'test1   a = 42   :(end)'
        test = code(s)
        :  f  <test>;test
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

}
