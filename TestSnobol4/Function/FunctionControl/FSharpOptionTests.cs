using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// Step 8 acceptance tests: F# option&lt;T&gt; and discriminated union coercion
/// via the reflection path (auto-prototype / ::MethodName binding).
///
/// FSharpOptionLibrary contains plain F# classes (no IExternalLibrary) so
/// LOAD routes through LoadDotNetPath → CallReflectFunction.
///
/// Coercion rules exercised:
///   option — Some(v) → value pushed normally
///           None     → SNOBOL4 failure branch (:F)
///   DU     — rendered as "CaseName [field1 [field2 ...]]" StringVar
/// </summary>
[TestClass]
public class FSharpOptionTests
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    private static string Str(string name, Builder b) =>
        b.Execute!.IdentifierTable[b.FoldCase(name)].ToString();

    // ── IntOption ──────────────────────────────────────────────────────────

    [TestMethod]
    public void FSharp_Option_ParseInt_Some()
    {
        // ParseInt("42") → Some(42) → IntegerVar 42
        var dll = SetupTests.FSharpOptionLibraryPath;
        Assert.IsTrue(File.Exists(dll), $"FSharpOptionLibrary.dll not found: {dll}");
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.IntOption::ParseInt')
        r = ParseInt('42')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("42", Str("r", b));
    }

    [TestMethod]
    public void FSharp_Option_ParseInt_None_Fails()
    {
        // ParseInt("abc") → None → :F branch
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.IntOption::ParseInt')
        ParseInt('abc')     :S(OK)F(FAIL)
FAIL    result = 'fail'     :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("fail", Str("result", b));
    }

    [TestMethod]
    public void FSharp_Option_SafeDiv_Some()
    {
        // SafeDiv(10, 3) → Some(3) → IntegerVar 3
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.IntOption::SafeDiv')
        r = SafeDiv(10, 3)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("3", Str("r", b));
    }

    [TestMethod]
    public void FSharp_Option_SafeDiv_DivByZero_Fails()
    {
        // SafeDiv(10, 0) → None → :F branch
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.IntOption::SafeDiv')
        SafeDiv(10, 0)      :S(OK)F(FAIL)
FAIL    result = 'fail'     :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("fail", Str("result", b));
    }

    // ── StringOption ───────────────────────────────────────────────────────

    [TestMethod]
    public void FSharp_Option_FirstWord_Some()
    {
        // FirstWord("hello world") → Some("hello") → StringVar "hello"
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.StringOption::FirstWord')
        r = FirstWord('hello world')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("hello", Str("r", b));
    }

    [TestMethod]
    public void FSharp_Option_FirstWord_EmptyString_Fails()
    {
        // FirstWord("") → None → :F branch
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.StringOption::FirstWord')
        FirstWord('')       :S(OK)F(FAIL)
FAIL    result = 'fail'     :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("fail", Str("result", b));
    }

    [TestMethod]
    public void FSharp_Option_LookupColor_Known()
    {
        // LookupColor("red") → Some("#FF0000")
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.StringOption::LookupColor')
        r = LookupColor('red')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("#FF0000", Str("r", b));
    }

    [TestMethod]
    public void FSharp_Option_LookupColor_Unknown_Fails()
    {
        // LookupColor("purple") → None → :F branch
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.StringOption::LookupColor')
        LookupColor('purple')   :S(OK)F(FAIL)
FAIL    result = 'fail'         :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("fail", Str("result", b));
    }

    // ── Shape DU ───────────────────────────────────────────────────────────

    [TestMethod]
    public void FSharp_DU_Shape_Circle()
    {
        // MakeCircle(3.5) → Circle 3.5 → StringVar "Circle 3.5"
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.ShapeFactory::MakeCircle')
        r = MakeCircle(3.5)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("Circle 3.5", Str("r", b));
    }

    [TestMethod]
    public void FSharp_DU_Shape_Rectangle()
    {
        // MakeRect(2.0, 4.0) → Rectangle(2,4) → StringVar "Rectangle 2 4"
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.ShapeFactory::MakeRect')
        r = MakeRect(2.0, 4.0)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        // F# tuple fields render as "(2, 4)" via FSharpValue — accept either format
        var result = Str("r", b);
        Assert.IsTrue(result.StartsWith("Rectangle"), $"Expected 'Rectangle ...' but got '{result}'");
    }

    [TestMethod]
    public void FSharp_DU_Shape_Point_ZeroFields()
    {
        // MakePoint() → Point → StringVar "Point"
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.ShapeFactory::MakePoint')
        r = MakePoint()
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("Point", Str("r", b));
    }

    // ── Outcome DU ─────────────────────────────────────────────────────────

    [TestMethod]
    public void FSharp_DU_Outcome_OkVal()
    {
        // Succeed("done") → OkVal "done" → StringVar "OkVal \"done\""
        // F# renders string fields with quotes in ToString()
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.OutcomeFactory::Succeed')
        r = Succeed('done')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var result = Str("r", b);
        Assert.IsTrue(result.StartsWith("OkVal"), $"Expected OkVal ... but got '{result}'");
        Assert.IsTrue(result.Contains("done"), $"Expected to contain 'done' but got '{result}'");
    }

    [TestMethod]
    public void FSharp_DU_Outcome_ErrVal()
    {
        // Fail("oops") → ErrVal "oops" → StringVar "ErrVal \"oops\""
        var dll = SetupTests.FSharpOptionLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpOptionLibrary.OutcomeFactory::Fail')
        r = Fail('oops')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var result = Str("r", b);
        Assert.IsTrue(result.StartsWith("ErrVal"), $"Expected ErrVal ... but got '{result}'");
        Assert.IsTrue(result.Contains("oops"), $"Expected to contain 'oops' but got '{result}'");
    }

    // ── Regression: existing F# IExternalLibrary path unaffected ──────────

    [TestMethod]
    public void FSharp_IExternalLibrary_StillWorks_After_Step8()
    {
        // The IExternalLibrary fast path must not be broken by the coercion changes
        var dll = SetupTests.FSharpLibraryPath;
        Assert.IsTrue(File.Exists(dll), $"FSharpLibrary.dll not found: {dll}");
        var b = Run($@"
        load('{dll}', 'FSharpLibrary.StringFunctions')
        r = FsFib(8)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("21", Str("r", b));
    }
}
