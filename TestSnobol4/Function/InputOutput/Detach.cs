using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

[TestClass]
public class Detach
{
    // "detach argument is not appropriate name" /* 87 */,

    [TestMethod]
    public void Detach_001()
    {
        var testFile = Path.Combine(SetupTests.WindowsOutput, "Frankenstein3.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "Frankenstein3.txt");

        var s = $@"
        input('READ','2','{testFile}')
        a = output = READ
        a = output = READ
        a = output = READ
        detach('READ')
        b = READ
        endfile('2')  
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("This ebook is for the use of anyone anywhere in the United States and", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void Detach_002()
    {
        var testFile = Path.Combine(SetupTests.WindowsOutput, "Frankenstein4.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "Frankenstein4.txt");

        var s = $@"
        input('READ','2','{testFile}')
        detach('READ')
        input('READ','2')
        a = output = READ
        a = output = READ
        a = output = READ
        detach('READ')
        b = READ
        endfile('2')  
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("This ebook is for the use of anyone anywhere in the United States and", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void Detach_087()
    {
        var testFile = Path.Combine(SetupTests.WindowsOutput, "Frankenstein5.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "Frankenstein5.txt");

        var s = $@"
        input('READ','2','{testFile}')
        detach(any('READ'))
        endfile('2')  
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(87, build.ErrorCodeHistory[0]);
    }
}