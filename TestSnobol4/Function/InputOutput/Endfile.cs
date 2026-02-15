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

    [TestMethod]
    public void Endfile_096()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\Eject.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Eject.txt";

        var s = $"""
                         output('write','2','{testFile}')
                         write = 'This ebook is for the use of anyone anywhere in the United States and'
                         write = 'is licensed under the Creative Commons Attribution-NonCommercial-NoDerivs 3.0 Unported License.'
                         write = 'You may copy it, give it away or re-use it under the terms'
                         write = 'To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-nd/3.0/'
                         endfile(any('2'))
                 end
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(96, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Endfile_093()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\Eject2.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Eject2.txt";

        var s = $"""
                         output('write','2','{testFile}')
                         write = 'This ebook is for the use of anyone anywhere in the United States and'
                         write = 'is licensed under the Creative Commons Attribution-NonCommercial-NoDerivs 3.0 Unported License.'
                         write = 'You may copy it, give it away or re-use it under the terms'
                         write = 'To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-nd/3.0/'
                         endfile("")
                 end
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(97, build.ErrorCodeHistory[0]);
    }
    
    [TestMethod]
    public void Endfile_098()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\Eject3.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Eject3.txt";

        var s = $"""
                         output('write','2','{testFile}')
                         write = 'This ebook is for the use of anyone anywhere in the United States and'
                         write = 'is licensed under the Creative Commons Attribution-NonCommercial-NoDerivs 3.0 Unported License.'
                         endfile('2')
                         write = 'You may copy it, give it away or re-use it under the terms'
                         write = 'To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-nd/3.0/'
                 end
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", build.Execute!.IdentifierTable[build.FoldCase("","a")].OutputChannel);
    }
}