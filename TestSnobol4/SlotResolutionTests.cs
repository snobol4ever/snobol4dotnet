using Snobol4.Common;
using System.Text;
using Test.TestLexer;

namespace Test.Phase2;

/// <summary>
/// Verifies that the pre-resolution pass (Phase 2) correctly populates
/// VariableSlots, FunctionSlots, and ConstantPool after Parse().
/// These tables are the foundation for the threaded execution engine.
/// </summary>
[TestClass]
public class SlotResolutionTests
{
    [TestMethod]
    public void Roman_VariableSlots_ContainsExpectedIdentifiers()
    {
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('ROMAN(N)T')                 :(ROMAN_END)
ROMAN   N   RPOS(1)  LEN(1) . T  =         :F(RETURN)
        '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+       T   BREAK(',') . T                  :F(FRETURN)
        ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+                                           :S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('1776')
        R4 = ROMAN('2026')
end");

        // Variable slots should include N, T, ROMAN, R1, R4
        var symbols = build.VariableSlots.Select(s => s.Symbol).ToHashSet();
        Assert.IsTrue(symbols.Contains("N"), "Expected slot for N");
        Assert.IsTrue(symbols.Contains("T"), "Expected slot for T");
        Assert.IsTrue(symbols.Contains("ROMAN"), "Expected slot for ROMAN");
        Assert.IsTrue(symbols.Contains("R1"), "Expected slot for R1");
        Assert.IsTrue(symbols.Contains("R4"), "Expected slot for R4");
    }

    [TestMethod]
    public void Roman_FunctionSlots_ContainsExpectedFunctions()
    {
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('ROMAN(N)T')                 :(ROMAN_END)
ROMAN   N   RPOS(1)  LEN(1) . T  =         :F(RETURN)
        '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+       T   BREAK(',') . T                  :F(FRETURN)
        ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+                                           :S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('1776')
end");

        var funcNames = build.FunctionSlots.Select(s => s.Symbol).ToHashSet();
        Assert.IsTrue(funcNames.Contains("DEFINE"), "Expected slot for DEFINE");
        Assert.IsTrue(funcNames.Contains("RPOS"),   "Expected slot for RPOS");
        Assert.IsTrue(funcNames.Contains("LEN"),    "Expected slot for LEN");
        Assert.IsTrue(funcNames.Contains("BREAK"),  "Expected slot for BREAK");
        Assert.IsTrue(funcNames.Contains("REPLACE"),"Expected slot for REPLACE");
        Assert.IsTrue(funcNames.Contains("ROMAN"),  "Expected slot for ROMAN (user-defined)");
    }

    [TestMethod]
    public void Roman_ConstantPool_ContainsLiterals()
    {
        var build = SetupTests.SetupScript("-b", @"
        R1 = ROMAN('1776')
        N = 42
        X = 3.14
end");

        var strings = build.Constants.Pool
            .OfType<StringVar>().Select(v => v.Data).ToHashSet();
        var integers = build.Constants.Pool
            .OfType<IntegerVar>().Select(v => v.Data).ToHashSet();

        Assert.IsTrue(strings.Contains("1776"), "Expected constant '1776'");
        Assert.IsTrue(integers.Contains(42L),   "Expected constant 42");
    }

    [TestMethod]
    public void SlotIndices_AreContiguous_StartingAtZero()
    {
        var build = SetupTests.SetupScript("-b", @"
        A = B + C
        D = E * F
end");

        for (var i = 0; i < build.VariableSlots.Count; i++)
            Assert.AreEqual(i, build.VariableSlots[i].SlotIndex,
                $"Variable slot {i} has wrong SlotIndex");

        for (var i = 0; i < build.FunctionSlots.Count; i++)
            Assert.AreEqual(i, build.FunctionSlots[i].SlotIndex,
                $"Function slot {i} has wrong SlotIndex");
    }

    [TestMethod]
    public void DuplicateIdentifiers_ProduceSingleSlot()
    {
        var build = SetupTests.SetupScript("-b", @"
        N = N + 1
        N = N + 1
        N = N + 1
end");

        var nSlots = build.VariableSlots.Count(s => s.Symbol == "N");
        Assert.AreEqual(1, nSlots, "N should appear exactly once in VariableSlots");
    }

    [TestMethod]
    public void AllExistingTests_StillPass_WithSlotResolution()
    {
        // Regression: slot resolution must not affect correctness of execution
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('ROMAN(N)T')                 :(ROMAN_END)
ROMAN   N   RPOS(1)  LEN(1) . T  =         :F(RETURN)
        '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+       T   BREAK(',') . T                  :F(FRETURN)
        ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+                                           :S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('1776')
        R2 = ROMAN('2026')
end");

        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("MDCCLXXVI", ((StringVar)build.Execute!.IdentifierTable["R1"]).Data);
        Assert.AreEqual("MMXXVI",    ((StringVar)build.Execute!.IdentifierTable["R2"]).Data);
    }
}
