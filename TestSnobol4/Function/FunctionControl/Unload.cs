using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

[TestClass]
public class Unload
{
    [TestMethod]
    public void TEST_Unload_001()
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
        Assert.AreEqual("Area of circle with radius 4.5 is 63.6172512351933",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("Area of square with side  15.9 is 252.81",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
    }

    [TestMethod]
    public void TEST_Unload_a_function_name_undefines_it()
    {
        // sbl -b: UNLOAD('F') undefines a DEFINE'd F (a call is then error 22), UNLOAD('NOSUCH') is no
        // error, and UNLOAD('SIZE') is error 248; snobol4dotnet crashed on all three
        var user = SetupTests.SetupScript("-b", @"
        DEFINE('F()')
        UNLOAD('F')
        UNLOAD('NOSUCH')
        R = 'unloaded'
        S = F()
end");
        Assert.AreEqual("unloaded", user.Execute!.IdentifierTable["R"].ToString());
        Assert.AreEqual(1, user.ErrorCodeHistory.Count);
        Assert.AreEqual(22, user.ErrorCodeHistory[0]);

        var builtin = SetupTests.SetupScript("-b", @"
        UNLOAD('SIZE')
end");
        Assert.AreEqual(248, builtin.ErrorCodeHistory[0]);
    }
}
