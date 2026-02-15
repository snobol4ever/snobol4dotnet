using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Operator;

[TestClass]
public class Star
{
    [TestMethod]
    public void TEST_STAR001()
    {
        var s = @"
        line1 = 'these two strings are almost alike.'
        line2 = ""the two strings aren't alike.""
        word = break(' .,') . w span(' ,.')
        string1 = line1 ' '                     :f(error)
        string2 = ' ' line2 ' '                 :f(error)
loop    string1 word =                          :f(out)
        string2 ' ' w any(' .,')                :f(loop)
        list = list w ', '                      :(loop)
out
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("two, strings, alike, ", build.Execute!.IdentifierTable[build.FoldCase("","list")].ToString());
    }

    [TestMethod]
    public void TEST_STAR002()
    {
        var s = @"
        line1 = 'these two strings are almost alike.'
        line2 = ""the two strings aren't alike.""
        word = break(' .,') . w span(' ,.')
        findw = ' ' *w any(' .,')
        string1 = line1 ' '                     :f(error)
        string2 = ' ' line2 ' '                 :f(error)
loop    string1 word =                          :f(out)
        string2 findw                           :f(loop)
        list = list w ', '                      :(loop)
out
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("two, strings, alike, ", build.Execute!.IdentifierTable[build.FoldCase("","list")].ToString());
    }

    [TestMethod]
    public void TEST_STAR003()
    {
        var s = @"
        &anchor = 1;
        line1 = 'two strings for testing'
        line2 = 'abcdefghijklmnopqrstuvwxyz'
        char = len(1) . ch
        findch = break(*ch)
        string1 = line1
        string2 = line2
loop    string1 char =                          :f(out)
        string2 findch                          :f(loop)
        list = list ch                          :(loop)
out     list
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("twostringsfortesting", build.Execute!.IdentifierTable[build.FoldCase("","list")].ToString());
    }

    [TestMethod]
    public void TEST_Pair()
    {
        var s = @"
        pair = (len(1) $ x *x) $ double
        'cook' pair
        list = list double
        'arron' pair
        list = list double
        'chickadee' pair
        list = list double
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("oorree", build.Execute!.IdentifierTable[build.FoldCase("","list")].ToString());
    }

    [TestMethod]
    public void TEST_Superbowl()
    {
        var s = @"
        bigp = (*p $ try *gt(size(try),size(big))) $ big fail
        str = 'IN 1964 NFL ATTENDANCE JUMPED TO 4,807,884; AN INCREASE OF 401,810'
        p = span('ABCDEFGHIJKLMNOPQRSTUVWXYZ,')
        big = ''
        str bigp
        result = 'The largest word is: ' big '. '
        p = span('0123456789,')
        big = ''
        str bigp
        result = result 'The largest number is: ' big '.'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("The largest word is: ATTENDANCE. The largest number is: 4,807,884.", build.Execute!.IdentifierTable[build.FoldCase("","result")].ToString());
    }

    [TestMethod]
    public void TEST_Recursive001()
    {
        var s = @"
        &anchor = 1;
        p = 'a' | ('b' *p)
        'bbba' p . match
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("bbba", build.Execute!.IdentifierTable[build.FoldCase("","match")].ToString());
    }

    [TestMethod]
    public void TEST_Recursive002()
    {
        var s = @"
        &anchor = 1;
        p = 'a' | ('b' **p)
        'bbba' p . match
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("bbba", build.Execute!.IdentifierTable[build.FoldCase("","match")].ToString());
    }

    [TestMethod]
    public void TEST_Error046()
    {
        var s = @"
        &anchor = 1;
        p = 'a' | ('b' *r)
        r = table(1)
        'bbba' p . match
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(46, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Convert_001()
    {
        var s = @"
        a = 'this is a test'
        b = eval(a)
        x = b
        y = datatype(b)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("this is a test", build.Execute!.IdentifierTable[build.FoldCase("", "a")].ToString());
        Assert.AreEqual("this is a test", build.Execute!.IdentifierTable[build.FoldCase("", "b")].ToString());
        Assert.AreEqual("this is a test", build.Execute!.IdentifierTable[build.FoldCase("", "x")].ToString());
        Assert.AreEqual("string", build.Execute!.IdentifierTable[build.FoldCase("","y")].ToString());

    }


    [TestMethod]
    public void TEST_Convert_002()
    {
        var s = @"
        a = convert('3.14 * 2.0', 'expression')
        b = datatype(a);
        c = eval(a)
        d = datatype(c)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("expression", build.Execute!.IdentifierTable[build.FoldCase("","b")].ToString());
        Assert.AreEqual("6.28", build.Execute!.IdentifierTable[build.FoldCase("","c")].ToString());
        Assert.AreEqual("real", build.Execute!.IdentifierTable[build.FoldCase("","d")].ToString());
    }

}