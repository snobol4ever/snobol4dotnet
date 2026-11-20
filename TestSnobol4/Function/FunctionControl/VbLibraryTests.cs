using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// M-NET-VB acceptance tests: VB.NET library loaded via the reflection path
/// (auto-prototype and ::MethodName binding).
///
/// VbLibrary contains plain VB.NET classes (no IExternalLibrary) so
/// LOAD routes through LoadDotNetPath → CallReflectFunction.
///
/// Coverage:
///   A. String return   — Reverser (auto-prototype, instance method)
///   B. Long return     — Arithmetic::Factorial, Arithmetic::Sum (explicit binding)
///   C. Double return   — Geometry::CircleArea (explicit binding)
///   D. Null → fail     — Predicate::NonEmptyOrFail (null return → :F branch)
///   E. Static method   — Formatter::Format (shared method, no instance)
///   F. Multi-load      — two LOAD calls to same DLL, two functions active
///   G. UNLOAD          — unload by DLL path, function removed
/// </summary>
[TestClass]
public class VbLibraryTests
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    private static string Str(string name, Builder b) =>
        b.Execute!.IdentifierTable[b.FoldCase(name)].ToString();

    // ── A. String return (auto-prototype) ─────────────────────────────────

    [TestMethod]
    public void Vb_AutoPrototype_Reverse()
    {
        // Reverser has one public method: Reverse(String) → String
        // Auto-prototype discovers it; function name = "Reverse"
        var dll = SetupTests.VbLibraryPath;
        Assert.IsTrue(File.Exists(dll), $"VbLibrary.dll not found: {dll}");
        var b = Run($@"
        load('{dll}', 'VbLibrary.Reverser')
        r = Reverse('hello')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("olleh", Str("r", b));
    }

    [TestMethod]
    public void Vb_AutoPrototype_Reverse_EmptyString()
    {
        var dll = SetupTests.VbLibraryPath;
        var b = Run($@"
        load('{dll}', 'VbLibrary.Reverser')
        r = Reverse('')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("", Str("r", b));
    }

    // ── B. Long return (explicit ::MethodName binding) ────────────────────

    [TestMethod]
    public void Vb_Explicit_Factorial()
    {
        // Arithmetic has two methods — auto-prototype would fail (ambiguous)
        // ::Factorial selects the one we want
        var dll = SetupTests.VbLibraryPath;
        var b = Run($@"
        load('{dll}', 'VbLibrary.Arithmetic::Factorial')
        r = Factorial(6)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("720", Str("r", b));
    }

    [TestMethod]
    public void Vb_Explicit_Sum()
    {
        var dll = SetupTests.VbLibraryPath;
        var b = Run($@"
        load('{dll}', 'VbLibrary.Arithmetic::Sum')
        r = Sum(17, 25)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("42", Str("r", b));
    }

    // ── C. Double return ──────────────────────────────────────────────────

    [TestMethod]
    public void Vb_Explicit_CircleArea()
    {
        // CircleArea(1.0) = π ≈ 3.14159...
        var dll = SetupTests.VbLibraryPath;
        var b = Run($@"
        load('{dll}', 'VbLibrary.Geometry::CircleArea')
        r = CircleArea(1.0)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var result = double.Parse(Str("r", b));
        Assert.AreEqual(Math.PI, result, 1e-10);
    }

    // ── D. Null → fail branch ─────────────────────────────────────────────

    [TestMethod]
    public void Vb_NullReturn_TriggersFailBranch()
    {
        // NonEmptyOrFail("") returns Nothing (null) → :F
        var dll = SetupTests.VbLibraryPath;
        var b = Run($@"
        load('{dll}', 'VbLibrary.Predicate::NonEmptyOrFail')
        NonEmptyOrFail('')  :S(OK)F(FAIL)
FAIL    result = 'fail'     :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("fail", Str("result", b));
    }

    [TestMethod]
    public void Vb_NullReturn_SomethingSucceeds()
    {
        // NonEmptyOrFail("hello") returns "hello" → :S
        var dll = SetupTests.VbLibraryPath;
        var b = Run($@"
        load('{dll}', 'VbLibrary.Predicate::NonEmptyOrFail')
        NonEmptyOrFail('hello')  :S(OK)F(FAIL)
FAIL    result = 'fail'          :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }

    // ── E. Static (Shared) method ─────────────────────────────────────────

    [TestMethod]
    public void Vb_Static_Formatter()
    {
        // Format is a Shared (static) method — no instance created
        var dll = SetupTests.VbLibraryPath;
        var b = Run($@"
        load('{dll}', 'VbLibrary.Formatter::Format')
        r = Format('score', 99)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("score=99", Str("r", b));
    }

    // ── F. Multi-load from same DLL ───────────────────────────────────────

    [TestMethod]
    public void Vb_MultiLoad_TwoFunctionsFromSameDll()
    {
        // Two LOAD calls to the same DLL → ref-count 2, both functions active
        var dll = SetupTests.VbLibraryPath;
        var b = Run($@"
        load('{dll}', 'VbLibrary.Arithmetic::Factorial')
        load('{dll}', 'VbLibrary.Arithmetic::Sum')
        r1 = Factorial(5)
        r2 = Sum(3, 4)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("120", Str("r1", b));
        Assert.AreEqual("7",   Str("r2", b));
    }

    // ── G. UNLOAD ─────────────────────────────────────────────────────────

    [TestMethod]
    public void Vb_Unload_RemovesFunction()
    {
        // After UNLOAD the function is gone; calling it produces error 22 (undefined function).
        // Error 22 is a fatal runtime error, not a predicate failure, so we verify ErrorCodeHistory.
        var dll = SetupTests.VbLibraryPath;
        var b = Run($@"
        load('{dll}', 'VbLibrary.Reverser')
        r1 = Reverse('ab')
        unload('{dll}')
        r2 = Reverse('cd')
end");
        Assert.AreEqual("ba", Str("r1", b));
        Assert.AreEqual(1, b.ErrorCodeHistory.Count);
        Assert.AreEqual(22, b.ErrorCodeHistory[0]);
    }
}
