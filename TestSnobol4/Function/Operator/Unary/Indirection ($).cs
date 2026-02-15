using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

[TestClass]
public class Indirection
{
    [TestMethod]
    public void TEST_Indirection_001()
    {
        var s = @"
        $'123' = 'abc'
        b = $'123'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","123")]).Data);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_002()
    {
        var s = @"
        v = '123'
        $v = 'abc'
        b = $v
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_003()
    {
        var s = @"
        v = 123
        $v = 'abc'
        b = $v
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_004()
    {
        var s = @"
        v = 123.45
        $v = 'abc'
        b = $v
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_005()
    {
        var s = @"
        t = table(1)
        t[1] = '123'
        $t[1] = 'abc'
        b = $'123'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_006()
    {
        var s = @"
        v = table(1)
        $v = 'abc'
        b = $v
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(239, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Indirection_007()
    {
        var s = @"
        v = array('10')
        $v = 'abc'
        b = $v
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(239, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Indirection_008()
    {
        var s = @"
        v = any('abc')
        $v = 'abc'
        b = $v
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(239, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Indirection_009()
    {
        var s = @"
        v = '123'
        $v = 'abc'
        b = $v
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_010()
    {
        var s = @"
        v = ''
        $v = 'abc'
        b = $v
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(239, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Indirection_011()
    {
        var s = @"
        word = 'run'
        $(word ':') = 99
        $(word ':') = $(word ':') + 1
        a = $(word ':')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(100, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_012()
    {
        var s = @"
        ''
        month = 'april'
        $month = 'cruel'
        a = month
        b = $month
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("april", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("cruel", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_013()
    {
        var s = @"
        month = 'april'
        $month = 'cruel'
        a = month
        b = $month
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("april", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("cruel", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_014()
    {
        var s = @"
        dog = 'bark'
        cat = 'meow'
        animal = 'cat'
        a = $animal
        animal = 'dog'
        b = $animal
        $dog = 'ruff'
        c = $animal
        d = $$animal
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("meow", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual("bark", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
        Assert.AreEqual("bark", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","c")]).Data);
        Assert.AreEqual("ruff", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","d")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_015()
    {
        var s = @"
        $'Alabama' = 'Montgomery'
        $'Alaska' = 'Juneau'
        $'Arizona' = 'Phoenix'
        $'Arkansas' = 'Little Rock'
        $'California' = 'Sacramento'
        $'Colorado' = 'Denver'
        $'Connecticut' = 'Hartford'
        $'Delaware' = 'Dover'
        $'Hawaii' = 'Honolulu'
        $'Florida' = 'Tallahassee'
        $'Georgia' = 'Atlanta'
        $'Idaho' = 'Boise'
        $'Illinois' = 'Springfield'
        $'Indiana' = 'Indianapolis'
        $'Iowa' = 'Des Moines'
        $'Kansas' = 'Topeka'
        $'Kentucky' = 'Frankfort'
        $'Louisiana' = 'Baton Rouge'
        $'Maine' = 'Augusta'
        $'Maryland' = 'Annapolis'
        $'Massachusetts' = 'Boston'
        leq(Alabama,'Montgomery')  :f(end)
        leq(Alaska,'Juneau')  :f(end)
        leq(Arizona,'Phoenix')  :f(end)
        leq(Arkansas,'Little Rock')  :f(end)
        leq(California,'Sacramento')  :f(end)
        leq(Colorado,'Denver')  :f(end)
        leq(Connecticut,'Hartford')  :f(end)
        leq(Delaware,'Dover')  :f(end)
        leq(Florida,'Tallahassee')  :f(end)
        leq(Georgia,'Atlanta')  :f(end)
        leq(Hawaii,'Honolulu')  :f(end)
        leq(Idaho,'Boise')  :f(end)
        leq(Illinois,'Springfield')  :f(end)
        leq(Indiana,'Indianapolis')  :f(end)
        leq(Iowa,'Des Moines')  :f(end)
        leq(Kansas,'Topeka')  :f(end)
        leq(Kentucky,'Frankfort')  :f(end)
        leq(Louisiana,'Baton Rouge')  :f(end)
        leq(Maine,'Augusta')  :f(end)
        leq(Maryland,'Annapolis')  :f(end)
        leq(Massachusetts,'Boston')  :f(end)
        d = 'success'

end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","d")]).Data);
    }

    [TestMethod]
    public void TEST_Indirection_016()
    {
        var s = @"
        $'~' = 'doublequote'
        $'$#@!*' = 53.1
        nm = dupl('ab cd', 1000)
        $nm = 'long'
        a = $'$#@!*' $'~' $nm
        b = size(nm)
        d = 'success'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("53.1doublequotelong", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","a")]).Data);
        Assert.AreEqual(5000, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("","b")]).Data);
    }
}