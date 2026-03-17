namespace Snobol4.Common;

/// <summary>
/// Public factory and accessor methods on Executive for use by IExternalLibrary
/// implementations. These methods expose ArrayVar/TableVar object lifecycle
/// operations without leaking the internal implementation details of those types.
///
/// Added for net-load-dotnet Step 7: SnobolVar/Pattern/Table/Array return
/// coercions via IExternalLibrary fast path.
/// </summary>
public partial class Executive
{
    // ── ArrayVar public API ───────────────────────────────────────────────

    /// <summary>
    /// Create a new 1-D ArrayVar with the given size (lower bound = 1) and
    /// "" fill. Returns null if the prototype is invalid.
    /// </summary>
    public ArrayVar? CreateArray(long size)
    {
        if (size < 1) return null;
        var arr = new ArrayVar();
        var err = arr.ConfigurePrototype(size.ToString(), new StringVar(""));
        return err == 0 ? arr : null;
    }

    /// <summary>
    /// Create a new ArrayVar from a SNOBOL4 prototype string (e.g. "3" or "1:5,2:4").
    /// Returns null if the prototype is invalid.
    /// </summary>
    public ArrayVar? CreateArray(string prototype, Var? fill = null)
    {
        var arr = new ArrayVar();
        var err = arr.ConfigurePrototype(prototype, fill ?? new StringVar(""));
        return err == 0 ? arr : null;
    }

    /// <summary>Get the element at 1-based index i from a 1-D ArrayVar.</summary>
    public Var ArrayGet(ArrayVar arr, long i) => arr.GetElement([i]);

    /// <summary>Set the element at 1-based index i in a 1-D ArrayVar.</summary>
    public void ArraySet(ArrayVar arr, long i, Var value) => arr.SetElement([i], value);

    /// <summary>Get the total element count of an ArrayVar.</summary>
    public long ArrayTotalSize(ArrayVar arr) => arr.TotalSize;

    /// <summary>Get all data elements of an ArrayVar (read-only view).</summary>
    public IReadOnlyList<Var> ArrayData(ArrayVar arr) => arr.Data;

    /// <summary>Set all slots of an ArrayVar to "".</summary>
    public void ArrayFillEmpty(ArrayVar arr)
    {
        for (var i = 0; i < arr.Data.Count; i++)
            arr.Data[i] = new StringVar("");
    }

    // ── TableVar public API ───────────────────────────────────────────────

    /// <summary>Create a new TableVar with "" fill.</summary>
    public TableVar CreateTable(Var? fill = null) =>
        new(fill ?? new StringVar(""));

    /// <summary>Store a value in a TableVar using a Var key (coerced to table key).</summary>
    public void TablePut(TableVar tbl, Var key, Var value) =>
        tbl.Set(key.GetTableKey(), value);

    /// <summary>Store a value in a TableVar using a string key.</summary>
    public void TablePut(TableVar tbl, string key, Var value) =>
        tbl.Set(key, value);

    /// <summary>Retrieve a value from a TableVar by Var key (returns fill if absent).</summary>
    public Var TableGet(TableVar tbl, Var key) =>
        tbl.GetOrDefault(key.GetTableKey());

    /// <summary>Retrieve a value from a TableVar by string key (returns fill if absent).</summary>
    public Var TableGet(TableVar tbl, string key) =>
        tbl.GetOrDefault(key);

    /// <summary>Get all keys currently stored in a TableVar.</summary>
    public IEnumerable<object> TableKeys(TableVar tbl) => tbl.GetKeys();

    /// <summary>Remove all entries from a TableVar.</summary>
    public void TableWipe(TableVar tbl) => tbl.Clear();

    /// <summary>Number of entries currently in a TableVar.</summary>
    public int TableCount(TableVar tbl) => tbl.Count;
}
