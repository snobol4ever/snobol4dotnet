using Snobol4.Common;
using Test.TestLexer;

namespace Test.ArraysTables;

[TestClass]
public class Item
{

    [TestMethod]
    public void TEST_Array_010()
    {
        var s = @"
        a = 'abc'
        b = item(a,3)
end
";
        var directives = "-b -cs";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(235, build.ErrorCodeHistory[0]);
    }

}