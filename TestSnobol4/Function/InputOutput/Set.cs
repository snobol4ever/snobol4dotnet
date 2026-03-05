using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

// Tests require external data files from dev machine (hardcoded Windows paths).
[TestClass, Ignore]
public class Set
{
    //"set first argument is not a suitable name" /* 291 */,
    //"set first argument is null" /* 292 */,
    //"inappropriate second argument to set" /* 293 */,
    //"inappropriate third argument to set" /* 294 */,
    //"set file does not exist" /* 295 */,
    //"set file does not permit setting file pointer" /* 296 */,
    //"set caused non-recoverable I/O error" /* 297 */,

    [TestMethod]
    public void Set_291()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\RecordTest.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/RecordTest.txt";

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

    [TestMethod]
    public void Set_292()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\RecordTest2.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/RecordTest2.txt";

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

    [TestMethod]
    public void Set_293()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\RecordTest3.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/RecordTest3.txt";

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

    [TestMethod]
    public void Set_294()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\RecordTest4.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/RecordTest4.txt";

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

    [TestMethod]
    public void Set_295()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\RecordTest0.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Output/RecordTest0.txt";

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

    [TestMethod]
    public void Set_001()
    {

        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\RecordTest5.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Output/RecordTest5.txt";

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

    [TestMethod]
    public void Set_002()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\RecordTest6.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Output/RecordTest6.txt";

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

    [TestMethod]
    public void Set_003()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\RecordTest7.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Output/RecordTest7.txt";

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

    [TestMethod]
    public void Set_004()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\RecordTest8.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/RecordTest8.txt";

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
}