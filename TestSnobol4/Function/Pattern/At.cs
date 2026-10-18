using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

[TestClass]
public class At
{

    [TestMethod]
    public void TEST_At_001()
    {
        var s = @"
        &anchor = 0
        'valley' 'a' @at1 arb 'e' @at2  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual(2, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("at1")]).Data);
        Assert.AreEqual(5, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("at2")]).Data);
    }
    [TestMethod]

    public void TEST_At_002()
    {
        var s = @"
        &anchor = 0
        'DOUBT' @at1 'B'  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual(3, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("at1")]).Data);
    }

    [TestMethod]
    public void TEST_At_003()
    {
        var s = @"
        &anchor = 0
        'FIX' @at1 'B'  :s(y)f(n)
y       result = 'success'   :(end)
n       result = 'fail' 
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }
    [TestMethod]
    public void TEST_At_004()
    {
        // @var captures cursor position — after LEN(2) it should be 2
        var s = @"
        subject = 'abcdef'
        subject len(2) @pos   :f(bad)
        result = pos   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(2L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("pos")]).Data);
    }

    [TestMethod]
    public void TEST_At_005()
    {
        // @var at start of match = 0
        var s = @"
        subject = 'hello'
        &anchor = 1
        subject @pos 'hello'   :f(bad)
        result = pos   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("pos")]).Data);
    }

    [TestMethod]
    public void TEST_At_006()
    {
        // @var captures position after each character in a scan loop
        var s = @"
        subject = 'abcd'
        &anchor = 1
        subject len(3) @pos   :f(bad)
        result = pos   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(3L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("pos")]).Data);
    }

    [TestMethod]
    public void TEST_At_007()
    {
        // @var between two fixed strings captures the boundary position
        var s = @"
        subject = 'helloworld'
        &anchor = 1
        subject 'hello' @mid 'world'   :f(bad)
        result = mid   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(5L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("mid")]).Data);
    }

    [TestMethod]
    public void TEST_At_008()
    {
        // two @captures in one pattern, non-anchor mode
        var s = @"
        &anchor = 0
        'xyzABCdef' 'A' @p1 'BC' @p2   :f(bad)
        result = 'ok'   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual(4L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("p1")]).Data);
        Assert.AreEqual(6L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("p2")]).Data);
    }

    [TestMethod]
    public void TEST_At_deferred_and_element_targets()
    {
        // sbl -b: @*V with V = .P sets V (3), @*$('CUR' N) sets CUR2 (4), @*F() with F NRETURNing .Q sets
        // Q (3), @*A<2> and @A<1> set the elements (2, 4)
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('F()')                  :(EF)
F       F = .Q                         :(NRETURN)
EF      V = .P
        'ABCDE' 'BC' @*V
        N = 2
        'ABCDE' 'CD' @*$('CUR' N)
        'ABCDE' 'BC' @*F()
        A = ARRAY(3)
        'ABCDE' 'B' @*A<2>
        'ABCDE' 'D' @A<1>
        R = V ' ' CUR2 ' ' Q ' ' A<2> ' ' A<1>
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3 4 3 2 4", build.Execute!.IdentifierTable["R"].ToString());
    }
}
