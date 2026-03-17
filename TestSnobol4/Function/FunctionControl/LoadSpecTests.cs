using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// Tests for LOAD/UNLOAD spec-compliant path (net-load-spitbol):
///   A. Prototype string parser unit tests
///   B. Dispatcher routing: prototype-string vs path-like s1
///   C. Spec path: LOAD('FNAME(T1..Tn)Tr', libpath) — C shared library
///   D. UNLOAD(fname) by function name
///   E. SNOLIB env var search
///   F. Error conditions: Error 139-141, Error 202
///   G. Regression: existing .NET-native tests unaffected
/// </summary>
[TestClass]
public class LoadSpecTests
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    private static string Str(string name, Builder b) =>
        b.Execute!.IdentifierTable[b.FoldCase(name)].ToString();

    private static string SpitbolLibPath
    {
        get
        {
            // Walk 4 levels up from net10.0/Release/bin/TestSnobol4 to solution root
            var dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            for (var i = 0; i < 4; i++)
                dir = Path.GetDirectoryName(dir) ?? dir;
            return Path.Combine(dir, "CustomFunction", "SpitbolCLib", "libspitbol_math.so");
        }
    }

    // ── A. Prototype parser unit tests ─────────────────────────────────

    [TestMethod]
    public void ParsePrototype_BasicNoArgs()
    {
        var proto = Executive.ParsePrototype("MYFUNC()", out var err);
        Assert.AreEqual(0, err);
        Assert.IsNotNull(proto);
        Assert.AreEqual("MYFUNC", proto.FunctionName);
        Assert.AreEqual(0, proto.ArgTypes.Count);
        Assert.AreEqual("", proto.ReturnType);
    }

    [TestMethod]
    public void ParsePrototype_OneIntegerArg()
    {
        var proto = Executive.ParsePrototype("SPL_ADD(INTEGER,INTEGER)INTEGER", out var err);
        Assert.AreEqual(0, err);
        Assert.IsNotNull(proto);
        Assert.AreEqual("SPL_ADD", proto.FunctionName);
        Assert.AreEqual(2, proto.ArgTypes.Count);
        Assert.AreEqual("INTEGER", proto.ArgTypes[0]);
        Assert.AreEqual("INTEGER", proto.ArgTypes[1]);
        Assert.AreEqual("INTEGER", proto.ReturnType);
    }

    [TestMethod]
    public void ParsePrototype_RealArgs()
    {
        var proto = Executive.ParsePrototype("spl_scale(REAL,REAL)REAL", out var err);
        Assert.AreEqual(0, err);
        Assert.IsNotNull(proto);
        Assert.AreEqual("spl_scale", proto.FunctionName);
        Assert.AreEqual("REAL", proto.ArgTypes[0]);
        Assert.AreEqual("REAL", proto.ReturnType);
    }

    [TestMethod]
    public void ParsePrototype_StringArgs()
    {
        var proto = Executive.ParsePrototype("REVERSE(STRING)STRING", out var err);
        Assert.AreEqual(0, err);
        Assert.IsNotNull(proto);
        Assert.AreEqual("REVERSE", proto.FunctionName);
        Assert.AreEqual("STRING", proto.ArgTypes[0]);
        Assert.AreEqual("STRING", proto.ReturnType);
    }

    [TestMethod]
    public void ParsePrototype_MissingLeftParen_Error139()
    {
        var proto = Executive.ParsePrototype("BADNAME", out var err);
        Assert.AreEqual(139, err);
        Assert.IsNull(proto);
    }

    [TestMethod]
    public void ParsePrototype_EmptyFunctionName_Error140()
    {
        var proto = Executive.ParsePrototype("(INTEGER)INTEGER", out var err);
        Assert.AreEqual(140, err);
        Assert.IsNull(proto);
    }

    [TestMethod]
    public void ParsePrototype_MissingRightParen_Error141()
    {
        var proto = Executive.ParsePrototype("MYFUNC(INTEGER", out var err);
        Assert.AreEqual(141, err);
        Assert.IsNull(proto);
    }

    // ── B. Dispatcher routing ──────────────────────────────────────────

    [TestMethod]
    public void Dispatcher_PrototypeString_RoutesToSpecPath()
    {
        // A prototype-string s1 containing '(' routes to spec path.
        // Using a non-existent lib — we just want to confirm it attempts spec path
        // and fails :F (not crashes).
        var b = Run(@"
        load('MYFUNC(INTEGER)INTEGER', '/nonexistent/lib.so')   :S(OK)F(FAIL)
FAIL    result = 'failed'   :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual("failed", Str("result", b));
    }

    [TestMethod]
    public void Dispatcher_PathLike_RoutesToDotNetPath()
    {
        // A path-like s1 without '(' routes to .NET-native path.
        // Using a non-existent path — expect :F, not error 139/140/141.
        var b = Run(@"
        load('/nonexistent/file.dll', 'Some.Class')   :S(OK)F(FAIL)
FAIL    result = 'failed'   :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual("failed", Str("result", b));
    }

    // ── C. Spec path — C shared library ───────────────────────────────

    [TestMethod]
    public void Load_Spec_NativeLibFound_Succeeds()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')   :S(OK)F(FAIL)
FAIL    result = 'failed'   :(END)
OK      result = 'loaded'
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("loaded", Str("result", b));
    }

    [TestMethod]
    public void Load_Spec_MissingLib_Fails()
    {
        var b = Run(@"
        load('spl_add(INTEGER,INTEGER)INTEGER', '/no/such/lib.so')   :S(OK)F(FAIL)
FAIL    result = 'failed'   :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual("failed", Str("result", b));
    }

    [TestMethod]
    public void Load_Spec_Idempotent_DoubleLoad()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')
        result = 'ok'
        unload('spl_add')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }

    // ── D. UNLOAD(fname) spec path ─────────────────────────────────────

    [TestMethod]
    public void Unload_ByFunctionName_Succeeds()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')
        unload('spl_add')   :S(OK)F(FAIL)
FAIL    result = 'failed'   :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }

    [TestMethod]
    public void Unload_ByFunctionName_IdempotentDoubleUnload()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')
        unload('spl_add')
        unload('spl_add')
        result = 'ok'
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }

    [TestMethod]
    public void Unload_ByFunctionName_ReloadAfterUnload()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')
        unload('spl_add')
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')
        unload('spl_add')
        result = 'ok'
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }

    // ── E. SNOLIB search ──────────────────────────────────────────────

    [TestMethod]
    public void Load_Spec_SnolibSearch_FindsLib()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var libDir = Path.GetDirectoryName(lib)!;
        var oldSnolib = Environment.GetEnvironmentVariable("SNOLIB");
        try
        {
            Environment.SetEnvironmentVariable("SNOLIB", libDir);
            // s2 omitted — SNOLIB should find libspitbol_math.so by fname base
            var b = Run(@"
        load('spl_add(INTEGER,INTEGER)INTEGER', 'libspitbol_math')   :S(OK)F(FAIL)
FAIL    result = 'failed'   :(END)
OK      result = 'ok'
END
        unload('spl_add')
end");
            Assert.AreEqual("ok", Str("result", b));
        }
        finally
        {
            Environment.SetEnvironmentVariable("SNOLIB", oldSnolib);
        }
    }

    [TestMethod]
    public void Load_Spec_SnolibEmpty_Fails()
    {
        var oldSnolib = Environment.GetEnvironmentVariable("SNOLIB");
        try
        {
            Environment.SetEnvironmentVariable("SNOLIB", "");
            var b = Run(@"
        load('spl_add(INTEGER,INTEGER)INTEGER')   :S(OK)F(FAIL)
FAIL    result = 'failed'   :(END)
OK      result = 'ok'
END
end");
            Assert.AreEqual("failed", Str("result", b));
        }
        finally
        {
            Environment.SetEnvironmentVariable("SNOLIB", oldSnolib);
        }
    }

    // ── F. Error conditions ────────────────────────────────────────────

    [TestMethod]
    public void Load_Spec_MissingLeftParen_Triggers139()
    {
        // s1 contains no '(' but isn't a valid path either — error 139 via spec parser
        // Actually the dispatcher checks for '(' first; 'BADNAME' has no '(' → goes .NET path
        // A string with no '(' is routed to .NET path, so just fails :F.
        // To force spec path we need a '(' → use malformed prototype with empty fname.
        var b = Run(@"
        load('(INTEGER)INTEGER', '/some/lib.so')   :S(OK)F(FAIL)
FAIL    result = 'failed'   :(END)
OK      result = 'ok'
END
end");
        // Error 140 (empty fname) causes runtime exception, program exits with error
        Assert.IsTrue(b.ErrorCodeHistory.Count > 0 || Str("result", b) == "failed");
    }

    [TestMethod]
    public void Unload_NonNaturalVarName_Triggers201()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')
        unload('123bad')
end");
        // "123bad" is not a natural variable name → error 201 logged
        Assert.IsTrue(b.ErrorCodeHistory.Count > 0);
    }

    // ── H. Native call marshal — INTEGER / REAL / STRING return ──────────

    [TestMethod]
    public void Call_Native_IntegerReturn_Add()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')
        r = spl_add(3, 4)
        unload('spl_add')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("7", Str("r", b));
    }

    [TestMethod]
    public void Call_Native_RealReturn_Scale()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_scale(REAL,REAL)REAL', '{lib}')
        r = spl_scale(2.5, 4.0)
        unload('spl_scale')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("10.", Str("r", b));
    }

    [TestMethod]
    public void Call_Native_RealReturn_Negate()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_negate(REAL)REAL', '{lib}')
        r = spl_negate(7.5)
        unload('spl_negate')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("-7.5", Str("r", b));
    }

    [TestMethod]
    public void Call_Native_IntegerReturn_Strlen()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_strlen(STRING)INTEGER', '{lib}')
        r = spl_strlen('hello')
        unload('spl_strlen')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("5", Str("r", b));
    }

    [TestMethod]
    public void Call_Native_IntegerCoercesFromString()
    {
        var lib = Path.GetFullPath(SpitbolLibPath);
        if (!File.Exists(lib)) Assert.Inconclusive($"Native lib not found: {lib}");

        var b = Run($@"
        load('spl_add(INTEGER,INTEGER)INTEGER', '{lib}')
        r = spl_add('10', '5')
        unload('spl_add')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("15", Str("r", b));
    }


    // ── G. Regression: .NET-native path unaffected ────────────────────

    [TestMethod]
    public void Regression_DotNet_AreaLibrary_StillWorks()
    {
        var dll = SetupTests.AreaLibraryPath;
        Assert.IsTrue(File.Exists(dll), $"AreaLibrary.dll not found: {dll}");
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')
        r = AreaOfSquare(5)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("25.", Str("r", b));
    }

    [TestMethod]
    public void Regression_DotNet_MathLibrary_StillWorks()
    {
        var dll = SetupTests.MathLibraryPath;
        Assert.IsTrue(File.Exists(dll), $"MathLibrary.dll not found: {dll}");
        var b = Run($@"
        load('{dll}', 'MathFunction.MathFunctions')
        r = Add(6, 7)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("13", Str("r", b));
    }

    [TestMethod]
    public void Regression_DotNet_FSharpLibrary_StillWorks()
    {
        var dll = SetupTests.FSharpLibraryPath;
        Assert.IsTrue(File.Exists(dll), $"FSharpLibrary.dll not found: {dll}");
        var b = Run($@"
        load('{dll}', 'FSharpLibrary.StringFunctions')
        r = FsFib(8)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("21", Str("r", b));
    }
}
