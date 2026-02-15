using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.FunctionControl;

[TestClass]
public class Arg
{

    [TestMethod]
    public void TEST_Arg_001()
    {
        var s = @"
                define('pythagoras(a,b)c,d')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                R1 = arg('PYTHAGORAS',1);
                R2 = arg('PYTHAGORAS',2);

end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(build.FoldCase("","a"), ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual(build.FoldCase("","b"), ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Arg_002()
    {
        var s = @"
                define('PYTHAGORAS(a,b)c,d')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                r1 = arg('PYTHAGORAS',0)  :f(n)
                r2 = 'success' :(end)
n               r2 = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Arg_003()
    {
        var s = @"
                define('PYTHAGORAS(a,b)c,d')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                r1 = arg('PYTHAGORAS',3)  :f(n)
                r2 = 'success' :(end)
n               r2 = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r1")]).Data);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r2")]).Data);
    }

    [TestMethod]
    public void TEST_Arg_004()
    {
        var s = @"
                define('PYTHAGORAS(a,b)c,d')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                r1 = arg('pithagoras',1)  :f(n)
                r2 = 'success' :(end)
n               r2 = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(63, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Arg_005()
    {
        var s = @"
                define('PYTHAGORAS(a,b)c,d')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                r1 = arg('PYTHAGORAS',any('123'))  :f(n)
                r2 = 'success' :(end)
n               r2 = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(62, build.ErrorCodeHistory[0]);
    }
}