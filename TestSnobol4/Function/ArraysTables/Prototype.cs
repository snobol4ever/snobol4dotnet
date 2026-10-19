using Snobol4.Common;
using Test.TestLexer;

namespace Test.ArraysTables;

[TestClass]
public class Prototype
{

    [TestMethod]
    public void TEST_Prototype_001()
    {
        var s = @"
        a = array('-5:10,3:5,20')
        b = prototype(a)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("-5:10,3:5,20", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }


    [TestMethod]
    public void TEST_Prototype_002()
    {
        var s = @"
        a = array(20)
        b = prototype(a)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // sbl -b: ARRAY(20) is a vector, whose prototype is the INTEGER 20
        Assert.AreEqual(20L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }


    [TestMethod]
    public void TEST_Prototype_003()
    {
        var s = @"
        a = table(20)
        b = prototype(a)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_Prototype_004()
    {
        var s = @"
        prototype(u)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(164, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Prototype_of_a_vector_is_an_integer()
    {
        // sbl -b: ARRAY(3) and ARRAY('3') are vectors, whose PROTOTYPE is the INTEGER 3 (so DIFFER(it,'3')
        // succeeds, as Budne's diag1 checks); ARRAY('1:3') and ARRAY('2,3') give STRING prototypes
        var build = SetupTests.SetupScript("-b", @"
        R = DATATYPE(PROTOTYPE(ARRAY(3))) ' ' DATATYPE(PROTOTYPE(ARRAY('3'))) ' '
+           DATATYPE(PROTOTYPE(ARRAY('1:3'))) ' ' DATATYPE(PROTOTYPE(ARRAY('2,3')))
        S = DIFFER(PROTOTYPE(ARRAY(3)),'3') 'differ'
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("INTEGER INTEGER STRING STRING", build.Execute!.IdentifierTable["R"].ToString());
        Assert.AreEqual("differ", build.Execute!.IdentifierTable["S"].ToString());
    }
}
