using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

[TestClass]
public class Datatype
{

    [TestMethod]
    public void TEST_DataType_Expression()
    {
        var s = @"
        b = datatype(*c)
end";

        var directives = "-b -cs";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("EXPRESSION", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_DataType_UserDefinedData_keeps_its_spelling_without_case_folding()
    {
        // sbl -bf answers node and Pair; sbl -b answers NODE and PAIR
        var s = @"
        DATA('node(val,nxt)')
        DATA('Pair(a,b)')
        R = DATATYPE(node(1,2)) ' ' DATATYPE(Pair(1,2)) ' ' DATATYPE(1)
END";
        var build = SetupTests.SetupScript("-b -f", s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("node Pair INTEGER", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}
