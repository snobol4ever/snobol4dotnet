using Snobol4.Common;
using Test.TestLexer;

namespace Test.Numeric;

/// <summary>
/// A real's string form is SPITBOL's: 15 significant digits, trailing zeros dropped, the point always
/// present; fixed notation from 0.1 up to below 1e15, otherwise 0.ddd and a signed exponent. The
/// expectation is sbl -b's output for the same values.
/// </summary>
[TestClass]
public class RealFormat
{
    [TestMethod]
    public void TEST_RealFormat_edges_of_both_forms()
    {
        var build = SetupTests.SetupScript("-b", @"
        R = R ' ' 0.0015
        R = R ' ' 0.0999
        R = R ' ' 0.1
        R = R ' ' 999999999999999.0
        R = R ' ' 1.0E14
        R = R ' ' 12345678901234567.0
        R = R ' ' -1.0E-20
        R = R ' ' 1.0E100
        R = R ' ' 2.5E-300
        R = R ' ' 0.0
        R = R ' ' 3.0
        R = R ' ' 1.25
        R = R ' ' 7.0E-1
        R = R ' ' 123.456
        R = R ' ' 9.99999999999999E14
        R = R ' ' 0.999999999999999
        R = R ' ' 0.9999999999999999
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(" 0.15e-2 0.999e-1 0.1 999999999999999. 100000000000000. 0.123456789012346e+17 -0.1e-19 0.1e+101 0.25e-299 0. 3. 1.25 0.7 123.456 999999999999999. 0.999999999999999 1.", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }

    [TestMethod]
    public void TEST_RealFormat_the_fifteenth_digit_is_spitbols()
    {
        // 171725223 / 414359639 prints ...804 under sbl -b, where a correctly rounded 15 digits would end
        // in 803: SPITBOL adds 0.5 * 10**-15 to the scaled value and truncates, and that is what is copied
        var build = SetupTests.SetupScript("-b", @"
        R = (171725223 * 1.0) / 414359639
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("0.414435207575804", build.Execute!.IdentifierTable["R"].ToString());
    }

    [TestMethod]
    public void TEST_RealFormat_a_real_is_read_as_spitbol_reads_it()
    {
        // SPITBOL reads these with its own arithmetic, not a correctly rounded parse, and so prints
        // ...064e+32 for 9.478653606090631e+31; the D exponent and surrounding blanks are SPITBOL's too
        var build = SetupTests.SetupScript("-b", @"
        R = 9.478653606090631e+31 ' ' ('1.8049542451833431e-278' + 0) ' ' ('  -2.5D2 ' + 0)
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("0.947865360609064e+32 0.180495424518334e-277 -250.", build.Execute!.IdentifierTable["R"].ToString());
    }
}
