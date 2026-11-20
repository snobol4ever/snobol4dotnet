using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.FunctionControl;

[TestClass]
public class Local
{
    [TestMethod]
    public void TEST_Local_001()
    {
        var s = @"
                define('pythagoras(a,b)c,d')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                r1 = local('pythagoras',1);
                r2 = local('pythagoras',2);

end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // sbl -b: LOCAL gives the local's NAME (DATATYPE NAME, printing as C and D)
        Assert.AreEqual(build.FoldCase("c"), ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Pointer);
        Assert.AreEqual(build.FoldCase("d"), ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Pointer);
    }
}