using Snobol4.Common;
using Test.TestLexer;

namespace Test.Operator;

[TestClass]
public class Name
{
    [TestMethod]
    public void TEST_Name_001()
    {
        var s = @"
        dog = 'kharma'
        dog
        a = .dog
        datatype(a)
        b = $.dog
        c = .$dog
        
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("kharma", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("dog")]).Data);

        if (build.BuildOptions.CaseFolding)
        {
            Assert.AreEqual("DOG", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Pointer);
            Assert.AreEqual("KHARMA", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Pointer);

        }
        else
        {
            Assert.AreEqual("dog", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Pointer);
            Assert.AreEqual("kharma", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Pointer);

        }
        Assert.AreEqual("kharma", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }
}
