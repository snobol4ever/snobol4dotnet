using Test.TestLexer;

namespace Test.TestGoto;

[TestClass]
public class TestGoto
{
    [TestMethod]
    public void TEST_S_GOTO_DROPPED()
    {
        var s = @"     eq(0,1)      :s(n)
         a = 'dropped'  :(end)
y        a = 'success'  :(end)
n        a = 'failure'   (end)
u        a = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("dropped", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_S_GOTO_SUCCESS()
    {
        var s = @"     eq(1,1)      :s(y)
         a = 'dropped'  :(end)
y        a = 'success'  :(end)
n        a = 'failure'  :(end)
u        a = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_F_GOTO_DROPPED()
    {
        var s = @"     eq(1,1)      :f(n)
         a = 'dropped'  :(end)
y        a = 'success'  :(end)
n        a = 'failure'  :(end)
u        a = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("dropped", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_F_GOTO_FAILURE()
    {
        var s = @"     eq(0,1)      :f(n)
         a = 'dropped'  :(end)
y        a = 'success'  :(end)
n        a = 'failure'  :(end)
u        a = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_U_GOTO_FAILURE()
    {
        var s = @"     eq(0,1)      :(u)
         a = 'dropped'  :(end)
y        a = 'success'  :(end)
n        a = 'failure'  :(end)
u        a = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("unconditional", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_U_GOTO_SUCCESS()
    {
        var s = @"     eq(1,1)      :(u)
         a = 'dropped'  :(end)
y        a = 'success'  :(end)
n        a = 'failure'  :(end)
u        a = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("unconditional", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_SF_GOTO_SUCCESS()
    {
        var s = @"     eq(1,1)      :s(y)f(n)
         a = 'dropped'  :(end)
y        a = 'success'  :(end)
n        a = 'failure'  :(end)
u        a = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_SF_GOTO_FAILURE()
    {
        var s = @"     eq(0,1)      :s(y)f(n)
         a = 'dropped'  :(end)
y        a = 'success'  :(end)
n        a = 'failure'  :(end)
u        a = 'unconditional'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }


    //***************************************************************************************


    [TestMethod]
    public void TEST_S_GOTO_DROPPED_DIRECT()
    {
        var s = @"
        y = code("" a = 'success'        :(end)"")
        n = code("" a = 'failure'        :(end)"")
        u = code("" a = 'unconditional'  :(end)"")
        eq(0,1)      :s<n>
        a = ""dropped""  :(end)
end";
        var directives = " -b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("dropped", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_S_GOTO_SUCCESS_DIRECT()
    {
        var s = @"
        y = code("" a = 'success'        :(end)"")
        n = code("" a = 'failure'        :(end)"")
        u = code("" a = 'unconditional'  :(end)"")
        eq(1,1)      :s<y>
        a = 'dropped'  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_F_GOTO_DROPPED_DIRECT()
    {
        var s = @"
        y = code("" a = 'success'        :(end)"")
        n = code("" a = 'failure'        :(end)"")
        u = code("" a = 'unconditional'  :(end)"")
        eq(1,1)      :f<n>
        a = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s );
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("dropped", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_F_GOTO_FAILURE_DIRECT()
    {
        var s = @"
        y = code("" a = 'success'        :(end)"")
        n = code("" a = 'failure'        :(end)"")
        u = code("" a = 'unconditional'  :(end)"")
        eq(0,1)      :f<n>
        a = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_U_GOTO_FAILURE_DIRECT()
    {
        var s = @"
        y = code("" a = 'success'        :(end)"")
        n = code("" a = 'failure'        :(end)"")
        u = code("" a = 'unconditional'  :(end)"")
        eq(0,1)      :<u>
        a = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("unconditional", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_U_GOTO_SUCCESS_DIRECT()
    {
        var s = @"
        y = code("" a = 'success'        :(end)"")
        n = code("" a = 'failure'        :(end)"")
        u = code("" a = 'unconditional'  :(end)"")
        eq(1,1)      :<y>
        a = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_SF_GOTO_SUCCESS_DIRECT()
    {
        var s = @"
        y = code("" a = 'success'        :(end)"")
        n = code("" a = 'failure'        :(end)"")
        u = code("" a = 'unconditional'  :(end)"")
        eq(1,1)      :s<y>f<n>
        a = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_SF_GOTO_FAILURE_DIRECT()
    {
        var s = @"
        y = code("" a = 'success'        :(end)"")
        n = code("" a = 'failure'        :(end)"")
        u = code("" a = 'unconditional'  :(end)"")
        eq(0,1)      :s<y>f<n>
        a = ""dropped""  :(end)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

}