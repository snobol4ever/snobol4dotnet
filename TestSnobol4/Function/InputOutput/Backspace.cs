using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

[TestClass]
public class Backspace
{
    //"backspace argument is not a suitable name" /* 316 */,
    //"backspace file does not exist" /* 317 */,
    //"backspace file does not permit backspace" /* 318 */,
    //"backspace caused non-recoverable error" /* 319 */,

    [TestMethod]
    public void Backspace_001()
    {
        var testFile = Path.Combine(SetupTests.WindowsOutput, "Frankenstein.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "Frankenstein.txt");


var s = $@"
        input('READ','2','{testFile}')
        d = read
        d = read
        d = read
        d = read
        d = read
        a = read
        backspace('2')
        b = read
        endfile('2')  
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("of the Project Gutenberg License included with this ebook or online", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("of the Project Gutenberg License included with this ebook or online", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void Backspace_316()
    {
        var s = @"
        backspace(any('2'))
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(316, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Backspace_317()
    {
        var s = @"
        backspace('2')
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(317, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Backspace_318()
    {
        var testFile = Path.Combine(SetupTests.WindowsOutput, "test319.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "test319.txt");

        var s = $@"
        output('read','2','{testFile}')
        backspace('2')
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(318, build.ErrorCodeHistory[0]);
    }
}