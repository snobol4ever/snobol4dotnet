using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

// Error-code tests that require no external files — always run.
[TestClass]
public class RewindErrors
{
    [TestMethod]
    public void Rewind_003()
    {
        var s = """
                         rewind(any('2'))
                 end
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(172, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Rewind_173()
    {
        var s = """
                     rewind('')
                 end
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(173, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Rewind_174()
    {
        var s = """
                         rewind('2')
                 end
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(174, build.ErrorCodeHistory[0]);
    }
}

[TestClass]
public class Rewind
{
    // Rewind_001: read 6 lines, rewind, read 6 again — last line should match
    [TestMethod]
    public void Rewind_001()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(testFile, new[]
            {
                "Line 1", "Line 2", "Line 3", "Line 4", "Line 5", "Line 6",
            });

            var s = $"""
                             input('read','2','{testFile}')
                             a = read
                             a = read
                             a = read
                             a = read
                             a = read
                             a = read
                             rewind('2')
                             b = read
                             b = read
                             b = read
                             b = read
                             b = read
                             b = read
                             endfile('2')
                     end
                     """;
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data,
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        }
        finally { File.Delete(testFile); }
    }

    // Rewind_002: write 3 lines, rewind, overwrite first line, read back
    [TestMethod]
    public void Rewind_002()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            var s = $"""
                             output('write','3','{testFile}')
                             write = 'I am already far north of London, and as I walk in the streets of'
                             write = 'Petersburgh, I feel a cold northern breeze play upon my cheeks, which'
                             write = 'braces my nerves and fills me with delight. Do you understand this'
                             rewind('3')
                             write = 'I am far north of London, and as I walk in the streets of'
                             endfile('3')
                             input('read','2','{testFile}')
                             a = read
                             b = read
                             endfile('2')
                     end
                     """;
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("I am far north of London, and as I walk in the streets of",
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
            // Second read: the new shorter first line overwrote the start of the old first line,
            // so the remainder of the original bytes follow (OS-specific line endings affect this).
            if (SetupTests.IsLinux)
                Assert.AreEqual("eets of", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
            else
                Assert.AreEqual("ets of", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        }
        finally { File.Delete(testFile); }
    }
}
