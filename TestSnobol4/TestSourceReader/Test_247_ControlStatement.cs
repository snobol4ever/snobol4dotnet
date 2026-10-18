using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestSourceReader;

[TestClass]
public partial class TestSourceReader
{
    [TestMethod]
    public void TEST_247_001()
    {
        // SPITBOL ignores a control it does not know (the manual: "ignores unrecognized control
        // statements"); sbl -b runs this program with no error
        var s = @"
-WTF
        R = 'ran'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ran", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data);
    }

}
