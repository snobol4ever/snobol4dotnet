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

    /// <summary>
    /// A real exponent beyond the integers is integral and even, as every double that large is, so a negative base takes it as SPITBOL
    /// does: -0.5 ** 1E300 is 0. and -1.0 ** 1E19 is 1., and only a result that overflows is error 266 (-2.0 ** 1E300, -0.5 ** -1E300).
    /// The console raised 266 for any exponent whose whole part passed the integers (found by the infinite_snobol4 generator at scope 8,
    /// -ATAN(1.5) ** 1E300). The operands are variables so that nothing folds as the program is compiled; sbl -b answered each line.
    /// </summary>
    [TestMethod]
    public void TEST_Power_a_huge_exponent_of_a_negative_base_is_even()
    {
        var build = SetupTests.SetupScript("-b", @"
        &ERRLIMIT = 10
        B = -0.5
        E = 1E300
        R1 = B ** E
        B = -1.0
        E = 1E19
        R2 = B ** E
        E = -1E300
        R3 = B ** E
        B = -2.0
        E = 1E300
        R4 = B ** E
        E4 = &ERRTYPE
        &ERRTYPE = 0
        B = -0.5
        E = -1E300
        R5 = B ** E
        E5 = &ERRTYPE
        R = R1 ' ' R2 ' ' R3 ' [' R4 '] ' E4 ' [' R5 '] ' E5
end");
        Assert.AreEqual("0. 1. 1. [] 266 [] 266", build.Execute!.IdentifierTable["R"].ToString());
    }
}
