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

    // Backspace_316: pattern arg as channel name → error 316
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

    // Backspace_317: channel not open → error 317
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

    // Backspace_318: output-only channel does not permit backspace → error 318
    [TestMethod]
    public void Backspace_318()
    {
        var testFile = Path.GetTempFileName();
        try
        {
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
        finally { File.Delete(testFile); }
    }

    // Backspace_001: read 5 lines, backspace, re-read the 5th line
    [TestMethod]
    public void Backspace_001()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            // Write known content so we can assert exact line values
            File.WriteAllLines(testFile, new[]
            {
                "Line 1: alpha",
                "Line 2: beta",
                "Line 3: gamma",
                "Line 4: delta",
                "Line 5: epsilon",
                "Line 6: zeta",
            });

            var s = $@"
        input('read','2','{testFile}')
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
            var a = ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data;
            var b = ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data;
            Assert.AreEqual("Line 6: zeta", a);
            Assert.AreEqual("Line 6: zeta", b);
        }
        finally { File.Delete(testFile); }
    }
}
