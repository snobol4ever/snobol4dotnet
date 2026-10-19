using Snobol4.Common;
using Test.TestLexer;

namespace Test.Memory;

[TestClass]
public class Collect
{
    [TestMethod]
    public void TEST_Collect_002_returns_integer()
    {
        // COLLECT() with no arg returns available memory as integer (non-negative)
        var s = @"
        c = collect()
        ge(c, 0)   :s(ok)f(bad)
ok      result = 'ok'  :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Collect_003_after_alloc()
    {
        // COLLECT() can be called after allocating data — still succeeds
        var s = @"
        a = array(100)
        t = table(50)
        collect()
        result = 'ok'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Collect_001()
    {
        var s = @"

        COLLECT(1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }
    [TestMethod]
    public void TEST_Collect_004_twice()
    {
        // COLLECT can be called multiple times without error
        var s = @"
        collect()
        collect()
        result = 'ok'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Collect_005_returns_nonneg()
    {
        // COLLECT() result is always >= 0
        var s = @"
        c1 = collect()
        c2 = collect()
        ge(c1, 0)   :f(bad)
        ge(c2, 0)   :f(bad)
        result = 'ok'   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Collect_and_host_storage_figures()
    {
        // sbl -bf: COLLECT() and COLLECT(100) answer integers (free words), COLLECT(100000000) fails, a
        // non-integer argument is error 73; HOST(-1,0) is the -i increment (1048576 by default), HOST(-1,1) the
        // -d heap (134217728 by default), HOST(-1,6) the word size 8 and HOST(-1,-1) error 254 (spitbol_x64's
        // gcbuster prints these). snobol4dotnet's COLLECT answered null, and HOST(-1,n) was error 254
        var build = SetupTests.SetupScript("-bf", """
                 &ERRLIMIT = 9
                 R = DATATYPE(COLLECT()) ' ' DATATYPE(COLLECT(100))
                 R = R DIFFER(COLLECT(100000000)) ' big'
                 X = COLLECT('a')
                 R = R ' ' &ERRTYPE ' ' HOST(-1,0) ' ' HOST(-1,1) ' ' HOST(-1,6)
                 X = HOST(-1,-1)
                 R = R ' ' &ERRTYPE
         END
         """);
        Assert.AreEqual("INTEGER INTEGER 73 1048576 134217728 8 254", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}
