using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// -INCLUDE finds a file through the SNOLIB directories when the current directory does not
/// hold it, as the SPITBOL manual documents. SNOLIB is process-wide, so these tests never run
/// beside another.
/// </summary>
[DoNotParallelize]
[TestClass]
public class IncludeSearchPath
{
    [TestMethod]
    public void TEST_Include_is_found_through_SNOLIB()
    {
        var dir = Path.Combine(Path.GetTempPath(), "snolib_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var old = Environment.GetEnvironmentVariable("SNOLIB");
        try
        {
            File.WriteAllText(Path.Combine(dir, "snolib_probe.inc"), "        probe = 'from snolib'\n");
            Environment.SetEnvironmentVariable("SNOLIB", "/nonexistent" + Path.PathSeparator + dir);
            var s = @"
-INCLUDE 'snolib_probe.inc'
end";
            var build = SetupTests.SetupScript("-b", s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("from snolib", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("probe")]).Data);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SNOLIB", old);
            Directory.Delete(dir, true);
        }
    }

    [TestMethod]
    public void TEST_Include_found_nowhere_is_error_285()
    {
        var old = Environment.GetEnvironmentVariable("SNOLIB");
        try
        {
            Environment.SetEnvironmentVariable("SNOLIB", "/nonexistent");
            var s = @"
-INCLUDE 'no_such_snolib_probe.inc'
end";
            var build = SetupTests.SetupScript("-b", s);
            CollectionAssert.Contains(build.ErrorCodeHistory, 285);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SNOLIB", old);
        }
    }
}
