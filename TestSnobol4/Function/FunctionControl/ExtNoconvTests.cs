using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// Tests for net-ext-noconv: NOCONV (type 0) prototype args pass ARRAY/TABLE/PDBLK
/// through to foreign functions unconverted.
///
/// C-ABI tests (Steps 1+2): prototype parser accepts NOCONV keyword and unknown
/// tokens; CallNativeFunction pins the Var and passes its address as IntPtr.
///
/// .NET IExternalLibrary tests (Step 3): TraverseArray/TraverseTable/GetDataFields
/// let a .NET plugin inspect SNOBOL4 objects received as arguments.
/// </summary>
[TestClass, DoNotParallelize]
public class ExtNoconvTests
{
    private static readonly object s_consoleLock = new();

    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    /// <summary>
    /// Runs a script and returns all text written to OUTPUT (Console.Error in DOTNET), trimmed.
    /// </summary>
    private static string RunCapture(string script)
    {
        lock (s_consoleLock)
        {
            var old = Console.Out;
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms) { AutoFlush = true };
            Console.SetOut(sw);
            try { SetupTests.SetupScript("-b", script); }
            finally { Console.SetOut(old); }
            ms.Position = 0;
            using var sr = new System.IO.StreamReader(ms);
            return sr.ReadToEnd().Trim();
        }
    }

    // ── A. Prototype parser: NOCONV keyword and unknown tokens ────────────

    [TestMethod]
    public void ParsePrototype_NoconvKeyword_Recognized()
    {
        var proto = Executive.ParsePrototype("SNC_TEST(NOCONV)INTEGER", out var err);
        Assert.AreEqual(0, err);
        Assert.IsNotNull(proto);
        Assert.AreEqual(1, proto.ArgTypes.Count);
        Assert.AreEqual("NOCONV", proto.ArgTypes[0]);
        Assert.AreEqual("INTEGER", proto.ReturnType);
    }

    [TestMethod]
    public void ParsePrototype_UnknownToken_BecomesNoconv()
    {
        // An unrecognised type token should be treated as NOCONV (type 0)
        var proto = Executive.ParsePrototype("SNC_TEST(XYZZY)INTEGER", out var err);
        Assert.AreEqual(0, err);
        Assert.IsNotNull(proto);
        Assert.AreEqual("NOCONV", proto.ArgTypes[0]);
    }

    [TestMethod]
    public void ParsePrototype_MixedNoconvAndInteger()
    {
        var proto = Executive.ParsePrototype("SNC_MIXED(NOCONV,INTEGER)INTEGER", out var err);
        Assert.AreEqual(0, err);
        Assert.IsNotNull(proto);
        Assert.AreEqual(2, proto.ArgTypes.Count);
        Assert.AreEqual("NOCONV",  proto.ArgTypes[0]);
        Assert.AreEqual("INTEGER", proto.ArgTypes[1]);
    }

    // ── B. C-ABI NOCONV: array and table arrive non-null ─────────────────
    // These tests require the GCHandle-pinning path in CallNativeFunction to
    // deliver a non-null pointer to the C library.  The .so is built and
    // exported correctly but the managed-object pinning marshal path needs
    // integration testing in a full native-interop environment.
    // The core net-ext-noconv feature (.NET traversal API) is covered by
    // the Noconv_DotNet_* tests above.

    [TestMethod]
    public void Noconv_CLib_ArrayPassed_NonNull()
    {
        // snc_array_passed(void*) returns 1 if pointer is non-null.
        // We LOAD with NOCONV arg type and pass an ARRAY — the function
        // should receive a non-null pinned pointer and return 1.
        var lib = SetupTests.NoconvCLibPath;
        if (!File.Exists(lib)) Assert.Inconclusive($"{Path.GetFileName(lib)} not found: {lib}");

        var output = RunCapture($@"
            LOAD('snc_array_passed(NOCONV)INTEGER', '{lib}')   :F(FEND)
            A = ARRAY(3)
            A<1> = 10
            A<2> = 20
            A<3> = 30
            R = snc_array_passed(A)                            :F(FEND)
            OUTPUT = R
FEND
END");
        Assert.AreEqual("1", output);
    }

    [TestMethod]
    public void Noconv_CLib_TablePassed_NonNull()
    {
        // snc_table_passed(void*) returns 1 if pointer is non-null.
        var lib = SetupTests.NoconvCLibPath;
        if (!File.Exists(lib)) Assert.Inconclusive($"{Path.GetFileName(lib)} not found: {lib}");

        var output = RunCapture($@"
            LOAD('snc_table_passed(NOCONV)INTEGER', '{lib}')   :F(FEND)
            T = TABLE()
            T<'key'> = 'value'
            R = snc_table_passed(T)                            :F(FEND)
            OUTPUT = R
FEND
END");
        Assert.AreEqual("1", output);
    }

    // ── C. .NET IExternalLibrary traversal API ────────────────────────────

    [TestMethod]
    public void Noconv_DotNet_ArraySum()
    {
        // NoconvLib.Traverser sums integer elements of an ArrayVar; returns INTEGER.
        var dll = SetupTests.NoconvDotNetLibraryPath;
        if (!File.Exists(dll)) Assert.Inconclusive($"NoconvDotNetLibrary.dll not found: {dll}");

        var b = Run($@"
            LOAD('{dll}', 'NoconvDotNetLibrary.NoconvLib')     :F(FEND)
            A = ARRAY(4)
            A<1> = 10
            A<2> = 20
            A<3> = 30
            A<4> = 40
            R = Traverser(A)                                   :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("100", b.Execute!.IdentifierTable[b.FoldCase("R")].ToString());
    }

    [TestMethod]
    public void Noconv_DotNet_TableCount()
    {
        // NoconvLib.TableInspector counts key-value pairs in a TableVar; returns INTEGER.
        var dll = SetupTests.NoconvDotNetLibraryPath;
        if (!File.Exists(dll)) Assert.Inconclusive($"NoconvDotNetLibrary.dll not found: {dll}");

        var b = Run($@"
            LOAD('{dll}', 'NoconvDotNetLibrary.NoconvLib')     :F(FEND)
            T = TABLE()
            T<'a'> = 1
            T<'b'> = 2
            T<'c'> = 3
            R = TableInspector(T)                              :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("3", b.Execute!.IdentifierTable[b.FoldCase("R")].ToString());
    }

    [TestMethod]
    public void Noconv_DotNet_DataFields()
    {
        // NoconvLib.DataFieldCount returns the number of fields in a DATA type instance.
        var dll = SetupTests.NoconvDotNetLibraryPath;
        if (!File.Exists(dll)) Assert.Inconclusive($"NoconvDotNetLibrary.dll not found: {dll}");

        var b = Run($@"
            LOAD('{dll}', 'NoconvDotNetLibrary.NoconvLib')     :F(FEND)
            DATA('POINT(X,Y)')
            P = POINT(3, 4)
            R = DataFieldCount(P)                              :F(FEND)
FEND
END");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("2", b.Execute!.IdentifierTable[b.FoldCase("R")].ToString());
    }
}
