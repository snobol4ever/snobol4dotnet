using Test.TestLexer;
using Snobol4.Common;

namespace Test.ProgramDefinedDataType;

[TestClass]
public class Field
{
    [TestMethod]
    public void TEST_Field_001()
    {
        var s = @"
        DATA('COMPLEX(REAL,IMAG)')
        X = COMPLEX(3.2, -2.0)
        R = FIELD('COMPLEX',1)
        I = FIELD('COMPLEX',2)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("IMAG", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("I")])).Data);
        Assert.AreEqual("REAL", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("R")])).Data);
    }

}
