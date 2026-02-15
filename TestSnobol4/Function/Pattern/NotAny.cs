using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class NotAny
{

    [TestMethod]
    public void TEST_NotAny_001()
    {
        var s = @"
        &anchor = 0
        notVowel = notany('aeiou')
        subject = 'vacuum'
        subject notVowel . v1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("v", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","v1")]).Data);
    }

    [TestMethod]
    public void TEST_NotAny_002()
    {
        var s = @"
        &anchor = 0
        dvowel = any('aeiou') notany('aeiou') 
        subject = 'vacuum'
        subject dvowel . v1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("ac", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","v1")]).Data);
    }

    [TestMethod]
    public void TEST_NotAny_003()
    {
        var s = @"
        pattern = notany(arb) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(151, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_NotAny_004()
    {
        var s = @"
        pattern = notany('') 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(151, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_NotAny_005()
    {
        var s = @"
        &anchor = 0
        vowel = notany('aeiou')
        subject = ''
        subject vowel . v1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_NotAny_006()
    {
        var s = @" 
        A = NOTANY(*B)
        B = '123'
        '123ABC' A . R1
        B = 'ABC'
        'ABC123' A . R2
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("A", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("1", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_NotAny_007()
    {
        var s = @" 
        A = NOTANY(*B)
        B = '123' | 'ABC'
        'ABCD3FG' A . R1
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(49, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_NotAny_008()
    {
        var s = @" 
        A = NOTANY('')
        '123456' A . R1
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(151, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_NotAny_009()
    {
        var s = @" 
        A = NOTANY('123456') 
        '' A . R1 :S(END)F(N)
N       R1 = 'FAIL'
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("FAIL", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
    }


}