using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Any
{

    [TestMethod]
    public void TEST_Any_001()
    {
        var s = @"
        &anchor = 0
        vowel = any('aeiou')
        subject = 'vacuum'
        subject vowel . v1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("a", ((StringVar)build.Execute!.IdentifierTable["V1"]).Data);
    }

    [TestMethod]
    public void TEST_Any_002()
    {
        var s = @"
        &anchor = 0
        dvowel = any('aeiou') any('aeiou') 
        subject = 'vacuum'
        subject dvowel . v1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("uu", ((StringVar)build.Execute!.IdentifierTable["V1"]).Data);
    }

    [TestMethod]
    public void TEST_Any_003()
    {
        var s = @"
        pattern = any(arb) 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(59, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Any_004()
    {
        var s = @"
        pattern = any('') 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(59, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Any_005()
    {
        var s = @"
        &anchor = 0
        vowel = any('aeiou')
        subject = ''
        subject vowel . v1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
    }

    [TestMethod]
    public void TEST_Any_006()
    {
        var s = @" 
        A = ANY(*B)
        B = '123'
        'ABCD3FG' A . R1
        B = 'ABC'
        '1234C56' A . R2
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("3", ((StringVar)build.Execute!.IdentifierTable["R1"]).Data);
        Assert.AreEqual("C", ((StringVar)build.Execute!.IdentifierTable["R2"]).Data);
    }

    [TestMethod]
    public void TEST_Any_007()
    {
        var s = @" 
        A = ANY(*B)
        B = '123' | 'ABC'
        'ABCD3FG' A . R1
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(43, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Any_008()
    {
        var s = @" 
        A = ANY('')
        '123456' A . R1
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(59, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Any_009()
    {
        var s = @" 
        A = ANY('123456')
        '' A . R1  :S(END)F(N)
N       R1 = 'FAIL'
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("FAIL", ((StringVar)build.Execute!.IdentifierTable["R1"]).Data);
    }
}