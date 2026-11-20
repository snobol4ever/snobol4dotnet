using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// Tests for net-load-xn: SPITBOL x32 C-ABI parity.
///
/// Covers:
///   A. xn1st  — first-call detection (snobol4_first_call / xndta counter)
///   B. xncbp  — callback registered via snobol4_register_callback fires on UNLOAD
///   C. xncbp  — callback fires via FireAllNativeCallbacks (ProcessExit simulation)
///   D. xnsave — double-fire guard: callback fires exactly once across UNLOAD + exit
///
/// All tests use libspitbol_xn.so (links libsnobol4_rt.so).
/// xn_reset_callback_count() resets the process-global counter between tests.
/// Tests are serialized ([DoNotParallelize]) so the shared C static stays coherent.
/// </summary>
[TestClass]
[DoNotParallelize]
public class LoadXnTests
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    private static string Lib => SetupTests.XnCLibPath;

    private static void SkipIfMissing()
    {
        if (!File.Exists(Lib))
            Assert.Inconclusive($"libspitbol_xn.so not found: {Lib}");
    }

    // ── A. xn1st: counter initialised only on first call ────────────────

    [TestMethod]
    public void Xn_FirstCall_CounterInitialisedOnce()
    {
        SkipIfMissing();

        var b = Run($@"
            LOAD('xn_counter()INTEGER', '{Lib}')  :F(FEND)
            C1 = xn_counter()                     :F(FEND)
            C2 = xn_counter()                     :F(FEND)
            C3 = xn_counter()                     :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var id   = b.Execute!.IdentifierTable;
        var fold = b.FoldCase;
        Assert.AreEqual("1", id[fold("C1")].ToString(), "call 1");
        Assert.AreEqual("2", id[fold("C2")].ToString(), "call 2");
        Assert.AreEqual("3", id[fold("C3")].ToString(), "call 3");
    }

    // ── B. xncbp: callback fired on UNLOAD ──────────────────────────────

    [TestMethod]
    public void Xn_Callback_FiredOnUnload()
    {
        SkipIfMissing();

        // Reset process-global counter so this test is independent.
        Run($@"
            LOAD('xn_reset_callback_count()INTEGER', '{Lib}')  :F(FEND)
            xn_reset_callback_count()
END");

        // Arm callback, then UNLOAD (should fire xn_cleanup once).
        Run($@"
            LOAD('xn_register_callback()INTEGER', '{Lib}')  :F(FEND)
            xn_register_callback()                          :F(FEND)
            UNLOAD('xn_register_callback')
END");

        // Query counter in a fresh run.
        var b = Run($@"
            LOAD('xn_callback_count()INTEGER', '{Lib}')  :F(FEND)
            CNT = xn_callback_count()                    :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("1",
            b.Execute!.IdentifierTable[b.FoldCase("CNT")].ToString(),
            "callback should have fired once on UNLOAD");
    }

    // ── C. xncbp: callback fired via ProcessExit hook ───────────────────

    [TestMethod]
    public void Xn_Callback_FiredOnProcessExit()
    {
        SkipIfMissing();

        // Reset counter.
        Run($@"
            LOAD('xn_reset_callback_count()INTEGER', '{Lib}')  :F(FEND)
            xn_reset_callback_count()
END");

        // Arm callback but do NOT UNLOAD — simulate only ProcessExit path.
        var b = Run($@"
            LOAD('xn_register_callback()INTEGER', '{Lib}')  :F(FEND)
            xn_register_callback()                          :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);

        // Verify not yet fired.
        var bBefore = Run($@"
            LOAD('xn_callback_count()INTEGER', '{Lib}')  :F(FEND)
            CNT = xn_callback_count()                    :F(FEND)
FEND
END");
        Assert.AreEqual("0",
            bBefore.Execute!.IdentifierTable[bBefore.FoldCase("CNT")].ToString(),
            "callback must not fire before ProcessExit");

        // Simulate ProcessExit on the Executive that holds the NativeContext.
        b.Execute!.FireAllNativeCallbacks();

        // Verify fired once.
        var bAfter = Run($@"
            LOAD('xn_callback_count()INTEGER', '{Lib}')  :F(FEND)
            CNT = xn_callback_count()                    :F(FEND)
FEND
END");
        Assert.AreEqual("1",
            bAfter.Execute!.IdentifierTable[bAfter.FoldCase("CNT")].ToString(),
            "callback should fire once via FireAllNativeCallbacks");
    }

    // ── D. xnsave: double-fire guard ────────────────────────────────────

    [TestMethod]
    public void Xn_Callback_DoubleFire_GuardWorks()
    {
        SkipIfMissing();

        // Reset counter.
        Run($@"
            LOAD('xn_reset_callback_count()INTEGER', '{Lib}')  :F(FEND)
            xn_reset_callback_count()
END");

        // Arm callback, UNLOAD fires it once, then call FireAllNativeCallbacks.
        // After UNLOAD the NativeContexts entry is removed, so FireAll iterates
        // zero entries — still exactly 1 fire total.
        var b = Run($@"
            LOAD('xn_register_callback()INTEGER', '{Lib}')  :F(FEND)
            xn_register_callback()                          :F(FEND)
            UNLOAD('xn_register_callback')
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);

        // Extra ProcessExit simulation — must be a no-op (entry already removed).
        b.Execute!.FireAllNativeCallbacks();

        var bCount = Run($@"
            LOAD('xn_callback_count()INTEGER', '{Lib}')  :F(FEND)
            CNT = xn_callback_count()                    :F(FEND)
FEND
END");
        Assert.AreEqual("1",
            bCount.Execute!.IdentifierTable[bCount.FoldCase("CNT")].ToString(),
            "double-fire guard: callback must fire exactly once");
    }
}
