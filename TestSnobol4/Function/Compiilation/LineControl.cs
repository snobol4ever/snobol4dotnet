using Snobol4.Common;
using Test.TestLexer;

namespace Test.Compilation;

/// <summary>
/// &amp;FILE and &amp;LINE through -INCLUDE and -LINE, as sbl -b reads them: an included file counts
/// its own lines from 1; when it ends on a comment line, the next statement after it carries its name;
/// -LINE n "file" renumbers the lines after it, and -LINE n keeps the file. (Budne's include and line.)
/// The test host names the program Test.sno.
/// </summary>
[DoNotParallelize]
[TestClass]
public class LineControl
{
    [TestMethod]
    public void TEST_LineControl_include_and_line_numbering()
    {
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSnobol4");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "line_inc.sno");
        File.WriteAllText(file, "        R = R ' [' &FILE ':' &LINE ']'\n*\n");
        try
        {
            var build = SetupTests.SetupScript("-b", @"        R = R ' a ' &FILE ':' &LINE
-INCLUDE ""line_inc.sno""
        R = R ' b ' &FILE ':' &LINE
        R = R ' c ' &FILE ':' &LINE
-LINE 1234 ""foo""
        R = R ' d ' &FILE ':' &LINE
-LINE 50
        R = R ' e ' &FILE ':' &LINE
end");
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(" a Test.sno:1 [line_inc.sno:1] b line_inc.sno:3 c Test.sno:4 d foo:1234 e foo:50",
                ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
        }
        finally { File.Delete(file); }
    }
}
