using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class BreakX
{

    [TestMethod]
    public void TEST_BreakX_001()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        gap = breakx(letters) 
        subject = ':= one,,, two,..  three';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual(":= ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp1")]).Data);
    }

    [TestMethod]
    public void TEST_BreakX_002()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        gap = notany(letters) breakx(letters) 
        subject = 'one,,, two,..  three';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual(",,, ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp1")]).Data);
    }

    [TestMethod]
    public void TEST_BreakX_003()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        gap = breakx(letters) 
        subject = 'one,,, two,..  three';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp1")]).Data);
    }

    [TestMethod]
    public void TEST_BreakX_004()
    {
        var s = @"
        &anchor = 0
        letters = ''
        gap = breakx(letters) 
        subject = 'one,,, two,..  three';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(45, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_BreakX_005()
    {
        var s = @"
        &anchor = 0
        subject = 'c'
        pattern = breakx('c')
        subject pattern = '****'   :f(n)
        temp1 = '[' subject ']'   
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual("[****c]", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp1")]).Data);
    }

    [TestMethod]
    public void TEST_BreakX_006()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        gap = breakx(letters) 
        subject = '5675765()*)(*)';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_BreakX_007()
    {
        var s = @"
        &anchor = 0
        subject = '12345'
        char = '3'
        gap = breakx(char)
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual("12", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp1")]).Data);
    }


    [TestMethod]
    public void TEST_BreakX_008()
    {
        var s = @"
        &anchor = 0
        subject = '12345'
        char = ''
        gap = breakx(char)
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(45, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_BreakX_009()
    {
        var s = @"
        &anchor = 0
        subject = '12345'
        char = '8'
        gap = breakx(char)
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_BreakX_010()
    {
        var s = @" 
        A = BREAKX(*B)
        B = '123'
        'ABCD3FG' A . R1
        B = 'ABC'
        '1234C56' A . R2
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("ABCD", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("1234", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

    [TestMethod]
    public void TEST_BreakX_011()
    {
        var s = @" 
        A = BREAKX(*B)
        B = '123' | 'ABC'
        'ABCD3FG' A . R1
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(45, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_BreakX_012()
    {
        var s = @"
        A = BREAKX('123456')
        '' A.R1
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
    }

    [TestMethod]
    public void TEST_BreakX_013()
    {
        var s = @"
        A = BREAKX('')
        '123456' A.R1
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(45, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_BreakX_014()
    {
        var s = @"
	SUB = 'EXCEPTIONS-ARE-AS-TRUE-AS-RULES'
	P1 = POS(0) BREAKX('A') . R2 'AS'
	SUB P1    :S(Y)F(N)
Y	R1 = 'SUCCESS' :(end)
N	R1 = 'FAILURE'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("SUCCESS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("EXCEPTIONS-ARE-", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

}