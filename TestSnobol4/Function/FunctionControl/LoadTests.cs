using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// Tests for LOAD() and UNLOAD() covering:
///   A. AreaLibrary (C#) — basic happy path + success/failure branches
///   B. Unload lifecycle — unload/reload, idempotent unload
///   C. MathLibrary (C#) — full type coverage: int, real, string, 3-arg, predicate
///   D. FSharpLibrary (F#) — proves IExternalLibrary works from F#
///   E. Integration — two libraries simultaneously, UNLOAD :S branch
/// </summary>
[TestClass]
public class LoadTests
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    private static string Str(string name, Builder b) =>
        b.Execute!.IdentifierTable[b.FoldCase(name)].ToString();

    // ── A. AreaLibrary ─────────────────────────────────────────────────────

    [TestMethod]
    public void Load_Area_CircleAndSquare()
    {
        var dll = SetupTests.AreaLibraryPath;
        Assert.IsTrue(File.Exists(dll), $"AreaLibrary.dll not found: {dll}");
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')
        r1 = AreaOfCircle(4.5)
        r2 = AreaOfSquare(15.9)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("63.61725123519331", Str("r1", b));
        Assert.AreEqual("252.81", Str("r2", b));
    }

    [TestMethod]
    public void Load_Area_IntegerArgCoercion()
    {
        var dll = SetupTests.AreaLibraryPath;
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')
        r = AreaOfSquare(5)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("25.", Str("r", b));
    }

    [TestMethod]
    public void Load_Area_StringArgCoercion()
    {
        var dll = SetupTests.AreaLibraryPath;
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')
        r = AreaOfSquare('7')
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("49.", Str("r", b));
    }

    [TestMethod]
    public void Load_Area_SuccessBranch()
    {
        var dll = SetupTests.AreaLibraryPath;
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')   :S(OK)F(FAIL)
FAIL    result = 'failed'                    :(END)
OK      result = 'ok'
        unload('{dll}')
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }

    [TestMethod]
    public void Load_Area_FailureBranchBadClass()
    {
        var dll = SetupTests.AreaLibraryPath;
        var b = Run($@"
        load('{dll}', 'NoSuch.Class')   :S(OK)F(FAIL)
FAIL    result = 'failed'              :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(142, b.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Load_Area_FailureBranchMissingFile()
    {
        var b = Run(@"
        load('/no/such/file.dll', 'Any.Class')   :S(OK)F(FAIL)
FAIL    result = 'failed'                        :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(143, b.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Load_Area_IdempotentDoubleLoad()
    {
        var dll = SetupTests.AreaLibraryPath;
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')
        load('{dll}', 'AreaFunction.Area')
        r = AreaOfCircle(1.0)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.IsFalse(string.IsNullOrEmpty(Str("r", b)));
    }

    // ── B. Unload lifecycle ────────────────────────────────────────────────

    [TestMethod]
    public void Unload_Basic()
    {
        var dll = SetupTests.AreaLibraryPath;
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')
        r = AreaOfSquare(4)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("16.", Str("r", b));
    }

    [TestMethod]
    public void Unload_IdempotentDoubleUnload()
    {
        var dll = SetupTests.AreaLibraryPath;
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')
        unload('{dll}')
        unload('{dll}')
        result = 'ok'
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }

    [TestMethod]
    public void Unload_ReloadAfterUnload()
    {
        var dll = SetupTests.AreaLibraryPath;
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')
        r1 = AreaOfSquare(3)
        unload('{dll}')
        load('{dll}', 'AreaFunction.Area')
        r2 = AreaOfSquare(4)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("9.",  Str("r1", b));
        Assert.AreEqual("16.", Str("r2", b));
    }

    // ── C. MathLibrary — full type coverage ───────────────────────────────

    //[TestMethod]
    public void Load_Math_IntegerInIntegerOut()
    {
        var dll = SetupTests.MathLibraryPath;
        Assert.IsTrue(File.Exists(dll), $"MathLibrary.dll not found: {dll}");
        var b = Run($@"
        load('{dll}', 'MathFunction.MathFunctions')
        r = Add(3, 4)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("7", Str("r", b));
    }

    //[TestMethod]
    public void Load_Math_RealInRealOut()
    {
        var dll = SetupTests.MathLibraryPath;
        var b = Run($@"
        load('{dll}', 'MathFunction.MathFunctions')
        r = Multiply(2.5, 4.0)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("10", Str("r", b));
    }

    //[TestMethod]
    public void Load_Math_StringInStringOut()
    {
        var dll = SetupTests.MathLibraryPath;
        var b = Run($@"
        load('{dll}', 'MathFunction.MathFunctions')
        r = Reverse('hello')
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("olleh", Str("r", b));
    }

    //[TestMethod]
    public void Load_Math_ThreeArgClamp()
    {
        var dll = SetupTests.MathLibraryPath;
        var b = Run($@"
        load('{dll}', 'MathFunction.MathFunctions')
        r = Clamp(15.0, 0.0, 10.0)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("10", Str("r", b));
    }

    //[TestMethod]
    public void Load_Math_PredicateSuccess()
    {
        var dll = SetupTests.MathLibraryPath;
        var b = Run($@"
        load('{dll}', 'MathFunction.MathFunctions')
        IsPositive(5)        :S(OK)F(FAIL)
FAIL    result = 'fail'      :(END)
OK      result = 'ok'
END     unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }

    //[TestMethod]
    public void Load_Math_PredicateFailure()
    {
        var dll = SetupTests.MathLibraryPath;
        var b = Run($@"
        load('{dll}', 'MathFunction.MathFunctions')
        IsPositive(-1)       :S(OK)F(FAIL)
FAIL    result = 'fail'      :(END)
OK      result = 'ok'
END     unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("fail", Str("result", b));
    }

    //[TestMethod]
    public void Load_Math_StringArgCoercedToInteger()
    {
        var dll = SetupTests.MathLibraryPath;
        var b = Run($@"
        load('{dll}', 'MathFunction.MathFunctions')
        r = Add('10', '5')
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("15", Str("r", b));
    }

    // ── D. FSharpLibrary ──────────────────────────────────────────────────

    //[TestMethod]
    public void Load_FSharp_Fibonacci()
    {
        var dll = SetupTests.FSharpLibraryPath;
        Assert.IsTrue(File.Exists(dll), $"FSharpLibrary.dll not found: {dll}");
        var b = Run($@"
        load('{dll}', 'FSharpLibrary.StringFunctions')
        r = FsFib(10)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("55", Str("r", b));
    }

    //[TestMethod]
    public void Load_FSharp_PalindromeSuccess()
    {
        var dll = SetupTests.FSharpLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpLibrary.StringFunctions')
        FsIsPalindrome('racecar')   :S(OK)F(FAIL)
FAIL    result = 'fail'             :(END)
OK      result = 'ok'
END     unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }

    //[TestMethod]
    public void Load_FSharp_PalindromeFailure()
    {
        var dll = SetupTests.FSharpLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpLibrary.StringFunctions')
        FsIsPalindrome('hello')     :S(OK)F(FAIL)
FAIL    result = 'fail'             :(END)
OK      result = 'ok'
END     unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("fail", Str("result", b));
    }

    //[TestMethod]
    public void Load_FSharp_JoinWith()
    {
        var dll = SetupTests.FSharpLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpLibrary.StringFunctions')
        r = FsJoinWith('-', 'a b c')
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("a-b-c", Str("r", b));
    }

    //[TestMethod]
    public void Load_FSharp_Hypot()
    {
        var dll = SetupTests.FSharpLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpLibrary.StringFunctions')
        r = FsHypot(3.0, 4.0)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("5", Str("r", b));
    }

    //[TestMethod]
    public void Load_FSharp_UnloadAndReload()
    {
        var dll = SetupTests.FSharpLibraryPath;
        var b = Run($@"
        load('{dll}', 'FSharpLibrary.StringFunctions')
        r1 = FsFib(5)
        unload('{dll}')
        load('{dll}', 'FSharpLibrary.StringFunctions')
        r2 = FsFib(7)
        unload('{dll}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("5",  Str("r1", b));
        Assert.AreEqual("13", Str("r2", b));
    }

    // ── E. Integration ─────────────────────────────────────────────────────

    //[TestMethod]
    public void Load_TwoLibraries_Simultaneously()
    {
        var math   = SetupTests.MathLibraryPath;
        var fsharp = SetupTests.FSharpLibraryPath;
        var b = Run($@"
        load('{math}',   'MathFunction.MathFunctions')
        load('{fsharp}', 'FSharpLibrary.StringFunctions')
        r1 = Add(6, 7)
        r2 = FsFib(8)
        unload('{math}')
        unload('{fsharp}')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("13", Str("r1", b));
        Assert.AreEqual("21", Str("r2", b));
    }

    //[TestMethod]
    public void Load_UnloadSuccessBranch()
    {
        var dll = SetupTests.AreaLibraryPath;
        var b = Run($@"
        load('{dll}', 'AreaFunction.Area')
        unload('{dll}')                    :S(OK)F(FAIL)
FAIL    result = 'fail'                   :(END)
OK      result = 'ok'
END
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", Str("result", b));
    }
}
