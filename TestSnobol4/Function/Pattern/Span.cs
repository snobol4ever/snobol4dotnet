using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Span
{

    [TestMethod]
    public void TEST_Span_001()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        word = span(letters) 
        subject = ':= one,,, two,..  three';
        subject word . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("one", ((StringVar)build.Execute!.IdentifierTable["TEMP1"]).Data);
    }

    [TestMethod]
    public void TEST_Span_002()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        word = span(letters) 
        subject = 'one,,, two,..  three';
        subject word . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("one", ((StringVar)build.Execute!.IdentifierTable["TEMP1"]).Data);
    }

    [TestMethod]
    public void TEST_Span_003()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        word = span(letters) 
        subject = '86%^';
        subject word . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable["TEMP1"]).Data);
    }

    [TestMethod]
    public void TEST_Span_004()
    {
        var s = @"
        &anchor = 0
        letters = ''
        gap = span(letters) 
        subject = 'one,,, two,..  three';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(188, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Span_005()
    {
        var s = @"
        &anchor = 0
        subject = 'c'
        pattern = span('c')
        subject pattern = '****'   :f(n)
        temp1 = '[' subject ']'   
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("[****]", ((StringVar)build.Execute!.IdentifierTable["TEMP1"]).Data);
    }

    [TestMethod]
    public void TEST_Span_006()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        gap = span(letters) 
        subject = '5675765()*)(*)';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable["TEMP1"]).Data);
    }

    [TestMethod]
    public void TEST_Span_007()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        word = span(letters) 
        subject = 'one';
        subject word . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("one", ((StringVar)build.Execute!.IdentifierTable["TEMP1"]).Data);
    }

    [TestMethod]
    public void TEST_Span_008()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        word = span(letters)
        gap = break(letters)
        subject = 'sample line'
        subject word . word1  :f(n)
        subject = 'plus ten degrees'
        subject word . word2  :f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("sample", ((StringVar)build.Execute!.IdentifierTable["WORD1"]).Data);
        Assert.AreEqual("plus", ((StringVar)build.Execute!.IdentifierTable["WORD2"]).Data);
    }

    [TestMethod]
    public void TEST_Span_009()
    {
        var s = @"
        &anchor = 0
        digits = '0123456789'
        integer = (any('+-') | '') span(digits)
        subject = 'set -43 volts'
        subject integer . integer1  :f(n)
        subject = 'set -43.625 volts'
        subject integer . integer2  :f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable["RESULT"]).Data);
        Assert.AreEqual("-43", ((StringVar)build.Execute!.IdentifierTable["INTEGER1"]).Data);
        Assert.AreEqual("-43", ((StringVar)build.Execute!.IdentifierTable["INTEGER2"]).Data);
    }

    [TestMethod]
    public void TEST_Span_010()
    {
        var s = @" 
        A = SPAN(*B)
        B = '123'
        '333' A
        '333' A . R1
        B = 'ABC'
        'CCC' A . R2
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("333", ((StringVar)build.Execute!.IdentifierTable["R1"]).Data);
        Assert.AreEqual("CCC", ((StringVar)build.Execute!.IdentifierTable["R2"]).Data);
    }

    [TestMethod]
    public void TEST_Span_011()
    {
        var s = @" 
        A = SPAN(*B)
        B = '123' | 'ABC'
        'ABCD3FG' A . R1
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(56, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Span_012()
    {
        var s = @"
        A = SPAN('123456')
        '' A . R1 :S(END)
        R1 = 'fail'
END
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable["R1"]).Data);
    }

    [TestMethod]
    public void TEST_Span_013()
    {
        var s = @"
        A = SPAN('')
        '123456' A.R1
END
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(188, build.ErrorCodeHistory[0]);
    }

}