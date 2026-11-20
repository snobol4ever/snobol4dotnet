using System.Runtime.InteropServices;
using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// Tests for net-ext-create: a C external function allocates a block of
/// opaque native state, returns it as EXTERNAL, and SNOBOL4 passes it back
/// as NOCONV on subsequent calls.
///
/// All tests use libspitbol_create.so — no libsnobol4_rt.so dependency.
/// </summary>
[TestClass]
public class ExtCreateTests
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    private static string Lib => SetupTests.CreateCLibPath;

    // ── A. create_counter / bump_counter / read_counter ──────────────────

    /// <summary>
    /// create_counter() returns EXTERNAL pointer; bump_counter(ptr) increments
    /// and returns the new value.  Five bumps → 1,2,3,4,5.
    /// Proves ExternalVar round-trips through SNOBOL4 storage and back as NOCONV.
    /// </summary>
    [TestMethod]
    public void Create_Counter_BumpFiveTimes()
    {
        if (!File.Exists(Lib)) Assert.Inconclusive($"{Path.GetFileName(Lib)} not found: {Lib}");

        var b = Run($@"
            LOAD('create_counter()EXTERNAL', '{Lib}')    :F(FEND)
            LOAD('bump_counter(NOCONV)INTEGER', '{Lib}') :F(FEND)
            PTR = create_counter()                       :F(FEND)
            R1 = bump_counter(PTR)                       :F(FEND)
            R2 = bump_counter(PTR)                       :F(FEND)
            R3 = bump_counter(PTR)                       :F(FEND)
            R4 = bump_counter(PTR)                       :F(FEND)
            R5 = bump_counter(PTR)                       :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var id   = b.Execute!.IdentifierTable;
        var fold = b.FoldCase;
        Assert.AreEqual("1", id[fold("R1")].ToString(), "bump 1");
        Assert.AreEqual("2", id[fold("R2")].ToString(), "bump 2");
        Assert.AreEqual("3", id[fold("R3")].ToString(), "bump 3");
        Assert.AreEqual("4", id[fold("R4")].ToString(), "bump 4");
        Assert.AreEqual("5", id[fold("R5")].ToString(), "bump 5");
    }

    /// <summary>
    /// read_counter reads without incrementing.
    /// After two bumps, read_counter returns 2 without changing state.
    /// </summary>
    [TestMethod]
    public void Create_Counter_ReadDoesNotIncrement()
    {
        if (!File.Exists(Lib)) Assert.Inconclusive($"{Path.GetFileName(Lib)} not found: {Lib}");

        var b = Run($@"
            LOAD('create_counter()EXTERNAL', '{Lib}')    :F(FEND)
            LOAD('bump_counter(NOCONV)INTEGER', '{Lib}') :F(FEND)
            LOAD('read_counter(NOCONV)INTEGER', '{Lib}') :F(FEND)
            PTR = create_counter()                       :F(FEND)
            R1 = bump_counter(PTR)                       :F(FEND)
            R2 = bump_counter(PTR)                       :F(FEND)
            RD = read_counter(PTR)                       :F(FEND)
            R3 = bump_counter(PTR)                       :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var id   = b.Execute!.IdentifierTable;
        var fold = b.FoldCase;
        Assert.AreEqual("2", id[fold("RD")].ToString(), "read after 2 bumps");
        Assert.AreEqual("3", id[fold("R3")].ToString(), "bump after read");
    }

    // ── B. create_pair / pair_sum ─────────────────────────────────────────

    /// <summary>
    /// create_pair(a,b) returns EXTERNAL; pair_sum(ptr) returns a+b.
    /// Tests EXTERNAL return with INTEGER inputs, and NOCONV roundtrip.
    /// </summary>
    [TestMethod]
    public void Create_Pair_SumCorrect()
    {
        if (!File.Exists(Lib)) Assert.Inconclusive($"{Path.GetFileName(Lib)} not found: {Lib}");

        var b = Run($@"
            LOAD('create_pair(INTEGER,INTEGER)EXTERNAL', '{Lib}') :F(FEND)
            LOAD('pair_sum(NOCONV)INTEGER', '{Lib}')              :F(FEND)
            P1 = create_pair(10, 32)                              :F(FEND)
            S1 = pair_sum(P1)                                     :F(FEND)
            P2 = create_pair(100, 1)                              :F(FEND)
            S2 = pair_sum(P2)                                     :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var id   = b.Execute!.IdentifierTable;
        var fold = b.FoldCase;
        Assert.AreEqual("42",  id[fold("S1")].ToString(), "10+32=42");
        Assert.AreEqual("101", id[fold("S2")].ToString(), "100+1=101");
    }

    // ── C. Two independent counters don't share state ─────────────────────

    /// <summary>
    /// Two separate create_counter() calls return independent blocks.
    /// Bumping one does not affect the other.
    /// </summary>
    [TestMethod]
    public void Create_TwoCounters_Independent()
    {
        if (!File.Exists(Lib)) Assert.Inconclusive($"{Path.GetFileName(Lib)} not found: {Lib}");

        var b = Run($@"
            LOAD('create_counter()EXTERNAL', '{Lib}')    :F(FEND)
            LOAD('bump_counter(NOCONV)INTEGER', '{Lib}') :F(FEND)
            A = create_counter()                         :F(FEND)
            B = create_counter()                         :F(FEND)
            A1 = bump_counter(A)                         :F(FEND)
            A2 = bump_counter(A)                         :F(FEND)
            A3 = bump_counter(A)                         :F(FEND)
            B1 = bump_counter(B)                         :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var id   = b.Execute!.IdentifierTable;
        var fold = b.FoldCase;
        Assert.AreEqual("3", id[fold("A3")].ToString(), "A bumped 3 times");
        Assert.AreEqual("1", id[fold("B1")].ToString(), "B independent, only 1 bump");
    }

    //[TestMethod]
    public void Diagnostic_LoadDLL()
    {
        Console.WriteLine($"Test process is {(Environment.Is64BitProcess ? "64-bit" : "32-bit")}");
        Console.WriteLine($"DLL path: {Lib}");
        Console.WriteLine($"DLL exists: {File.Exists(Lib)}");

        var pdbPath = Path.ChangeExtension(Lib, ".pdb");
        Console.WriteLine($"PDB exists: {File.Exists(pdbPath)}");

        // Try direct load to see the error
        try
        {
            var handle = NativeLibrary.Load(Lib);
            Console.WriteLine($"✅ DLL loaded: 0x{handle:X}");

            if (NativeLibrary.TryGetExport(handle, "create_counter", out var funcPtr))
            {
                Console.WriteLine($"✅ create_counter found at: 0x{funcPtr:X}");
            }
            else
            {
                Console.WriteLine("❌ create_counter NOT exported");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Load failed: {ex.Message}");
        }
    }

    //[TestMethod]
    public void Verify_DLL_Exports()
    {
        Console.WriteLine($"DLL Path: {Lib}");
        Console.WriteLine($"DLL Exists: {File.Exists(Lib)}");

        if (!File.Exists(Lib))
        {
            Assert.Fail("DLL not found!");
        }

        // Check architecture
        //var info = new System.Diagnostics.FileVersionInfo();
        Console.WriteLine($"Process is {(Environment.Is64BitProcess ? "64-bit" : "32-bit")}");

        // Try to load
        IntPtr handle = IntPtr.Zero;
        try
        {
            handle = NativeLibrary.Load(Lib);
            Console.WriteLine($"✅ DLL loaded successfully: 0x{handle:X}");

            // Check exports
            var exports = new[] { "create_counter", "bump_counter", "read_counter", "create_pair", "pair_sum" };
            foreach (var export in exports)
            {
                if (NativeLibrary.TryGetExport(handle, export, out var funcPtr))
                {
                    Console.WriteLine($"✅ {export} found at 0x{funcPtr:X}");
                }
                else
                {
                    Console.WriteLine($"❌ {export} NOT FOUND");
                    Assert.Fail($"Export {export} is missing!");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Load failed: {ex.Message}");
            Assert.Fail(ex.Message);
        }
        finally
        {
            if (handle != IntPtr.Zero)
            {
                NativeLibrary.Free(handle);
            }
        }
    }
}
