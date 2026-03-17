using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// Tests for net-load-dotnet Steps 2 and 3:
///   Step 2 — Auto-prototype: LOAD('dll', 'Ns.Class') reflects the single
///             public method and registers it by method name.
///   Step 3 — Explicit binding: LOAD('dll', 'Ns.Class::Method') picks a
///             named method from a multi-method class.
/// ReflectLibrary has no IExternalLibrary — all registration is via reflection.
/// </summary>
[TestClass]
public class LoadAutoPrototypeTests
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    private static string Str(string name, Builder b) =>
        b.Execute!.IdentifierTable[b.FoldCase(name)].ToString();

    private static string Dll => SetupTests.ReflectLibraryPath;

    // ── Step 2: auto-prototype — single public method discovered ─────────

    [TestMethod]
    public void AutoProto_Doubler_IntegerReturn()
    {
        // Doubler has one method: Double(long) → long
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Doubler')
        result = Double(21)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("42", Str("result", b));
    }

    [TestMethod]
    public void AutoProto_Greeter_StaticStringMethod()
    {
        // Greeter has one static method: Greet(string) → string
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Greeter')
        result = Greet('World')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("Hello, World!", Str("result", b));
    }

    [TestMethod]
    public void AutoProto_SuccessBranch()
    {
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Doubler')   :S(OK)F(FAIL)
FAIL    result = 'failed'                           :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual("ok", Str("result", b));
    }

    [TestMethod]
    public void AutoProto_FailureBranch_ClassNotFound()
    {
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.NoSuchClass')   :S(OK)F(FAIL)
FAIL    result = 'failed'                               :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual("failed", Str("result", b));
    }

    [TestMethod]
    public void AutoProto_FailureBranch_AmbiguousMultiMethod()
    {
        // Calculator has two public methods — auto-prototype without :: must fail
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Calculator')   :S(OK)F(FAIL)
FAIL    result = 'failed'                              :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual("failed", Str("result", b));
    }

    [TestMethod]
    public void AutoProto_Idempotent_DoubleLoad()
    {
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Doubler')
        load('{Dll}', 'ReflectFunction.Doubler')
        result = Double(7)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("14", Str("result", b));
    }

    [TestMethod]
    public void AutoProto_UnloadByFname()
    {
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Doubler')
        r1 = Double(5)
        unload('Double')
        r2 = r1
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("10", Str("r1", b));
    }

    [TestMethod]
    public void AutoProto_Formatter_MixedArgs()
    {
        // Formatter.Format(string, long) → string
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Formatter')
        result = Format('count', 99)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("count=99", Str("result", b));
    }

    // ── Step 3: explicit ::MethodName binding ─────────────────────────────

    [TestMethod]
    public void ExplicitBinding_Calculator_Square()
    {
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Calculator::Square')
        result = Square(9.0)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("81.", Str("result", b));
    }

    [TestMethod]
    public void ExplicitBinding_Calculator_Cube()
    {
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Calculator::Cube')
        result = Cube(3.0)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("27.", Str("result", b));
    }

    [TestMethod]
    public void ExplicitBinding_BothMethodsFromSameDll()
    {
        // Load Square and Cube from the same DLL in two calls
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Calculator::Square')
        load('{Dll}', 'ReflectFunction.Calculator::Cube')
        r1 = Square(4.0)
        r2 = Cube(2.0)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("16.", Str("r1", b));
        Assert.AreEqual("8.",  Str("r2", b));
    }

    [TestMethod]
    public void ExplicitBinding_FailureBranch_MethodNotFound()
    {
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Calculator::NoSuchMethod')   :S(OK)F(FAIL)
FAIL    result = 'failed'                                            :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual("failed", Str("result", b));
    }

    [TestMethod]
    public void ExplicitBinding_UnloadByFname()
    {
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Calculator::Square')
        r1 = Square(6.0)
        unload('Square')
        r2 = r1
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("36.", Str("r1", b));
    }

    // ── Coexistence: IExternalLibrary and auto-prototype in same program ──

    [TestMethod]
    public void AutoProto_CoexistsWithIExternalLibrary()
    {
        var mathDll = SetupTests.MathLibraryPath;
        var b = Run($@"
        load('{mathDll}', 'MathFunction.MathFunctions')
        load('{Dll}', 'ReflectFunction.Doubler')
        r1 = Add(10, 5)
        r2 = Double(7)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("15",  Str("r1", b));
        Assert.AreEqual("14",  Str("r2", b));
    }

    // ── Step 4: ref-count ActiveContexts by DLL path ──────────────────────

    [TestMethod]
    public void RefCount_TwoFunctionsFromSameDll_BothWork()
    {
        // Load Square and Cube from the same DLL — only one ALC should be created.
        // Both functions must work correctly after both LOADs.
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Calculator::Square')
        load('{Dll}', 'ReflectFunction.Calculator::Cube')
        r1 = Square(5.0)
        r2 = Cube(3.0)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("25.", Str("r1", b));
        Assert.AreEqual("27.", Str("r2", b));
    }

    [TestMethod]
    public void RefCount_UnloadFirst_SecondStillWorks()
    {
        // UNLOAD(Square) should decrement count to 1 — Cube must still be callable.
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Calculator::Square')
        load('{Dll}', 'ReflectFunction.Calculator::Cube')
        r1 = Square(4.0)
        unload('Square')
        r2 = Cube(2.0)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("16.", Str("r1", b));
        Assert.AreEqual("8.",  Str("r2", b));
    }

    [TestMethod]
    public void RefCount_UnloadBoth_AlcReleased()
    {
        // UNLOAD both — ref-count reaches zero, ALC released.
        // Neither function should be callable after both UNLOADs.
        // (We just verify no error during UNLOAD and results before UNLOAD are correct.)
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Calculator::Square')
        load('{Dll}', 'ReflectFunction.Calculator::Cube')
        r1 = Square(3.0)
        r2 = Cube(2.0)
        unload('Square')
        unload('Cube')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("9.",  Str("r1", b));
        Assert.AreEqual("8.",  Str("r2", b));
    }

    [TestMethod]
    public void RefCount_ThreeFunctionsFromSameDll_UnloadInOrder()
    {
        // Load Doubler, Square, Cube from same DLL — three FNAMEs, ref-count=3.
        // Unload one at a time; remaining functions continue to work.
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Doubler')
        load('{Dll}', 'ReflectFunction.Calculator::Square')
        load('{Dll}', 'ReflectFunction.Calculator::Cube')
        r1 = Double(6)
        r2 = Square(4.0)
        unload('Double')
        r3 = Cube(3.0)
        unload('Square')
        r4 = r3
        unload('Cube')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("12",  Str("r1", b));
        Assert.AreEqual("16.", Str("r2", b));
        Assert.AreEqual("27.", Str("r3", b));
    }

    [TestMethod]
    public void RefCount_ReloadAfterFullUnload_Works()
    {
        // After all FNAMEs from a DLL are unloaded (ref=0), a fresh LOAD should succeed.
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Doubler')
        r1 = Double(7)
        unload('Double')
        load('{Dll}', 'ReflectFunction.Doubler')
        r2 = Double(9)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("14", Str("r1", b));
        Assert.AreEqual("18", Str("r2", b));
    }

    // ── Step 5: async Task<T> return — blocking-await adapter ─────────────

    [TestMethod]
    public void Async_TaskLong_BlocksAndReturnsResult()
    {
        // AsyncDoubler.AsyncDouble(long) → Task<long>; SNOBOL4 call should block.
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.AsyncDoubler')
        result = AsyncDouble(21)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("42", Str("result", b));
    }

    [TestMethod]
    public void Async_TaskString_BlocksAndReturnsResult()
    {
        // AsyncGreeter.AsyncGreet(string) → Task<string>.
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.AsyncGreeter')
        result = AsyncGreet('World')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("Hello async, World!", Str("result", b));
    }

    [TestMethod]
    public void Async_NonGenericTask_ReturnsNull()
    {
        // AsyncVoidWorker.AsyncVoid(string) → Task (non-generic) → mapped to null → empty string.
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.AsyncVoidWorker')
        result = 'before'
        AsyncVoid('test')
        result = 'after'
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("after", Str("result", b));
    }

    [TestMethod]
    public void Async_CoexistsWithSync_SameDll()
    {
        // Mix sync (Doubler) and async (AsyncDoubler) from same DLL via ref-count.
        var b = Run($@"
        load('{Dll}', 'ReflectFunction.Doubler')
        load('{Dll}', 'ReflectFunction.AsyncDoubler')
        r1 = Double(10)
        r2 = AsyncDouble(10)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("20", Str("r1", b));
        Assert.AreEqual("20", Str("r2", b));
    }
}
