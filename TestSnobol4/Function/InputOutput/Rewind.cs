using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

// Tests require external data files from dev machine (hardcoded Windows paths).
[TestClass, Ignore]
public class Rewind
{

    //172 REWIND argument is not a suitable name
    //173 REWIND argument is null
    //174 REWIND file does not exist
    //175 REWIND file does not permit rewind
    //176 REWIND caused non-recoverable error

    [TestMethod]
    public void Rewind_001()
    {

        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\Frankenstein.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Frankenstein.txt";

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
        Assert.AreEqual(((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data, ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);

    }


    [TestMethod]
    public void Rewind_002()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\Frankenstein2.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Frankenstein2.txt";

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
        Assert.AreEqual(((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data, "I am far north of London, and as I walk in the streets of");
        if (SetupTests.IsLinux)
            Assert.AreEqual("eets of", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        else
            Assert.AreEqual("ets of", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

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