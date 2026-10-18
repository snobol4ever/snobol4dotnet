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
}
