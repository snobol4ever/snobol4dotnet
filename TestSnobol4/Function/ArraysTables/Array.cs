using Snobol4.Common;
using Test.TestLexer;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace Test.ArraysTables;

[TestClass]
public class Array
{

    [TestMethod]
    public void TEST_Array_001()
    {
        var s = @"
        a = array('-5:10,3:5,20')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Array_002()
    {
        var s = @"
        a = array(20)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Array_003()
    {
        var s = @"
        a = array('-a:10,3:5,20')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(65, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_004()
    {
        var s = @"
        a = array('-5:10,b:5,20')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(65, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_005()
    {
        var s = @"
        a = array('-5:a,3:5,20')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(66, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_006()
    {
        var s = @"
        a = array('-5:a,3:5,20')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(66, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_007()
    {
        var s = @"
        a = array('-5:a,3:5,20')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(66, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_008()
    {
        var s = @"
        a = array('-5:10,3:5,c')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_009()
    {
        var s = @"
        a = array('-5:5,5:3,5')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_010()
    {
        var s = @"
        a = 'abc'
        b = a<3>
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(1, build.ErrorCodeHistory.Count);
        Assert.AreEqual(235, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_011()
    {
        var s = @"
        a = array(20)
        b = a<'abc'>
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(238, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_012()
    {
        var s = @"
        a = array('20,10,5')
        b = a<30,1>
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(236, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_013()
    {
        var s = @"
        a = array(0)
        b = a<30>
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s); 
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_014()
    {
        var s = @"
        a = array(10)
        b = a<-1>       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_015()
    {
        var s = @"
        a = array(0)
        b = a<3,3>
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_016()
    {
        var s = @"
        a = array('3,3')
        b = a<1>
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(236, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_017()
    {
        var s = @"
        a = array('0,3:5,20')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_018()
    {
        var s = @"
        a = array('3:5,0,20')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_019()
    {
        var s = @"
        a = array('3:5,20,0')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_020()
    {
        var s = @"
        a = array(0)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(67, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_021()
    {
        var s = @"
        a = array('20,10,5')
        b = a<6,1,5,2>
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(236, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Array_022()
    {
        var s = @"
        a = array(10)
        b = a<0>       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_023()
    {
        var s = @"
        a = array(10)
        b = a<1>       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_024()
    {
        var s = @"
        a = array(10)
        b = a<10>       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_025()
    {
        var s = @"
        a = array(10)
        b = a<11>       :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Array_026()
    {
        var s = @"
        a = array(10,999)
        b = a<3>
        eq(b,999) :s(y)f(n)
y       result = 'success' :(end)
n       result = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

}

