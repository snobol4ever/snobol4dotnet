using Test.TestLexer;

namespace Test.Memory;

[TestClass]
public class Dump
{
    [TestMethod]
    public void TEST_Dump_001()
    {
        var s = @"

        I = 10;
        S = 'STRING'
        T = TABLE(10)
        T<1> = 20
        T<2> = 'string'

        DUMP(1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Dump_002()
    {
        var s = @"

        I = 10;
        S = 'STRING'
        T = TABLE(10)
        T<1> = 20
        T<2> = 'string'

        DUMP(2)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Dump_003()
    {
        var s = @"

        I = 10;
        S = 'STRING'
        T = TABLE(10)
        T<1> = 20
        T<2> = 'string'

        DUMP(3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }
}