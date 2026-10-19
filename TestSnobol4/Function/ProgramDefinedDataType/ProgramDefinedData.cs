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
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(-2.0, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("i")])).Data);
        Assert.AreEqual(3.2, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("r")])).Data);
    }

    [TestMethod]
    public void TEST_Data_002()
    {
        var s = @"
        DATA('COMPLEX(REAL,IMAG)')
        X = COMPLEX('AAA', 'BBB')
        I = IMAG(X)
        R = REAL(X)
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("BBB", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("I")])).Data);
        Assert.AreEqual("AAA", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("R")])).Data);
    }

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
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(-2.0, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("I1")])).Data);
        Assert.AreEqual(3.2, ((RealVar)(build.Execute!.IdentifierTable[build.FoldCase("R1")])).Data);
        Assert.AreEqual(45, ((IntegerVar)(build.Execute!.IdentifierTable[build.FoldCase("I2")])).Data);
        Assert.AreEqual(-5, ((IntegerVar)(build.Execute!.IdentifierTable[build.FoldCase("R2")])).Data);
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
        Assert.AreEqual("1PRICE", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("R3")])).Data);
        Assert.AreEqual("", ((StringVar)(build.Execute!.IdentifierTable[build.FoldCase("R4")])).Data);
    }

    [TestMethod]
    public void TEST_Data_a_system_function_name_is_refused()
    {
        // sbl -b: DATA('ITEM(A,B)') and DATA('NODE(SIZE,B)') are error 248, ITEM and SIZE being system
        // functions (Knuth's topological sort, ported, defines ITEM)
        var type = SetupTests.SetupScript("-b", @"
        DATA('ITEM(A,B)')
end");
        Assert.AreEqual(248, type.ErrorCodeHistory[0]);

        var field = SetupTests.SetupScript("-b", @"
        DATA('NODE(SIZE,B)')
end");
        Assert.AreEqual(248, field.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Data_a_program_defined_type_may_be_redefined()
    {
        // sbl -b: a second DATA('NODE(B)') replaces NODE(A), so B(NODE(5)) is 5
        var build = SetupTests.SetupScript("-b", @"
        DATA('NODE(A)')
        DATA('NODE(B)')
        R = B(NODE(5))
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("5", build.Execute!.IdentifierTable["R"].ToString());
    }

    [TestMethod]
    public void TEST_ProgramDefinedData_opsyn_synonyms()
    {
        // sbl -b answers "3 4 PT 9 9": a constructor or field function OPSYN'd to a new name (MK for PT, GX for
        // XC, GX2 for GX) still builds that type and reads and sets that field (testpgms' test6 queues through
        // QLINK, OPSYN'd to COUNT). snobol4dotnet looked the called name up as the type or field name and crashed
        var build = SetupTests.SetupScript("-b", """
                 DATA('PT(XC,YC)')
                 OPSYN('MK','PT')
                 OPSYN('GX','XC')
                 P = MK(3,4)
                 R = GX(P) ' ' YC(P) ' ' DATATYPE(P)
                 GX(P) = 9
                 OPSYN('GX2','GX')
                 R = R ' ' XC(P) ' ' GX2(P)
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("3 4 PT 9 9", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data);
    }
}
