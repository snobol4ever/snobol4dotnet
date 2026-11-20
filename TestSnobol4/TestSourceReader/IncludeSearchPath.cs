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

    // the sbl the oracle is reads SETL4PATH, not SNOLIB, for its include files (the harness sets it for sbl)
    [TestMethod]
    public void TEST_Include_is_found_through_SETL4PATH()
    {
        var dir = Path.Combine(Path.GetTempPath(), "setl4path_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var (oldLib, oldPath) = (Environment.GetEnvironmentVariable("SNOLIB"), Environment.GetEnvironmentVariable("SETL4PATH"));
        try
        {
            File.WriteAllText(Path.Combine(dir, "setl4path_probe.inc"), "        probe = 'from setl4path'\n");
            Environment.SetEnvironmentVariable("SNOLIB", null);
            Environment.SetEnvironmentVariable("SETL4PATH", ".:" + dir);
            var s = @"
-INCLUDE 'setl4path_probe.inc'
end";
            var build = SetupTests.SetupScript("-b", s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("from setl4path", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("probe")]).Data);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SNOLIB", oldLib);
            Environment.SetEnvironmentVariable("SETL4PATH", oldPath);
            Directory.Delete(dir, true);
        }
    }
}
