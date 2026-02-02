using Snobol4.Common;
using Test.TestLexer;

namespace Gimpel;

[TestClass]
public class Gimpel
{
    [TestMethod]
    public void UPLO()
    {
        var s = @"

* UPLO.inc - UPLO(S) will return its argument with upper case letters
*	     converted to lower case, and vice versa.  Non-alphabetic
*	     characters are ignored.
*
	    DEFINE('UPLO(S)')
	    UP_LO  =  'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz'
	    LO_UP  =  'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ'
						:(UPLO_END)
UPLO	UPLO   =  REPLACE(S, UP_LO, LO_UP)	:(RETURN)
UPLO_END
        R = UPLO('Hello, World!')
END

";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("hELLO, wORLD!", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }

 //   [TestMethod]
    public void ROMAN()
    {
        var s = @"
* ROMAN.inc - ROMAN(N) will return the roman numeral representation
*	      of the integer N.  0 < N < 4000.
*
    	DEFINE('ROMAN(N)T')			:(ROMAN_END)
ROMAN	N   RPOS(1)  LEN(1) . T  =		:F(RETURN)
    	'0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+   	T   BREAK(',') . T			:F(FRETURN)
	    ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+						:S(RETURN)F(FRETURN)
ROMAN_END

        R = ROMAN(1999)
END


";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}