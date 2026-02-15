using Test.TestLexer;

namespace Test.TestGoto;

[TestClass]
public class TestGoto
{
    [TestMethod]
    public void TEST_S_GOTO_DROPPED()
    {
        var s = @"     eq(0,1)      :s(n)
         A = 'dropped'  :(end)
y        A = 'success'  :(end)
n        A = 'failure'   (end)
u        A = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("dropped", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_S_GOTO_SUCCESS()
    {
        var s = @"     eq(1,1)      :s(y)
         A = 'dropped'  :(end)
y        A = 'success'  :(end)
n        A = 'failure'  :(end)
u        A = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_F_GOTO_DROPPED()
    {
        var s = @"     eq(1,1)      :f(n)
         A = 'dropped'  :(end)
y        A = 'success'  :(end)
n        A = 'failure'  :(end)
u        A = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("dropped", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_F_GOTO_FAILURE()
    {
        var s = @"     eq(0,1)      :f(n)
         A = 'dropped'  :(end)
y        A = 'success'  :(end)
n        A = 'failure'  :(end)
u        A = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_U_GOTO_FAILURE()
    {
        var s = @"     eq(0,1)      :(u)
         A = 'dropped'  :(end)
y        A = 'success'  :(end)
n        A = 'failure'  :(end)
u        A = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("unconditional", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_U_GOTO_SUCCESS()
    {
        var s = @"     eq(1,1)      :(u)
         A = 'dropped'  :(end)
y        A = 'success'  :(end)
n        A = 'failure'  :(end)
u        A = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("unconditional", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_SF_GOTO_SUCCESS()
    {
        var s = @"     eq(1,1)      :s(y)f(n)
         A = 'dropped'  :(end)
y        A = 'success'  :(end)
n        A = 'failure'  :(end)
u        A = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_SF_GOTO_FAILURE()
    {
        var s = @"     eq(0,1)      :s(y)f(n)
         A = 'dropped'  :(end)
y        A = 'success'  :(end)
n        A = 'failure'  :(end)
u        A = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }


    //***************************************************************************************


    [TestMethod]
    public void TEST_S_GOTO_DROPPED_DIRECT()
    {
        var s = @"
        y = code("" A = 'success'        :(end)"")
        n = code("" A = 'failure'        :(end)"")
        u = code("" A = 'unconditional'  :(end)"")
        eq(0,1)      :s<n>
        A = ""dropped""  :(end)
end";
        var directives = " -b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("dropped", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_S_GOTO_SUCCESS_DIRECT()
    {
        var s = @"
        y = code("" A = 'success'        :(end)"")
        n = code("" A = 'failure'        :(end)"")
        u = code("" A = 'unconditional'  :(end)"")
        eq(1,1)      :s<y>
        A = 'dropped'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_F_GOTO_DROPPED_DIRECT()
    {
        var s = @"
        y = code("" A = 'success'        :(end)"")
        n = code("" A = 'failure'        :(end)"")
        u = code("" A = 'unconditional'  :(end)"")
        eq(1,1)      :f<n>
        A = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s );
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("dropped", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_F_GOTO_FAILURE_DIRECT()
    {
        var s = @"
        y = code("" A = 'success'        :(end)"")
        n = code("" A = 'failure'        :(end)"")
        u = code("" A = 'unconditional'  :(end)"")
        eq(0,1)      :f<n>
        A = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_U_GOTO_FAILURE_DIRECT()
    {
        var s = @"
        y = code("" A = 'success'        :(end)"")
        n = code("" A = 'failure'        :(end)"")
        u = code("" A = 'unconditional'  :(end)"")
        eq(0,1)      :<u>
        A = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("unconditional", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_U_GOTO_SUCCESS_DIRECT()
    {
        var s = @"
        y = code("" A = 'success'        :(end)"")
        n = code("" A = 'failure'        :(end)"")
        u = code("" A = 'unconditional'  :(end)"")
        eq(1,1)      :<y>
        A = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_SF_GOTO_SUCCESS_DIRECT()
    {
        var s = @"
        y = code("" A = 'success'        :(end)"")
        n = code("" A = 'failure'        :(end)"")
        u = code("" A = 'unconditional'  :(end)"")
        eq(1,1)      :s<y>f<n>
        A = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_SF_GOTO_FAILURE_DIRECT()
    {
        var s = @"
        y = code("" A = 'success'        :(end)"")
        n = code("" A = 'failure'        :(end)"")
        u = code("" A = 'unconditional'  :(end)"")
        eq(0,1)      :s<y>f<n>
        A = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

}