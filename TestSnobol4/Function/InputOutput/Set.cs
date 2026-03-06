using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

[TestClass]
public class Set
{
    //"set first argument is not a suitable name" /* 291 */,
    //"set first argument is null" /* 292 */,
    //"inappropriate second argument to set" /* 293 */,
    //"inappropriate third argument to set" /* 294 */,
    //"set file does not exist" /* 295 */,
    //"set file does not permit setting file pointer" /* 296 */,
    //"set caused non-recoverable I/O error" /* 297 */,

    // Set_291: pattern arg as channel → error 291
    [TestMethod]
    public void Set_291()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            WriteRecordFile(testFile, 8);
            var s = $@"
        input('READ','2','{testFile}')
        output = read
        output = read
        output = read
        output = read
        set(any('2'),4,0)
        output = ''
        a = output = read
        endfile('2')
end
";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(291, build.ErrorCodeHistory[0]);
        }
        finally { File.Delete(testFile); }
    }

    // Set_292: empty channel string → error 292
    [TestMethod]
    public void Set_292()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            WriteRecordFile(testFile, 8);
            var s = $@"
        input('READ','2','{testFile}')
        output = read
        output = read
        output = read
        output = read
        set('',4,0)
        output = ''
        a = output = read
        endfile('2')
end
";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(292, build.ErrorCodeHistory[0]);
        }
        finally { File.Delete(testFile); }
    }

    // Set_293: pattern arg as offset → error 293
    [TestMethod]
    public void Set_293()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            WriteRecordFile(testFile, 8);
            var s = $@"
        input('READ','2','{testFile}')
        output = read
        output = read
        output = read
        output = read
        set('2',any('4'),0)
        output = ''
        a = output = read
        endfile('2')
end
";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(293, build.ErrorCodeHistory[0]);
        }
        finally { File.Delete(testFile); }
    }

    // Set_294: whence=2 is not supported → error 294
    [TestMethod]
    public void Set_294()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            WriteRecordFile(testFile, 8);
            var s = $@"
        input('READ','2','{testFile}')
        output = read
        output = read
        output = read
        output = read
        set('2',4,2)
        output = ''
        a = output = read
        endfile('2')
end
";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(294, build.ErrorCodeHistory[0]);
        }
        finally { File.Delete(testFile); }
    }

    // Set_295: channel '55' not open → error 295
    [TestMethod]
    public void Set_295()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            WriteRecordFile(testFile, 8);
            var s = $@"
        input('read','2','{testFile}')
        output = read
        output = read
        output = read
        output = read
        set('55',4,0)
        output = ''
        a = output = read
        endfile('2')
end
";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(295, build.ErrorCodeHistory[0]);
        }
        finally { File.Delete(testFile); }
    }

    // Set_001: seek to record 4 (0-based) from start, read → "Record 5"
    [TestMethod]
    public void Set_001()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            WriteRecordFile(testFile, 10);
            var s = $@"
        input('read','2','{testFile}')
        output = read
        output = read
        output = read
        output = read
        set('2',4,0)
        output = ''
        a = output = read
        endfile('2')
end
";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("Record 5", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        }
        finally { File.Delete(testFile); }
    }

    // Set_002: seek to record 0 from start (rewind), read → "Record 1"
    [TestMethod]
    public void Set_002()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            WriteRecordFile(testFile, 10);
            var s = $@"
        input('read','2','{testFile}')
        output = read
        output = read
        set('2',0,0)
        output = ''
        a = output = read
        endfile('2')
end
";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("Record 1", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        }
        finally { File.Delete(testFile); }
    }

    // Set_003: read 2 lines, seek 4 forward from current, read → "Record 7"
    [TestMethod]
    public void Set_003()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            WriteRecordFile(testFile, 10);
            var s = $@"
        input('read','2','{testFile}')
        output = read
        output = read
        set('2',4,1)
        output = ''
        a = output = read
        endfile('2')
end
";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("Record 7", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        }
        finally { File.Delete(testFile); }
    }

    // Set_004: read 5 lines, seek -1 from current (back 1), read → "Record 5"
    [TestMethod]
    public void Set_004()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            WriteRecordFile(testFile, 10);
            var s = $@"
        input('read','2','{testFile}')
        output = read
        output = read
        output = read
        output = read
        output = read
        set('2',-1,1)
        output = ''
        a = output = read
        endfile('2')
end
";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("Record 5", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        }
        finally { File.Delete(testFile); }
    }

    /// <summary>Write n lines "Record 1" … "Record n" to a temp file.</summary>
    private static void WriteRecordFile(string path, int count)
    {
        File.WriteAllLines(path, Enumerable.Range(1, count).Select(i => $"Record {i}"));
    }
}
