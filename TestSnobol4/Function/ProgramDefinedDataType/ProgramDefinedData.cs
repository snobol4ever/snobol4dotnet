using Test.TestLexer;
using Snobol4.Common;

namespace Test.ProgramDefinedDataType;

[TestClass]
public class Data
{
    [TestMethod]
    public void TEST_Data_001()
    {
        var s = @"
        data('complex(real,imag)')
        x = complex(3.2, -2.0)
        i = imag(x)
        r = real(x)
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(-2.0, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("","i")])).Data);
        Assert.AreEqual(3.2, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("","r")])).Data);
    }

    [TestMethod]
    public void TEST_Data_002()
    {
        var s = @"
        DATA('COMPLEX(REAL,IMAG)')
        X = COMPLEX('AAA', 'BBB')
        I = IMAG(X)
        R = REAL(X)
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("BBB", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("","i")])).Data);
        Assert.AreEqual("AAA", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("","r")])).Data);
    }

    [TestMethod]
    public void TEST_Data_003()
    {
        var s = @"
        DATA('COMPLEX(REAL,IMAG)')
        X = COMPLEX(ANY('ABC'),SPAN('123'))
        I = IMAG(X)
        R = REAL(X)
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("Snobol4.Common.SpanPattern", ((PatternVar)(build.Execute!.IdentifierTable[build.FoldCase("","i")])).Data.ToString());
        Assert.AreEqual("Snobol4.Common.AnyPattern", ((PatternVar)(build.Execute!.IdentifierTable[build.FoldCase("","r")])).Data.ToString());
    }

    [TestMethod]
    public void TEST_Data_004()
    {
        var s = @"
        DATA('COMPLEX(REAL,IMAG)')
        X1 = COMPLEX(3.2, -2.0)
        I1 = IMAG(X1)
        R1 = REAL(X1)
        X2 = COMPLEX('AAA', 'BBB')
        IMAG(X2) = 45
        REAL(X2) = -5
        I2 = IMAG(X2)
        R2 = REAL(X2)
END";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(-2.0, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("","i1")])).Data);
        Assert.AreEqual(3.2, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("","r1")])).Data);
        Assert.AreEqual(45, ((IntegerVar)(build.Execute!.IdentifierTable[build.FoldCase("","i2")])).Data);
        Assert.AreEqual(-5, ((IntegerVar)(build.Execute!.IdentifierTable[build.FoldCase("","r2")])).Data);
    }


    [TestMethod]
    public void TEST_Data_005()
    {
        var s = @"
        DATA('PRO+DUCT(NAME,1PRICE,QUANTITY,MFG)')    :S(Y)F(N)
Y       ITEM1 = APPLY(.$'PRO+DUCT', 'CAPERS', 2.39, 48, 'BRINE BROTHERS')
        R1 = DATATYPE(ITEM1)
        R2 = APPLY(.$'1PRICE', ITEM1)
        R3 = FIELD(.$'PRO+DUCT',2)                         :(END)
N       R4 = 'FAILURE'
END
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("PRO+DUCT", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("","r1")])).Data);
        Assert.AreEqual(2.39, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("","r2")])).Data);
        Assert.AreEqual("1PRICE", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("","r3")])).Data);
        Assert.AreEqual("", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("","r4")])).Data);
    }
}
