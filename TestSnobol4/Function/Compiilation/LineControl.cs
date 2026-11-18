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

    // a semicolon begins a statement, and sbl takes its file where it begins: an included file whose last line ends in a semicolon,
    // or in a comment after one, leaves the next statement after it carrying its name, as a last comment line does (sbl -b)
    [TestMethod]
    public void TEST_LineControl_an_include_ending_in_a_semicolon_names_the_next_statement()
    {
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSnobol4");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "line_semi.sno");
        foreach (var tail in new[] { " ;", " ;* c", " ;   " })
        {
            File.WriteAllText(file, "        R = R ' [' &FILE ']'" + tail + "\n");
            try
            {
                var build = SetupTests.SetupScript("-b", @"        R = R ' a ' &FILE
-INCLUDE ""line_semi.sno""
        R = R ' b ' &FILE
        R = R ' c ' &FILE
end");
                Assert.AreEqual(0, build.ErrorCodeHistory.Count, tail);
                Assert.AreEqual(" a Test.sno [line_semi.sno] b line_semi.sno c Test.sno", ((StringVar)build.Execute!.IdentifierTable["R"]).Data, tail);
            }
            finally { File.Delete(file); }
        }
    }

    // a semicolon at the end of a line of the including file carries no included name on: after an include that ends on a comment
    // line, the statement after it carries the included name and the statement after that the program's (sbl -b)
    [TestMethod]
    public void TEST_LineControl_a_semicolon_in_the_including_file_carries_no_included_name()
    {
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSnobol4");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "line_note.sno");
        File.WriteAllText(file, "        R = R ' [' &FILE ']'\n*\n");
        try
        {
            var build = SetupTests.SetupScript("-b", @"-INCLUDE ""line_note.sno""
        R = R ' b ' &FILE ;
        R = R ' c ' &FILE
end");
            Assert.AreEqual(" [line_note.sno] b line_note.sno c Test.sno", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
        }
        finally { File.Delete(file); }
    }
}
