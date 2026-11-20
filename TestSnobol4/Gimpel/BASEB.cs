using Snobol4.Common;
using Test.TestLexer;

namespace Test.Gimpel;

[TestClass]
public class BASEB
{
    [TestMethod]
    public void BASEB0()
    {
        var s = @"

*  BASEB(N,B) will convert the integer N to its base B representation.
*
*  B may be any positive integer <=36.
*
	    DEFINE('BASEB(N,B)R,C')
	    BASEB_ALPHA  =  '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ'
						    :(BASEB_END)
BASEB	EQ(N,0)					:S(RETURN)
	    R  =  REMDR(N,B)
	    BASEB_ALPHA  TAB(*R)   LEN(1) . C	:F(ERROR)
        BASEB  =  C BASEB
        OUTPUT =  C BASEB;
	    N  =  N / B			:(BASEB)
BASEB_END
        OUTPUT = r1 = BASEB(761,8)
        OUTPUT = r2 = BASEB(761,16)
        OUTPUT = r3 = BASEB(761,2)

end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("1371", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("2F9", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
        Assert.AreEqual("1011111001", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r3")]).Data);
    }
}