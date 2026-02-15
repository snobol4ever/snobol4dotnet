using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class Break
{

    [TestMethod]
    public void TEST_Break_001()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        gap = break(letters) 
        subject = ':= one,,, two,..  three';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual(":= ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp1")]).Data);
    }

    [TestMethod]
    public void TEST_Break_002()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        gap = notany(letters) break(letters) 
        subject = 'one,,, two,..  three';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual(",,, ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp1")]).Data);
    }

    [TestMethod]
    public void TEST_Break_003()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        gap = break(letters) 
        subject = 'one,,, two,..  three';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp1")]).Data);
    }

    [TestMethod]
    public void TEST_Break_004()
    {
        var s = @"
        &anchor = 0
        letters = ''
        gap = break(letters) 
        subject = 'one,,, two,..  three';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(69, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Break_005()
    {
        var s = @"
        &anchor = 0
        subject = 'c'
        pattern = break('c')
        subject pattern = '****'   :f(n)
        temp1 = '[' subject ']'   
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("[****c]", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp1")]).Data);
    }

    [TestMethod]
    public void TEST_Break_006()
    {
        var s = @"
        &anchor = 0
        letters = 'abcdefghijklmnopqrstuvwxyz'
        gap = break(letters) 
        subject = '5675765()*)(*)';
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Break_007()
    {
        var s = @"
        &anchor = 0
        subject = '12345'
        char = '3'
        gap = break(char)
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("12", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp1")]).Data);
    }


    [TestMethod]
    public void TEST_Break_008()
    {
        var s = @"
        &anchor = 0
        subject = '12345'
        char = ''
        gap = break(char)
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(69, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Break_009()
    {
        var s = @"
        &anchor = 0
        subject = '12345'
        char = '8'
        gap = break(char)
        subject gap . temp1  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
    }

    [TestMethod]
    public void TEST_Break_010()
    {
        var s = @" 
        A = BREAK(*B)
        B = '123'
        'ABCD3FG' A . R1
        B = 'ABC'
        '1234C56' A . R2
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("ABCD", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("1234", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Break_011()
    {
        var s = @" 
        A = BREAK(*B)
        B = '123' | 'ABC'
        'ABCD3FG' A . R1
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(69, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Break_012()
    {
        var s = @"
        A = BREAK('123456')
        '' A.R1
END
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
    }

    [TestMethod]
    public void TEST_Break_013()
    {
        var s = @"
        A = BREAK('')
        '123456' A.R1
END
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(69, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Break_014()
    {
        var s = @"
	SUB = 'EXCEPTIONS-ARE-AS-TRUE-AS-RULES'
	P1 = POS(0) BREAK('A') . R2 'AS'
	SUB P1    :S(Y)F(N)
Y	R1 = 'SUCCESS' :(END)
N	R1 = 'FAILURE'
END
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("FAILURE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }


}