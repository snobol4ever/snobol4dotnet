using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

[TestClass]
public class Load
{
    [TestMethod]
    public void TEST_Load_001()
    {
        var dllName = SetupTests.AreaLibraryPath;
        Assert.IsTrue(File.Exists(dllName), $"AreaLibrary.dll not found at: {dllName}");

        var s = $"""

                         load('{dllName}', 'AreaFunction.Area')
                         r1 = 'Area of circle with radius ' 4.5 ' is ' AreaOfCircle(4.5)
                         r2 = 'Area of square with side  ' 15.9 ' is ' AreaOfSquare(15.9)
                         unload('{dllName}')
                 end
                 """;
        const string directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("Area of circle with radius 4.5 is 63.61725123519331",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("Area of square with side  15.9 is 252.81",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
    }
}
