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
            // After detach, READ is unassociated and keeps the last line it read (sbl -b).
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
            // sbl -b: the detached variable keeps the last line it read, so b is that line, not null
            Assert.AreEqual("This ebook is for the use of anyone anywhere in the United States and",
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
            // sbl -b: the detached variable keeps the last line it read, so b is that line, not null
            Assert.AreEqual("This ebook is for the use of anyone anywhere in the United States and",
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
        }
        finally { File.Delete(testFile); }
    }

    // Detach_003: DETACH ends the association of the one variable it names; another variable on the same output channel keeps it
    [TestMethod]
    public void Detach_003()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            // sbl -bf: records a1, b1, b2 -- A's second assignment writes nothing, B's still does
            var s = $@"
        output('A','1','{testFile}')
        output('B','1')
        A = 'a1'
        B = 'b1'
        detach('A')
        A = 'a2'
        B = 'b2'
        endfile('1')
end
";
            var build = SetupTests.SetupScript("-b", s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            CollectionAssert.AreEqual(new[] { "a1", "b1", "b2" }, File.ReadAllLines(testFile));
        }
        finally { File.Delete(testFile); }
    }

    // Detach_004: the same for input; the variable left on the channel goes on reading its lines
    [TestMethod]
    public void Detach_004()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            File.WriteAllLines(testFile, new[] { "one", "two", "three" });

            // sbl -bf: A reads one and keeps it once detached; B, still on channel 3, reads two
            var s = $@"
        input('A','3','{testFile}')
        input('B','3')
        x = A
        detach('A')
        y = A
        z = B
        endfile('3')
end
";
            var build = SetupTests.SetupScript("-b", s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("one", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data);
            Assert.AreEqual("one", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("y")]).Data);
            Assert.AreEqual("two", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("z")]).Data);
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
