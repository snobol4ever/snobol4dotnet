using Snobol4.Common;
using Test.TestLexer;

namespace Test.Compilation;

[TestClass]
public class Eval
{
    [TestMethod]
    public void TEST_Eval001()
    {
        var s = @"
        a = eval(17)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17, i.Data);
    }

    [TestMethod]
    public void TEST_Eval002()
    {
        var s = @"
        a = eval(17 * 2)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(34, i.Data);
    }

    [TestMethod]
    public void TEST_Eval003()
    {
        var s = @"
        a = eval('17')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17, i.Data);
    }

    [TestMethod]
    public void TEST_Eval004()
    {
        var s = @"
        a = eval('17' '34')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(1734, i.Data);
    }

    [TestMethod]
    public void TEST_Eval005()
    {
        var s = @"
        a = eval(17.34)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var r = (RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17.34, r.Data);
    }


    [TestMethod]
    public void TEST_Eval006()
    {
        var s = @"
        a = eval(1.0 / 3.0)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var r = (RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(1.0 / 3.0, r.Data);
    }

    [TestMethod]
    public void TEST_Eval007()
    {
        var s = @"
        a = eval('17.34')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var r = (RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17.34, r.Data);
    }

    [TestMethod]
    public void TEST_Eval008()
    {
        var s = @"
        a = eval('17' '.34')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var r = (RealVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(17.34, r.Data);
    }

    [TestMethod]
    public void TEST_Eval009()
    {
        var s = @"
        a = eval('17 + 34')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(51, i.Data);
    }

    [TestMethod]
    public void TEST_Eval010()
    {
        var s = @"
        n = 15
        a = eval('n * 2')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(30, i.Data);
    }

    [TestMethod]
    public void TEST_Eval011()
    {
        var s = @"
        a = eval('4 ' ' - 8')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(-4, i.Data);
    }

    [TestMethod]
    public void TEST_Eval012()
    {
        var s = @"
        a = eval('2 + eval(3)')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(5, i.Data);
    }

    [TestMethod]
    public void TEST_Eval013()
    {
        var s = @"
        N = 15
        a = eval('3 * N + 2')
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var i = (IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual(47, i.Data);
    }

    [TestMethod]
    public void TEST_Eval014()
    {
        var s = @"
        e = *('n squared is ' n ^ 2)
        n = 15
        a = eval(e)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        var v = (StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")];
        Assert.AreEqual("n squared is 225", v.Data);
    }

    [TestMethod]
    public void TEST_Eval_called_by_name_of_a_value_is_249()
    {
        // sbl -bf: EVAL called by name evaluates its expression by name, so an expression that gives a value is error 249, "expression
        // evaluated by name returned value", wherever a name is required: .EVAL('1 + 2'), .EVAL(5), .EVAL(*(1 + 2)), . EVAL(...) and
        // $ EVAL(...) in a pattern, @EVAL(...) and EVAL(...) = 4; an expression that is a variable gives its name (EVAL('X') = 'y'
        // assigns X). The console said 21 for each, and 212 for the assignment (the master's
        // eval_datatype_indirect_replace_branch_1, read against a fresh sbl run)
        var build = SetupTests.SetupScript("-bf", @"        &ERRLIMIT = 100
        SETEXIT('H')
        N = .EVAL('1 + 2')
        N = .EVAL(5)
        N = .EVAL(*(1 + 2))
        'abc' ? LEN(1) . EVAL('1 + 2')
        'abc' ? LEN(1) $ EVAL('1 + 2')
        'abc' ? @EVAL('1 + 2')
        EVAL('1 + 2') = 4
        X = 'x'
        EVAL('X') = 'y'
        S = S ' X=' X                       :(END)
H       S = S ' ' &ERRTYPE
        SETEXIT('H')                        :(CONTINUE)
END");
        Assert.AreEqual(" 249 249 249 249 249 249 249 X=y", ((StringVar)build.Execute!.IdentifierTable["S"]).Data);
    }
}
