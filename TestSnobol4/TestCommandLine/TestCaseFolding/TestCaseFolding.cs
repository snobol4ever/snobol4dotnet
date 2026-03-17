using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestCaseFolding;

[TestClass]
public class TestCaseFolding
{

    [TestMethod]
    public void TEST_217_002()
    {
        var s = @"
test    a = b
TEST    a = b
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(217, build.ErrorCodeHistory[0]);
        }
        else
        {
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        }
    }

    [TestMethod]
    public void TEST_217_004()
    {
        var s = @"
test    a = b
TEST    a = b
end
";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(217, build.ErrorCodeHistory[0]);
        }
        else
        {
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        }
    }

    [TestMethod]
    public void TEST_217_006()
    {
        var s = @"
test    a = b
TEST    a = b
end
";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(217, build.ErrorCodeHistory[0]);
        }
        else
        {
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        }
    }

    [TestMethod]
    public void TEST_DataType_UserDefinedData()
    {
        var s = @"
     	data('PRODUCT(NAME,PRICE,QUANTITY,MFG)')
	    ITEM1 = PRODUCT('CAPERS', 2.39, 48, 'BRINE BROTHERS')
        b = datatype(ITEM1);
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // SPITBOL MINIMAL folds DATA type names to lowercase (flstg at sdat1 in sbl.min)
        Assert.AreEqual("product", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_Identifier()
    {
        var s = @"
        a = 'lower case'
        A = 'upper case'
end";
        var directives = "-b ";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        //Assert.AreEqual("lower case", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual("upper case", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("A")]).Data);
    }
}