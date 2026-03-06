using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

[TestClass]
public class Detach
{
    // "detach argument is not appropriate name" /* 87 */,

    // Detach_001: read 3 lines, detach, read again → empty string
    [TestMethod]
    public void Detach_001()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            // After 3 reads, 'a' holds the 3rd line.
            // After detach, READ is unassociated so reading it gives empty string.
            File.WriteAllLines(testFile, new[]
            {
                "Line 1: intro",
                "Line 2: middle",
                "This ebook is for the use of anyone anywhere in the United States and",
            });

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
            Assert.AreEqual("This ebook is for the use of anyone anywhere in the United States and",
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
            Assert.AreEqual("",
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        }
        finally { File.Delete(testFile); }
    }

    // Detach_002: detach, re-attach stdin, read 3 lines, detach again
    [TestMethod]
    public void Detach_002()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(testFile, new[]
            {
                "Line 1: intro",
                "Line 2: middle",
                "This ebook is for the use of anyone anywhere in the United States and",
            });

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
            Assert.AreEqual("This ebook is for the use of anyone anywhere in the United States and",
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
            Assert.AreEqual("",
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        }
        finally { File.Delete(testFile); }
    }

    // Detach_087: pattern arg as channel name → error 87
    [TestMethod]
    public void Detach_087()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(testFile, new[] { "line 1", "line 2" });

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
        finally { File.Delete(testFile); }
    }
}
