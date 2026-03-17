using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// Step 7 — net-load-dotnet: SnobolVar/Pattern/Table/Array return coercions
/// via IExternalLibrary fast path.
///
/// ObjectLifecycleLibrary exposes 15 functions across 3 groups:
///   ARRAY: MakeArray, ArraySet, ArrayGet, ArraySum, ArrayClear
///   TABLE: MakeTable, TablePut, TableGet, TableKeys, TableWipe
///   POINT: MakePoint, PointX, PointY, PointMove, PointReset
///
/// Each test verifies that a native C# function returning ArrayVar or TableVar
/// directly passes the object through to SNOBOL4 with zero coercion overhead.
/// </summary>
[TestClass]
public class LoadObjectLifecycleTests
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    private static string Str(string name, Builder b) =>
        b.Execute!.IdentifierTable[b.FoldCase(name)].ToString();

    private static string Dll => SetupTests.ObjectLifecycleLibraryPath;

    private static string LoadPreamble => $"        load('{Dll}', 'ObjectLifecycle.ObjectLifecycleFunctions')";

    // ── ARRAY: MakeArray ──────────────────────────────────────────────────

    [TestMethod]
    public void MakeArray_ReturnsDatatypeArray()
    {
        // MakeArray returns an ArrayVar — DATATYPE should report 'ARRAY'
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(3)
        result = DATATYPE(a)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("array", Str("result", b));
    }

    [TestMethod]
    public void MakeArray_SizeReflectedInPrototype()
    {
        // PROTOTYPE of a 5-element array should be '5'
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(5)
        result = PROTOTYPE(a)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("5", Str("result", b));
    }

    // ── ARRAY: ArraySet / ArrayGet ────────────────────────────────────────

    [TestMethod]
    public void ArraySet_ArrayGet_RoundTrip()
    {
        // Set slot 2 to 42, get it back
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(3)
        ArraySet(a, 2, 42)
        result = ArrayGet(a, 2)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("42", Str("result", b));
    }

    [TestMethod]
    public void ArraySet_Returns_SameArray()
    {
        // ArraySet returns the array — chain: set then read prototype
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(3)
        b = ArraySet(a, 1, 'hello')
        result = PROTOTYPE(b)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("3", Str("result", b));
    }

    [TestMethod]
    public void ArrayGet_UnsetSlot_ReturnsEmptyString()
    {
        // Unset slots were filled with "" at construction
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(4)
        result = ArrayGet(a, 3)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("", Str("result", b));
    }

    // ── ARRAY: ArraySum ───────────────────────────────────────────────────

    [TestMethod]
    public void ArraySum_SumsIntegerSlots()
    {
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(3)
        ArraySet(a, 1, 10)
        ArraySet(a, 2, 20)
        ArraySet(a, 3, 30)
        result = ArraySum(a)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("60", Str("result", b));
    }

    [TestMethod]
    public void ArraySum_NonIntegerSlotsCountAsZero()
    {
        // Slot 3 is a string — should not blow up, just counts as 0
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(3)
        ArraySet(a, 1, 5)
        ArraySet(a, 2, 10)
        ArraySet(a, 3, 'hello')
        result = ArraySum(a)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("15", Str("result", b));
    }

    // ── ARRAY: ArrayClear ─────────────────────────────────────────────────

    [TestMethod]
    public void ArrayClear_ResetsAllSlots()
    {
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(3)
        ArraySet(a, 1, 99)
        ArraySet(a, 2, 88)
        ArrayClear(a)
        r1 = ArrayGet(a, 1)
        r2 = ArrayGet(a, 2)
        result = ArraySum(a)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("",  Str("r1",     b));
        Assert.AreEqual("",  Str("r2",     b));
        Assert.AreEqual("0", Str("result", b));
    }

    // ── TABLE: MakeTable ──────────────────────────────────────────────────

    [TestMethod]
    public void MakeTable_ReturnsDatatypeTable()
    {
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        result = DATATYPE(t)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("table", Str("result", b));
    }

    // ── TABLE: TablePut / TableGet ────────────────────────────────────────

    [TestMethod]
    public void TablePut_TableGet_StringKey_RoundTrip()
    {
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        TablePut(t, 'name', 'Alice')
        result = TableGet(t, 'name')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("Alice", Str("result", b));
    }

    [TestMethod]
    public void TablePut_TableGet_IntegerKey_RoundTrip()
    {
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        TablePut(t, 42, 'answer')
        result = TableGet(t, 42)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("answer", Str("result", b));
    }

    [TestMethod]
    public void TableGet_MissingKey_ReturnsFill()
    {
        // Fill is "" — missing key should return empty string
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        result = TableGet(t, 'nosuchkey')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("", Str("result", b));
    }

    [TestMethod]
    public void TablePut_MultipleKeys()
    {
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        TablePut(t, 'a', '1')
        TablePut(t, 'b', '2')
        TablePut(t, 'c', '3')
        r1 = TableGet(t, 'a')
        r2 = TableGet(t, 'b')
        r3 = TableGet(t, 'c')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("1", Str("r1", b));
        Assert.AreEqual("2", Str("r2", b));
        Assert.AreEqual("3", Str("r3", b));
    }

    // ── TABLE: TableKeys ──────────────────────────────────────────────────

    [TestMethod]
    public void TableKeys_EmptyTable_ReturnsZero()
    {
        // Empty table — TableKeys returns IntegerVar(0) sentinel
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        result = TableKeys(t)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("0", Str("result", b));
    }

    [TestMethod]
    public void TableKeys_PopulatedTable_ReturnsArray()
    {
        // 2 keys → ArrayVar of size 2; PROTOTYPE should be '2'
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        TablePut(t, 'x', 10)
        TablePut(t, 'y', 20)
        keys = TableKeys(t)
        result = PROTOTYPE(keys)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("2", Str("result", b));
    }

    // ── TABLE: TableWipe ──────────────────────────────────────────────────

    [TestMethod]
    public void TableWipe_ClearsAllEntries()
    {
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        TablePut(t, 'a', 1)
        TablePut(t, 'b', 2)
        TableWipe(t)
        r1 = TableGet(t, 'a')
        r2 = TableKeys(t)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("",  Str("r1", b));   // fill after wipe
        Assert.AreEqual("0", Str("r2", b));   // 0 keys after wipe
    }

    // ── POINT: MakePoint / PointX / PointY ───────────────────────────────

    [TestMethod]
    public void MakePoint_ReturnsArray()
    {
        // Point is backed by 2-slot ArrayVar
        var b = Run($@"
{LoadPreamble}
        p = MakePoint(3, 7)
        result = DATATYPE(p)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("array", Str("result", b));
    }

    [TestMethod]
    public void PointX_ReturnsXCoordinate()
    {
        var b = Run($@"
{LoadPreamble}
        p = MakePoint(3, 7)
        result = PointX(p)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("3", Str("result", b));
    }

    [TestMethod]
    public void PointY_ReturnsYCoordinate()
    {
        var b = Run($@"
{LoadPreamble}
        p = MakePoint(3, 7)
        result = PointY(p)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("7", Str("result", b));
    }

    // ── POINT: PointMove ──────────────────────────────────────────────────

    [TestMethod]
    public void PointMove_ReturnsNewPointWithOffset()
    {
        var b = Run($@"
{LoadPreamble}
        p = MakePoint(3, 7)
        p2 = PointMove(p, 2, -3)
        rx = PointX(p2)
        ry = PointY(p2)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("5",  Str("rx", b));
        Assert.AreEqual("4",  Str("ry", b));
    }

    [TestMethod]
    public void PointMove_OriginalUnchanged()
    {
        // PointMove returns a NEW array — original should be untouched
        var b = Run($@"
{LoadPreamble}
        p = MakePoint(10, 20)
        p2 = PointMove(p, 5, 5)
        ox = PointX(p)
        oy = PointY(p)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("10", Str("ox", b));
        Assert.AreEqual("20", Str("oy", b));
    }

    // ── POINT: PointReset ─────────────────────────────────────────────────

    [TestMethod]
    public void PointReset_ReturnsOrigin()
    {
        var b = Run($@"
{LoadPreamble}
        p = MakePoint(99, 77)
        p0 = PointReset(p)
        rx = PointX(p0)
        ry = PointY(p0)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("0", Str("rx", b));
        Assert.AreEqual("0", Str("ry", b));
    }

    // ── Return type: ArrayVar passes through without StringVar coercion ───

    [TestMethod]
    public void ArrayReturn_IsNotStringCoerced()
    {
        // If the Var v => v arm is missing and we fall through to ToString(),
        // DATATYPE would return 'STRING' instead of 'ARRAY'.
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(2)
        result = DATATYPE(a)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        // This passes only if CallReflectFunction preserves the ArrayVar
        Assert.AreNotEqual("string", Str("result", b));
        Assert.AreEqual("array", Str("result", b));
    }

    [TestMethod]
    public void TableReturn_IsNotStringCoerced()
    {
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        result = DATATYPE(t)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreNotEqual("string", Str("result", b));
        Assert.AreEqual("table", Str("result", b));
    }

    // ── Full lifecycle: create → populate → use → clear ───────────────────

    [TestMethod]
    public void FullLifecycle_Array()
    {
        var b = Run($@"
{LoadPreamble}
        a = MakeArray(4)
        ArraySet(a, 1, 10)
        ArraySet(a, 2, 20)
        ArraySet(a, 3, 30)
        ArraySet(a, 4, 40)
        sum1 = ArraySum(a)
        ArrayClear(a)
        sum2 = ArraySum(a)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("100", Str("sum1", b));
        Assert.AreEqual("0",   Str("sum2", b));
    }

    [TestMethod]
    public void FullLifecycle_Table()
    {
        var b = Run($@"
{LoadPreamble}
        t = MakeTable()
        TablePut(t, 'x', 'alpha')
        TablePut(t, 'y', 'beta')
        r1 = TableGet(t, 'x')
        r2 = TableGet(t, 'y')
        TableWipe(t)
        r3 = TableGet(t, 'x')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("alpha", Str("r1", b));
        Assert.AreEqual("beta",  Str("r2", b));
        Assert.AreEqual("",      Str("r3", b));
    }

    [TestMethod]
    public void FullLifecycle_Point()
    {
        var b = Run($@"
{LoadPreamble}
        p = MakePoint(1, 2)
        p = PointMove(p, 9, 8)
        p = PointMove(p, -1, -1)
        rx = PointX(p)
        ry = PointY(p)
        p0 = PointReset(p)
        ox = PointX(p0)
        oy = PointY(p0)
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        Assert.AreEqual("9", Str("rx", b));
        Assert.AreEqual("9", Str("ry", b));
        Assert.AreEqual("0", Str("ox", b));
        Assert.AreEqual("0", Str("oy", b));
    }
}
