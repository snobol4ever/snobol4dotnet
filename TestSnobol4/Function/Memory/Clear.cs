using Snobol4.Common;
using Test.TestLexer;

namespace Test.Memory;

[TestClass]
public class Clear
{

    [TestMethod]
    public void TEST_Clear_001f()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        skip = 'b,bb'
        clear()
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","aa")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","bb")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","cc")]).Data);
    }

    [TestMethod]
    public void TEST_Clear_002f()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        skip = 'b,bb'
        clear(skip)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("b", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","aa")]).Data);
        Assert.AreEqual("2", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","bb")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","cc")]).Data);
    }

    [TestMethod]
    public void TEST_Clear_003f()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        skip = ''
        clear(skip)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","aa")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","bb")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","cc")]).Data);
    }

    [TestMethod]
    public void TEST_Clear_004f()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        skip = 'a,,b'
        clear(skip)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(72, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Clear_005f()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        clear(any('123'))
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(71, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Clear_001()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        skip = 'b,bb'
        clear()
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","aa")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","bb")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","cc")]).Data);
    }

    [TestMethod]
    public void TEST_Clear_002()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        skip = 'B,BB'
        clear(skip)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("b", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","aa")]).Data);
        Assert.AreEqual("2", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","bb")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","cc")]).Data);
    }

    [TestMethod]
    public void TEST_Clear_003()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        skip = ''
        clear(skip)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","aa")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","bb")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","cc")]).Data);
    }

    [TestMethod]
    public void TEST_Clear_004()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        skip = 'a,,b'
        clear(skip)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(72, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Clear_005()
    {
        var s = @"

        a = 'a'
        b = 'b'
        c = 'c'
        aa = '1'
        bb = '2'
        cc = '3'
        clear(any('123'))
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(71, build.ErrorCodeHistory[0]);
    }

}