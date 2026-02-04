using Snobol4.Common;
using Test.TestLexer;

namespace TestGimpel;

[TestClass]
public class UPLO
{
    [TestMethod]
    public void UPLO0()
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
}