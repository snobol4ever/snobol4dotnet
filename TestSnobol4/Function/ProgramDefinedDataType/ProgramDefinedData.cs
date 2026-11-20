using Test.TestLexer;
using Snobol4.Common;

namespace Test.ProgramDefinedDataType;

[TestClass]
public class Data
{

    [TestMethod]
    public void TEST_Data_003()
    {
        var s = @"
        DATA('COMPLEX(REAL,IMAG)')
        X = COMPLEX(ANY('ABC'),SPAN('123'))
        I = IMAG(X)
        R = REAL(X)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("Snobol4.Common.SpanPattern", ((PatternVar)(build.Execute!.IdentifierTable[build.FoldCase("I")])).Data.ToString());
        Assert.AreEqual("Snobol4.Common.AnyPattern", ((PatternVar)(build.Execute!.IdentifierTable[build.FoldCase("R")])).Data.ToString());
    }


    [TestMethod]
    public void TEST_Data_005()
    {
        var s = @"
        DATA('PRO+DUCT(NAME,1PRICE,QUANTITY,MFG)')    :S(Y)F(N)
Y       ITEM1 = APPLY(.$'PRO+DUCT', 'CAPERS', 2.39, 48, 'BRINE BROTHERS')
        R1 = DATATYPE(ITEM1)
        R2 = APPLY(.$'1PRICE', ITEM1)
        R3 = FIELD(.$'PRO+DUCT',2)                         :(end)
N       R4 = 'FAILURE'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // Under case folding the type name is folded to upper case, as sbl -b answers
        Assert.AreEqual("PRO+DUCT", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("R1")])).Data);
        Assert.AreEqual(2.39, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("R2")])).Data);
        // FIELD gives the field's NAME, as sbl does
        Assert.AreEqual("1PRICE", ((NameVar)(build.Execute!.IdentifierTable[build.FoldCase("R3")])).Pointer);
        Assert.AreEqual("", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("R4")])).Data);
    }
}
