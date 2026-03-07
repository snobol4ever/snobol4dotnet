using Snobol4.Common;
using Test.TestLexer;

namespace Test.Phase3;

/// <summary>
/// Verifies that ThreadedCodeCompiler produces a well-formed Instruction[]
/// for representative SNOBOL4 programs.  These tests inspect structure only —
/// execution correctness is Phase 4.
/// </summary>
[TestClass]
public class ThreadedCompilerTests
{
    private static Instruction[] Compile(string script)
    {
        var build = SetupTests.SetupScript("-b", script, compileOnly: true);
        return new ThreadedCodeCompiler(build).Compile();
    }

    // -----------------------------------------------------------------------
    // Structural invariants
    // -----------------------------------------------------------------------

    [TestMethod]
    public void Thread_IsNonEmpty()
    {
        var t = Compile("        N = 1\nend");
        Assert.IsTrue(t.Length > 0);
    }

    [TestMethod]
    public void Thread_StartsWithInit()
    {
        var t = Compile("        N = 1\nend");
        // After Step 6, compiled statement bodies absorb Init into the delegate.
        // The first instruction is now CallMsil (body delegate) or Init (for
        // un-compiled statements). Either is valid.
        Assert.IsTrue(t[0].Op == OpCode.Init || t[0].Op == OpCode.CallMsil,
            $"Expected Init or CallMsil as first opcode, got {t[0].Op}");
    }

    [TestMethod]
    public void Thread_EndsWithHalt()
    {
        var t = Compile("        N = 1\nend");
        Assert.AreEqual(OpCode.Halt, t[^1].Op);
    }

    [TestMethod]
    public void Thread_ContainsFinalizeAfterBody()
    {
        var t = Compile("        N = 1\nend");
        // After Step 6, Finalize is inlined into the CallMsil delegate for
        // compiled bodies; un-compiled statements still emit it. Either path
        // is valid — what matters is the program executes correctly.
        bool hasCallMsil  = t.Any(i => i.Op == OpCode.CallMsil);
        bool hasFinalize  = t.Any(i => i.Op == OpCode.Finalize);
        Assert.IsTrue(hasCallMsil || hasFinalize,
            "Expected either CallMsil (with inlined Finalize) or explicit Finalize");
    }

    // -----------------------------------------------------------------------
    // Opcode coverage
    // -----------------------------------------------------------------------

    [TestMethod]
    public void Assignment_EmitsExpectedOpcodes()
    {
        var t = Compile("        N = 1\nend");
        var ops = t.Select(i => i.Op).ToHashSet();
        // With MSIL compilation active the body is replaced by CallMsil;
        // without it (or for goto token lists) the individual opcodes appear.
        bool hasMsil     = ops.Contains(OpCode.CallMsil);
        bool hasThreaded = ops.Contains(OpCode.PushVar) &&
                           ops.Contains(OpCode.PushConst) &&
                           ops.Contains(OpCode.BinaryEquals);
        Assert.IsTrue(hasMsil || hasThreaded);
    }

    [TestMethod]
    public void Addition_EmitsOpAdd()
    {
        var t = Compile("        N = N + 1\nend");
        // CallMsil subsumes OpAdd when MSIL compilation is active.
        Assert.IsTrue(t.Any(i => i.Op == OpCode.OpAdd || i.Op == OpCode.CallMsil));
    }

    [TestMethod]
    public void FunctionCall_EmitsCallFunc()
    {
        var t = Compile("        N = LT(N, 10) N\nend");
        // CallMsil subsumes CallFunc when MSIL compilation is active.
        Assert.IsTrue(t.Any(i => i.Op == OpCode.CallFunc || i.Op == OpCode.CallMsil));
    }

    [TestMethod]
    public void UnconditionalGoto_EmitsSaveRestoreAndDispatch()
    {
        var t = Compile("LOOP    N = N + 1   :(LOOP)\nend");
        var ops = t.Select(i => i.Op).ToHashSet();
        // After Step 9, a single-identifier :(LABEL) goto is absorbed into the
        // body delegate — no SaveFailure/RestoreFailure/GotoIndirect in thread.
        // The delegate calls ResolveLabel and returns the target IP directly.
        bool absorbedIntoDelegate = ops.Contains(OpCode.CallMsil) &&
                                    !ops.Contains(OpCode.GotoIndirect);
        bool legacyThreaded       = ops.Contains(OpCode.SaveFailure) &&
                                    ops.Contains(OpCode.RestoreFailure) &&
                                    ops.Contains(OpCode.GotoIndirect);
        Assert.IsTrue(absorbedIntoDelegate || legacyThreaded,
            "Expected either absorbed delegate or legacy SaveFailure/GotoIndirect pattern");
    }

    [TestMethod]
    public void SuccessGoto_EmitsJumpOnFailureForFallThrough()
    {
        var t = Compile("        LT(N,10)   :S(DONE)\nDONE\nend");
        // After Step 10, single-identifier :S(LABEL) is absorbed into the body
        // delegate — the delegate branches on Failure internally, so no
        // JumpOnFailure appears in the thread for compiled statements.
        bool hasMsil          = t.Any(i => i.Op == OpCode.CallMsil);
        bool hasJumpOnFailure = t.Any(i => i.Op == OpCode.JumpOnFailure);
        Assert.IsTrue(hasMsil || hasJumpOnFailure,
            "Expected either CallMsil (with absorbed :S goto) or JumpOnFailure");
    }

    [TestMethod]
    public void FailureGoto_EmitsJumpOnSuccessForFallThrough()
    {
        var t = Compile("        LT(N,10)   :F(DONE)\nDONE\nend");
        bool hasMsil          = t.Any(i => i.Op == OpCode.CallMsil);
        bool hasJumpOnSuccess = t.Any(i => i.Op == OpCode.JumpOnSuccess);
        Assert.IsTrue(hasMsil || hasJumpOnSuccess,
            "Expected either CallMsil (with absorbed :F goto) or JumpOnSuccess");
    }

    // -----------------------------------------------------------------------
    // Jump target validity
    // -----------------------------------------------------------------------

    [TestMethod]
    public void AllJumpTargets_AreInBounds_SimpleLoop()
    {
        var t = Compile(@"
        N = 0
LOOP    N = N + 1
        LT(N, 100)      :S(LOOP)
        RESULT = N
end");
        for (var i = 0; i < t.Length; i++)
        {
            if (t[i].Op is OpCode.Jump or OpCode.JumpOnSuccess or OpCode.JumpOnFailure)
                Assert.IsTrue(t[i].IntOperand >= 0 && t[i].IntOperand < t.Length,
                    $"Instr {i} ({t[i].Op}) target {t[i].IntOperand} out of bounds");
        }
    }

    [TestMethod]
    public void AllJumpTargets_AreInBounds_Roman()
    {
        var t = Compile(@"
        DEFINE('ROMAN(N)T')                 :(ROMAN_END)
ROMAN   N   RPOS(1)  LEN(1) . T  =         :F(RETURN)
        '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+       T   BREAK(',') . T                  :F(FRETURN)
        ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+                                           :S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('1776')
end");
        for (var i = 0; i < t.Length; i++)
        {
            if (t[i].Op is OpCode.Jump or OpCode.JumpOnSuccess or OpCode.JumpOnFailure)
                Assert.IsTrue(t[i].IntOperand >= 0 && t[i].IntOperand < t.Length,
                    $"Instr {i} ({t[i].Op}) target {t[i].IntOperand} out of bounds");
        }
    }

    // -----------------------------------------------------------------------
    // Slot index bounds
    // -----------------------------------------------------------------------

    [TestMethod]
    public void PushVar_SlotIndices_AreInBounds()
    {
        var build = SetupTests.SetupScript("-b", "        R1 = ROMAN('1776')\nend");
        var t = new ThreadedCodeCompiler(build).Compile();
        foreach (var instr in t.Where(i => i.Op == OpCode.PushVar))
            Assert.IsTrue(instr.IntOperand >= 0 && instr.IntOperand < build.VariableSlots.Count,
                $"PushVar slot {instr.IntOperand} out of range");
    }

    [TestMethod]
    public void PushConst_SlotIndices_AreInBounds()
    {
        var build = SetupTests.SetupScript("-b", "        R1 = '1776'\nend");
        var t = new ThreadedCodeCompiler(build).Compile();
        foreach (var instr in t.Where(i => i.Op == OpCode.PushConst))
            Assert.IsTrue(instr.IntOperand >= 0 && instr.IntOperand < build.Constants.Count,
                $"PushConst slot {instr.IntOperand} out of range");
    }
}
