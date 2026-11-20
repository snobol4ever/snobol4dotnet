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
        var testFile = Path.Combine(SetupTests.WindowsOutput, "Eject.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "Eject.txt");

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
        var testFile = Path.Combine(SetupTests.WindowsOutput, "Eject2.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "Eject2.txt");

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
        var testFile = Path.Combine(SetupTests.WindowsOutput, "Eject3.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "Eject3.txt");

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
        Assert.AreEqual("", build.Execute!.IdentifierTable[build.FoldCase("a")].OutputChannel);
    }

    [TestMethod]
    public void Endfile_happy_path()
    {
        // Write lines then endfile — file gets both lines
        var testFile = Path.GetTempFileName();
        try
        {
            var s = $"""
                             output('write','3','{testFile}')
                             write = 'alpha'
                             write = 'beta'
                             endfile('3')
                     end
                     """;
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            var lines = File.ReadAllLines(testFile);
            Assert.IsTrue(lines.Length >= 2);
            Assert.AreEqual("alpha", lines[0]);
            Assert.AreEqual("beta", lines[1]);
        }
        finally { File.Delete(testFile); }
    }

    [TestMethod]
    public void Endfile_numeric_channel_name()
    {
        // Channel name as integer-convertible string
        var testFile = Path.GetTempFileName();
        try
        {
            var s = $"""
                             output('W2','4','{testFile}')
                             W2 = 'line one'
                             endfile('4')
                     end
                     """;
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        }
        finally { File.Delete(testFile); }
    }
}