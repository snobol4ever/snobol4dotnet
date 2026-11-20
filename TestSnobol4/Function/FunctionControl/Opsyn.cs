using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

[TestClass]
public class Opsyn
{

    // Requires AreaLibrary.dll — resolved from solution root via SetupTests.AreaLibraryPath.
    [TestMethod]
    public void TEST_Opsyn_001()
    {
        var dllName = SetupTests.AreaLibraryPath;
        Assert.IsTrue(File.Exists(dllName), $"AreaLibrary.dll not found at: {dllName}");

        var s = $"""

                         load('{dllName}', 'AreaFunction.Area')
                         opsyn('#','AreaOfCircle',1)
                         opsyn('%','AreaOfSquare',1)
                         r1 = 'Area of circle with radius ' 4.5 ' is ' #4.5
                         r2 = 'Area of square with side  ' 15.9 ' is ' %15.9
                         unload('{dllName}')
                 end
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("Area of circle with radius 4.5 is 63.6172512351933", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("Area of square with side  15.9 is 252.81", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
    }
}
