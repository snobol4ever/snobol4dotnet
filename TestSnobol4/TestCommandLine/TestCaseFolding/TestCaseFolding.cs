using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestCaseFolding;

[TestClass]
public class TestCaseFolding
{

    [TestMethod]
    public void TEST_217_002()
    {
        var s = @"
test    a = b
TEST    a = b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(217, build.ErrorCodeHistory[0]);
        }
        else
        {
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        }
    }

    [TestMethod]
    public void TEST_217_004()
    {
        var s = @"
test    a = b
TEST    a = b
end
";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(217, build.ErrorCodeHistory[0]);
        }
        else
        {
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        }
    }

    [TestMethod]
    public void TEST_217_006()
    {
        var s = @"
test    a = b
TEST    a = b
end
";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(217, build.ErrorCodeHistory[0]);
        }
        else
        {
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        }
    }
}