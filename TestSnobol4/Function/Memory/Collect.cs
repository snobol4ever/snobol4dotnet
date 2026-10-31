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

    [TestMethod]
    public void TEST_Collect_answers_the_same_storage_twice_with_nothing_between()
    {
        // COLLECT() read GC.GetTotalMemory(true), which counts the part of a thread's allocation context not yet used, so two calls with
        // nothing between them differed by 520 or 1024 words now and then and gimpel's TIMEGC printed a negative count of freed storage
        // about a run in three. Run on its own the engine answers the same figure to a handful of words (the command line reads 40 of 40
        // TIMEGC runs alike); this test host has threads of its own that move the process's heap now and then, so after the first
        // hundred calls (the interpreter warming up) the last sixty may swing by more than 128 words a few times: 2, 5, 8 and 6 in four
        // runs on the measure now used, against 27, 36, 32 and 29 on GC.GetTotalMemory, so the bar is 15
        var build = SetupTests.SetupScript("-bf", """
                 I = 0
                 SWINGS = 0
                 PREV = COLLECT()
         LOOP    I = I + 1
                 NOW = COLLECT()
                 D = NOW - PREV
                 PREV = NOW
                 LE(I,100)                  :S(NEXT)
                 DIFFS = DIFFS ' ' D
                 GT(D,128)                  :S(SWING)
                 LT(D,-128)                 :S(SWING)
         NEXT    LT(I,160)                  :S(LOOP)F(DONE)
         SWING   SWINGS = SWINGS + 1        :(NEXT)
         DONE
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var swings = ((IntegerVar)build.Execute!.IdentifierTable["SWINGS"]).Data;
        Assert.IsTrue(swings <= 15, swings + " of 60 changes of COLLECT() exceed 128 words:" + ((StringVar)build.Execute!.IdentifierTable["DIFFS"]).Data);
    }

    [TestMethod]
    public void TEST_Collect_sees_the_storage_of_a_released_array_as_freed()
    {
        // the figure that stopped jittering is still the storage the program holds: releasing a 3,000 element array is at least
        // 2,000 words more free at the next COLLECT(), once the interpreter has warmed up
        var build = SetupTests.SetupScript("-bf", """
                 I = 0
         WARM    I = I + 1
                 COLLECT()
                 LT(I,100)                  :S(WARM)
                 A = ARRAY(3000)
                 HELD = COLLECT()
                 A =
                 FREED = COLLECT() - HELD
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var freed = ((IntegerVar)build.Execute!.IdentifierTable["FREED"]).Data;
        Assert.IsTrue(freed >= 2000, "words freed: " + freed);
    }
}
