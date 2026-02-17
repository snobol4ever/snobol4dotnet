using Snobol4.Common;
using Test.TestLexer;

namespace Test.Gimpel;

[TestClass]
public class ROMAN
{

    [TestMethod]
    public void ROMAN0()
    {
        var s = @"
* ROMAN.inc - ROMAN(N) will return the roman numeral representation
*             of the integer N.  0 < N < 4000.
*
	    DEFINE('ROMAN(N)T')			:(ROMAN_END)
ROMAN	OUTPUT = N
        N   RPOS(1)  LEN(1) . T  =		:F(RETURN)
	    '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+	    T   BREAK(',') . T			:F(FRETURN)
	    ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+						:S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('1776')
        R2 = ROMAN('9')
        R3 = ROMAN('45')
        R4 = ROMAN('2026')
end

";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("MDCCLXXVI", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("IX", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
        Assert.AreEqual("XLV", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data);
        Assert.AreEqual("MMXXVI", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R4")]).Data);
    }

    [TestMethod]
    public void ROMAN2()
    {
        var s = @"
* ROMAN.inc - ROMAN(N) will return the roman numeral representation
*	      of the integer N.  0 < N < 4000.
*
	    DEFINE('ROMAN(N)T')			:(ROMAN_END)
ROMAN	N   RPOS(1)  LEN(1) . T  =		:F(RETURN)
	    '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+	    T   BREAK(',') . T			:F(FRETURN)
	    ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+						:S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('A57')   :F(N)
        R2 = 'Succeed'     :(end)
N       R2 = 'Fail'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("Fail", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

}