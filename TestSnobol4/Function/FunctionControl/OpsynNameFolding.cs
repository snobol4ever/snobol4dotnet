using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// OPSYN's new function name is a name like any other: while names fold, OPSYN('h', 'f') defines H, which the program calls as h or H,
/// as in SPITBOL. It defined h, unfolded, so H(2) was error 22 under -b. Found by the infinite_snobol4 generator's &amp;CASE probes.
/// Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class OpsynNameFolding
{
    [TestMethod]
    public void TEST_OpsynNameFolding_the_new_name_folds()
    {
        var program = @"        DEFINE('f(n)')                              :(fe)
f       f = n * 10                                  :(RETURN)
fe      OPSYN('h', 'f')
        OUTPUT = H(2)
        OUTPUT = APPLY('H', 3)
        OUTPUT = APPLY('h', 4)
        OPSYN('Size2', 'SIZE')
        OUTPUT = SIZE2('abc')
END";
        Assert.AreEqual(@"20
30
40
3", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
