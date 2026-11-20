using Test.TestLexer;

namespace Test.InputOutput;

[TestClass]
public class InputOutput
{
    // The files a program opens for input, made empty in the folder the program runs in: INPUT of a file that is not there leaves the
    // channel unopened, so a test of a channel in use read a file an earlier run had left behind and failed on a fresh clone
    private static void MakeFiles(params string[] names)
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSnobol4");
        Directory.CreateDirectory(dir);
        foreach (var name in names)
            File.WriteAllText(Path.Combine(dir, name), "");
    }

    // Hardcoded Windows file path replaced with temp file containing known numeric data.
    [TestMethod]
    public void TEST_Input_001()
    {
        var testFile = Path.GetTempFileName();
        try
        {
            // a = -input        → -123  (line 1 = 123)
            // b = input - input → -93   (line 2 = 15, line 3 = 108 → 15 - 108 = -93)
            // c = atan(input)   → 1.19… (line 4 = 2.5 → atan(2.5) ≈ 1.1902899496825317)
            File.WriteAllLines(testFile, new[] { "123", "15", "108", "2.5" });

            var s = $@"
        input('input','2','{testFile}')
	    a = -input
	    b = input - input
	    c = atan(input)
end";
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("-123", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
            Assert.AreEqual("-93", build.Execute!.IdentifierTable[build.FoldCase("b")].ToString());
            Assert.AreEqual("1.19028994968253", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
        }
        finally { File.Delete(testFile); }
    }


}