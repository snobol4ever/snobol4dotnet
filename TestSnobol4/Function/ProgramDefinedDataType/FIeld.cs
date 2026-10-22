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
        // FIELD gives the field's NAME, as sbl does (DATATYPE is NAME, and it is IDENT to .IMAG)
        Assert.AreEqual("IMAG", ((NameVar)(build.Execute!.IdentifierTable[build.FoldCase("I")])).Pointer);
        Assert.AreEqual("REAL", ((NameVar)(build.Execute!.IdentifierTable[build.FoldCase("R")])).Pointer);
    }

    [TestMethod]
    public void TEST_Field_002()
    {
        // FIELD with 3-field datatype
        var s = @"
        DATA('POINT(X,Y,Z)')
        P = POINT(1, 2, 3)
        F1 = FIELD('POINT', 1)
        F2 = FIELD('POINT', 2)
        F3 = FIELD('POINT', 3)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("X", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("F1")]).Pointer);
        Assert.AreEqual("Y", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("F2")]).Pointer);
        Assert.AreEqual("Z", ((NameVar)build.Execute!.IdentifierTable[build.FoldCase("F3")]).Pointer);
    }

    [TestMethod]
    public void TEST_Field_003()
    {
        // FIELD out of range fails
        var s = @"
        DATA('PAIR(A,B)')
        FIELD('PAIR', 3)   :s(bad)
        result = 'ok'      :(end)
bad     result = 'fail'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }
}
