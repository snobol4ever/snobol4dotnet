using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// ** as SPITBOL's o_exp computes it: integer powers multiply out (overflow is error 17), a real power goes
/// by exp(y * ln(x)), and a negative base to a non-integral power is error 311.
/// </summary>
[TestClass]
public class PowerSpitbol
{
    [TestMethod]
    public void TEST_Power_as_spitbol_computes_it()
    {
        // sbl -b answers 0.179434868275613e-279 [] 17 [] 311 8. 18.5202591774521 (Math.Pow gives ...603e-279)
        var build = SetupTests.SetupScript("-b", @"
        &ERRLIMIT = 5
        A = 2.1229509447726124e-200 ** 1.4010207140730579
        N = 2
        B = N ** 64
        E1 = &ERRTYPE
        M = -8.0
        C = M ** 0.5
        E2 = &ERRTYPE
        R = A ' [' B '] ' E1 ' [' C '] ' E2 ' ' (2.0 ** 3) ' ' (7 ** 1.5)
end");
        Assert.AreEqual("0.179434868275613e-279 [] 17 [] 311 8. 18.5202591774521", build.Execute!.IdentifierTable["R"].ToString());
    }
}
