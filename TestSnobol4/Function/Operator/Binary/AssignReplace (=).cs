using Test.TestLexer;

namespace Test.Operator;

[TestClass]
public class Assign
{
    #region LeftVar: string

    [TestMethod]
    public void TEST_Assign_SS()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		'subject' = 'this is a test'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_IS()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		'subject' = 2
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_RS()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		'subject' = 3.14
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AS()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    'subject' = array(3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AiS()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    'subject' = array(3)[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TS()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    'subject' = table()
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TiS()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    'subject' = table()[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_PS()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    'subject' =  a | b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_NS()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    'subject' =  a
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region LeftVar: integer

    [TestMethod]
    public void TEST_Assign_SI()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		100 = 'this is a test'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_II()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		100 = 2
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_RI()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		100 = 3.14
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AI()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    100 = array(3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AiI()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    100 = array(3)[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TI()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    100 = table()
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TiI()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    100 = table()[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_PI()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    100 =  a | b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Assign_NI()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    100 =  a
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region  LeftVar: real

    [TestMethod]
    public void TEST_Assign_SR()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		3.14 = 'this is a test'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_IR()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		3.14 = 2
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_RR()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		3.14 = 3.14
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AR()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    3.14 = array(3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AiR()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    3.14 = array(3)[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TR()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    3.14 = table()
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TiR()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    3.14 = table()[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_PR()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    3.14 =  a | b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Assign_NR()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    3.14 =  a
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region LeftVar: array

    [TestMethod]
    public void TEST_Assign_SA()
    {
        // error 021 -- function called by name returned a value
        var s = @"		array(3) = 'this is a test'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_IA()
    {
        // error 021 -- function called by name returned a value
        var s = @"		array(3) = 2
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_RA()
    {
        // error 021 -- function called by name returned a value
        var s = @"		array(3) = array(3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AA()
    {
        // error 021 -- function called by name returned a value
        var s = @"    array(3) = array(3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AiA()
    {
        // error 021 -- function called by name returned a value
        var s = @"    array(3) = array(3)[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TA()
    {
        // error 021 -- function called by name returned a value
        var s = @"    array(3) = table()
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TiA()
    {
        // error 021 -- function called by name returned a value
        var s = @"    array(3) = table()[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_PA()
    {
        // error 021 -- function called by name returned a value
        var s = @"    array(3) =  a | b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Assign_NA()
    {
        // error 021 -- function called by name returned a value
        var s = @"    array(3) =  a
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region LeftVar: table

    [TestMethod]
    public void TEST_Assign_ST()
    {
        // error 021 -- function called by name returned a value
        var s = @"		table() = 'this is a test'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_IT()
    {
        // error 021 -- function called by name returned a value
        var s = @"		table() = 2
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    // error 021 -- function called by name returned a value
    public void TEST_Assign_RT()
    {
        var s = @"		table() = 3.14
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AT()
    {
        // error 021 -- function called by name returned a value
        var s = @"    table() = array(3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AiT()
    {
        // error 021 -- function called by name returned a value
        var s = @"    table() = array(3)[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TT()
    {
        // error 021 -- function called by name returned a value
        var s = @"    table() = table()
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TiT()
    {
        // error 021 -- function called by name returned a value
        var s = @"    table() = table()[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_PT()
    {
        // error 021 -- function called by name returned a value
        var s = @"    table() =  a | b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Assign_NT()
    {
        // error 021 -- function called by name returned a value
        var s = @"    table() =  a
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region LeftVar: pattern

    [TestMethod]
    public void TEST_Assign_SP()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		(a | b) = 'this is a test'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_IP()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		(a | b) = 2
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_RP()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"		(a | b) = 3.14
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AP()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    (a | b) = array(3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_AiP()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    (a | b) = array(3)[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TP()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    (a | b) = table()
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_TiP()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    (a | b) = table()[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Assign_PP()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    (a | b) =  a | b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Assign_NP()
    {
        // error 212 -- syntax error: value used where name is required
        var s = @"    (a | b) =  a
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(212, build.ErrorCodeHistory[0]);
    }

    #endregion

    #region LeftVar: indexed array

    [TestMethod]
    public void TEST_Assign_SAi()
    {
        var s = @"    a = array(3)[1] =  'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_IAi()
    {
        var s = @"		a = array(3)[1] = 2
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("2", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_RAi()
    {
        var s = @"		a = array(3)[1] = 3.14
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3.14", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_AAi()
    {
        var s = @"    a = array(3)[1] = 'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_AiAi()
    {
        var s = @"    a = array(3)[1] = array(3)[2] = 'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_TAi()
    {
        var s = @"   a = array(3)[1] = table()[1] = 'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_TiAi()
    {
        var s = @"
        a = array(3)[1] = table()[1] = 'subject'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_PAi()
    {
        var s = @"
        c = array(3)[1] =  a | b
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("pattern", build.Execute!.IdentifierTable[build.FoldCase("","c")].ToString());
    }


    [TestMethod]
    public void TEST_Assign_NAi()
    {
        var s = @"    a = array(3)[1] =  a = 'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    #endregion

    #region LeftVar: indexed table

    [TestMethod]
    public void TEST_Assign_STI()
    {
        var s = @"    a = table()[1] =  'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_ITI()
    {
        var s = @"		a = table()[1] = 2
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("2", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_RTI()
    {
        var s = @"		a = table()[1] = 3.14
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3.14", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_ATI()
    {
        var s = @"    a = table()[1] = 'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_AiTI()
    {
        var s = @"    a = table()[1] = array(3)[2] = 'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_TTI()
    {
        var s = @"   a = table()[1] = table()[1] = 'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_TiTI()
    {
        var s = @"    a = table()[1] = table()[1] = 'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_PTI()
    {
        var s = @"    a = table()[1] =  a | b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("pattern", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }


    [TestMethod]
    public void TEST_Assign_NTI()
    {
        var s = @"    a = table()[1] =  a = 'subject'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("subject", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    #endregion

    #region LeftVar Handler string

    [TestMethod]
    public void TEST_Assign_SIv()
    {
        var s = @"    a =  'subject'
        a = 'replacement'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("replacement", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_IIv()
    {
        var s = @"    a =  'subject'
        a =  100
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("100", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_RIv()
    {
        var s = @"    a =  'subject'
        a = 3.14
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3.14", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_AIv()
    {
        var s = @"    a =  'subject'
        a = array(3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("array", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_AiIv()
    {
        var s = @"
        a =  'subject'
        b = array(20)
        b[10] = 'replacement'
        a = b[10]
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("replacement", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_TIv()
    {
        var s = @"
        a = 'subject'
        a = table()
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("table", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_TiIv()
    {
        var s = @"    a =  'subject'
        b = table()
        b[1] = 'replacement'
        a = b[1]
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("replacement", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_PIv()
    {
        var s = @"    a =  'subject'
        a = 'a' | 'b'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("pattern", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    [TestMethod]
    public void TEST_Assign_NIv()
    {
        var s = @"    a =  'subject'
        b = 'replacement'
        a = b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("replacement", build.Execute!.IdentifierTable[build.FoldCase("","a")].ToString());
    }

    #endregion
}