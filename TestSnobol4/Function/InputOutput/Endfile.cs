using Test.TestLexer;

namespace Test.InputOutput;

[TestClass]
public class Endfile
{
    //"endfile argument is not a suitable name" /* 96 */,
    //"endfile argument is null" /* 97 */,
    //"endfile file does not exist" /* 98 */,
    //"endfile file does not permit endfile" /* 99 */,
    //"endfile caused non-recoverable output error" /* 100 */,

    // Endfile_096: bad channel name (pattern arg) → error 96
    [TestMethod]
    public void Endfile_096()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            var s = $"""
                             output('write','2','{testFile}')
                             write = 'line 1'
                             endfile(any('2'))
                     end
                     """;
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(96, build.ErrorCodeHistory[0]);
        }
        finally { File.Delete(testFile); }
    }

    // Endfile_093: null channel string → error 97
    [TestMethod]
    public void Endfile_093()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            var s = $"""
                             output('write','2','{testFile}')
                             write = 'line 1'
                             endfile("")
                     end
                     """;
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(97, build.ErrorCodeHistory[0]);
        }
        finally { File.Delete(testFile); }
    }

    // Endfile_098: endfile closes channel; variable loses its output channel
    [TestMethod]
    public void Endfile_098()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            var s = $"""
                             output('write','2','{testFile}')
                             write = 'line 1'
                             write = 'line 2'
                             endfile('2')
                             write = 'line 3'
                     end
                     """;
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("", build.Execute!.IdentifierTable[build.FoldCase("a")].OutputChannel);
        }
        finally { File.Delete(testFile); }
    }
}
