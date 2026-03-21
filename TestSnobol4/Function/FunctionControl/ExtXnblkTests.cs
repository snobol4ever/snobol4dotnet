using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// Tests for net-ext-xnblk: XNBLK opaque persistent state.
///
/// A C external function allocates a private xndta[] storage block on first
/// call and receives the same block on every subsequent call.  This mirrors
/// the SPITBOL xn1st / xndta[] mechanism from blocks32.h.
///
/// C-ABI tests use libspitbol_xn.so which links against libsnobol4_rt.so.
/// The .NET test uses a StatefulSumLibrary (IExternalLibrary + IStatefulExternalLibrary).
/// </summary>
[TestClass]
public class ExtXnblkTests
{
    private static readonly object s_consoleLock = new();

    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    private static string RunCapture(string script)
    {
        lock (s_consoleLock)
        {
            var old = Console.Error;
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms) { AutoFlush = true };
            Console.SetError(sw);
            try { SetupTests.SetupScript("-b", script); }
            finally { Console.SetError(old); }
            ms.Position = 0;
            using var sr = new System.IO.StreamReader(ms);
            return sr.ReadToEnd().Trim();
        }
    }

    // ── A. C-ABI: xndta counter increments across calls ─────────────────

    /// <summary>
    /// xn_counter() uses xndta[0] as a persistent counter.
    /// Calling it 5 times should return 1, 2, 3, 4, 5.
    /// </summary>
    [TestMethod]
    public void Xnblk_Counter_Increments()
    {
        var lib = SetupTests.XnCLibPath;
        if (!File.Exists(lib)) Assert.Inconclusive($"libspitbol_xn.so not found: {lib}");

        var b = Run($@"
            LOAD('xn_counter()INTEGER', '{lib}')  :F(FEND)
            R1 = xn_counter()                     :F(FEND)
            R2 = xn_counter()                     :F(FEND)
            R3 = xn_counter()                     :F(FEND)
            R4 = xn_counter()                     :F(FEND)
            R5 = xn_counter()                     :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var id = b.Execute!.IdentifierTable;
        var fold = b.FoldCase;
        Assert.AreEqual("1", id[fold("R1")].ToString(), "call 1");
        Assert.AreEqual("2", id[fold("R2")].ToString(), "call 2");
        Assert.AreEqual("3", id[fold("R3")].ToString(), "call 3");
        Assert.AreEqual("4", id[fold("R4")].ToString(), "call 4");
        Assert.AreEqual("5", id[fold("R5")].ToString(), "call 5");
    }

    /// <summary>
    /// xn_first_call_flag() returns 1 on the first call, 0 on the second.
    /// Directly tests the snobol4_first_call() shim.
    /// </summary>
    [TestMethod]
    public void Xnblk_FirstCall_FlagWorks()
    {
        var lib = SetupTests.XnCLibPath;
        if (!File.Exists(lib)) Assert.Inconclusive($"libspitbol_xn.so not found: {lib}");

        var b = Run($@"
            LOAD('xn_first_call_flag()INTEGER', '{lib}')  :F(FEND)
            F1 = xn_first_call_flag()                     :F(FEND)
            F2 = xn_first_call_flag()                     :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var id = b.Execute!.IdentifierTable;
        var fold = b.FoldCase;
        Assert.AreEqual("1", id[fold("F1")].ToString(), "first call should be 1");
        Assert.AreEqual("0", id[fold("F2")].ToString(), "second call should be 0");
    }

    // ── B. .NET: IStatefulExternalLibrary OnFirstCall fires once ─────────

    /// <summary>
    /// A .NET library implementing IStatefulExternalLibrary accumulates a sum
    /// across calls.  OnFirstCall resets the accumulator to 0 exactly once.
    /// Three SNOBOL4 calls to AccumSum(10), AccumSum(20), AccumSum(30) should
    /// produce running totals 10, 30, 60 — verifying xndta-equivalent persistence.
    /// </summary>
    [TestMethod]
    public void Xnblk_DotNet_StatefulLibrary()
    {
        StatefulSumLibrary.Reset();

        // Boot a real Builder with an empty program so we get a live Executive.
        var b = Run("END");
        var exec = b.Execute!;

        // Manually perform the same Init+wrap that Load.cs does for IStatefulExternalLibrary.
        var lib = new StatefulSumLibrary();
        var keysBefore = new HashSet<string>(exec.FunctionTable.Keys);
        lib.Init(exec);

        var stateful = (IStatefulExternalLibrary)lib;
        var fired = false;
        foreach (var key in exec.FunctionTable.Keys.Where(k => !keysBefore.Contains(k)).ToList())
        {
            var original = exec.FunctionTable[key];
            if (original is null) continue;
            exec.FunctionTable[key] = new FunctionTableEntry(
                exec, key,
                args =>
                {
                    if (!fired) { fired = true; stateful.OnFirstCall(exec); }
                    original.Handler(args);
                },
                original.ArgumentCount, false);
        }

        // Prime accumulator to a non-zero value so we can prove OnFirstCall resets it.
        StatefulSumLibrary.Accumulator = 999;

        // Invoke AccumSum three times through the wrapped entry.
        var fnKey = b.FoldCase("AccumSum");
        exec.FunctionTable[fnKey]!.Handler([new IntegerVar(10)]);
        exec.FunctionTable[fnKey]!.Handler([new IntegerVar(20)]);
        exec.FunctionTable[fnKey]!.Handler([new IntegerVar(30)]);

        Assert.IsTrue(fired, "first-call guard should have fired");
        Assert.AreEqual(1, StatefulSumLibrary.OnFirstCallCount,
            "OnFirstCall must fire exactly once");
        Assert.AreEqual(60L, StatefulSumLibrary.Accumulator,
            "accumulator after reset+10+20+30 should be 60");
    }
}

// ── In-process stateful library fixture ──────────────────────────────────────

/// <summary>
/// Minimal in-process IExternalLibrary + IStatefulExternalLibrary fixture.
/// Registers AccumSum(INTEGER)INTEGER — adds argument to a static accumulator.
/// OnFirstCall resets the accumulator.
/// </summary>
internal class StatefulSumLibrary : IExternalLibrary, IStatefulExternalLibrary
{
    internal static long Accumulator;
    internal static int  OnFirstCallCount;

    internal static void Reset() { Accumulator = 0; OnFirstCallCount = 0; }

    public void Init(Executive executive)
    {
        var key = executive.Parent.FoldCase("AccumSum");
        executive.FunctionTable[key] = new FunctionTableEntry(
            executive, key,
            args =>
            {
                var v = args.Count > 0 ? args[0] : new IntegerVar(0);
                v.Convert(Executive.VarType.INTEGER, out _, out var iv, executive);
                Accumulator += (long)iv;
                executive.SystemStack.Push(new IntegerVar(Accumulator));
                executive.Failure = false;
            },
            1, false);
    }

    public void OnFirstCall(Executive executive)
    {
        Accumulator = 0;
        OnFirstCallCount++;
    }
}
