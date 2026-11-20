using Snobol4.Common;
using Test.TestLexer;

namespace Test.Compilation;

/// <summary>
/// -INCLUDE "name " finds name: a blank before the closing quote is dropped, as sbl -b reads it
/// (Budne's include and line).
/// </summary>
[DoNotParallelize]
[TestClass]
public class IncludeName
{
    [TestMethod]
    public void TEST_IncludeName_a_trailing_blank_is_dropped()
    {
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSnobol4");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "include_name_trailing_blank.sno");
        File.WriteAllText(file, "        R = R 'included'\n");
        try
        {
            var build = SetupTests.SetupScript("-b", @"
-INCLUDE ""include_name_trailing_blank.sno ""
        R = R ' after'
end");
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("included after", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
        }
        finally { File.Delete(file); }
    }

    // sbl keeps the name it opened (the blank dropped) and compares the name as written to the kept ones, so a name with a blank is
    // included again, once after each other such name, and a name without is skipped after it was included with one (sbl -b)
    [TestMethod]
    public void TEST_IncludeName_a_name_is_compared_as_written_to_the_names_as_opened()
    {
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSnobol4");
        Directory.CreateDirectory(directory);
        var file = Path.Combine(directory, "include_name_twice.sno");
        File.WriteAllText(file, "        R = R 'i'\n");
        try
        {
            foreach (var (names, count) in new[] { (new[] { "include_name_twice.sno ", "include_name_twice.sno" }, 1),
                         (new[] { "include_name_twice.sno ", "include_name_twice.sno " }, 2),
                         (new[] { "include_name_twice.sno", "include_name_twice.sno " }, 2),
                         (new[] { "include_name_twice.sno", "include_name_twice.sno", "include_name_twice.sno ", "include_name_twice.sno  " }, 3) })
            {
                var includes = string.Join("\n", names.Select(name => $"-INCLUDE \"{name}\""));
                var build = SetupTests.SetupScript("-b", includes + "\n        R = R ' after'\nend");
                Assert.AreEqual(0, build.ErrorCodeHistory.Count);
                Assert.AreEqual(new string('i', count) + " after", ((StringVar)build.Execute!.IdentifierTable["R"]).Data, string.Join("|", names));
            }
        }
        finally { File.Delete(file); }
    }
}
