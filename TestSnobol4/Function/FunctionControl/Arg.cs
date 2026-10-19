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
                r1 = arg('pythagoras',1);
                r2 = arg('pythagoras',2);

end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // sbl -b: ARG gives the parameter's NAME (DATATYPE NAME, printing as A and B)
        Assert.AreEqual(build.FoldCase("a"), ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Pointer);
        Assert.AreEqual(build.FoldCase("b"), ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Pointer);
    }

    [TestMethod]
    public void TEST_Arg_002()
    {
        var s = @"
                define('pythagoras(a,b)c,d')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                r1 = arg('pythagoras',0)  :f(n)
                r2 = 'success' :(end)
n               r2 = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
    }

    [TestMethod]
    public void TEST_Arg_003()
    {
        var s = @"
                define('pythagoras(a,b)c,d')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                r1 = arg('pythagoras',3)  :f(n)
                r2 = 'success' :(end)
n               r2 = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
    }

    [TestMethod]
    public void TEST_Arg_004()
    {
        var s = @"
                define('pythagoras(a,b)c,d')  :(double_end)
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
                define('pythagoras(a,b)c,d')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                r1 = arg('pythagoras',any('123'))  :f(n)
                r2 = 'success' :(end)
n               r2 = 'failure'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(62, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Arg_and_Local_return_names()
    {
        // sbl -b: ARG and LOCAL return the NAME of the parameter or local, so $ARG(.F,2) reads B, IDENT(ARG(.F,2),
        // .B) succeeds and $LOCAL(.F,1) = 'lv' assigns L; testpgms' test1 checks DIFFER(ARG(.JLAB,1),'A').
        // snobol4dotnet returned the name as a string
        var build = SetupTests.SetupScript("-b", """
                 DEFINE('F(A,B)L')
                 N = ARG(.F,2)
                 B = 'bval'
                 R = DATATYPE(N) ' ' $N
                 R = R IDENT(N, .B) ' same'
                 $LOCAL(.F,1) = 'lv'
                 R = R ' ' L ' ' DATATYPE(LOCAL(.F,1)) ' ' ARG(.F,1)
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("NAME bval same lv NAME A", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data);
    }
}
