namespace Snobol4.Common;

/// <summary>
/// A pool of interned constant Var objects, created once at compile time.
///
/// In the current C# generation path, every execution of a statement like
///   x.Constant("1776")
/// allocates a new StringVar. The ConstantPool allocates each distinct
/// constant value exactly once and reuses it across all executions.
///
/// Constants are immutable in SNOBOL4 (they are never the target of
/// assignment), so sharing is safe.
///
/// Usage: at compile time, call GetOrAdd() for each literal token to obtain
/// a slot index. At runtime, the threaded executor reads Pool[slotIndex]
/// to push the pre-built Var onto the system stack.
/// </summary>
internal sealed class ConstantPool
{
    private readonly List<Var> _pool = new(64);

    // Separate indices for each type so we can look up quickly without
    // boxing integers/reals into strings for the key.
    private readonly Dictionary<string, int> _stringIndex =
        new(StringComparer.Ordinal);
    private readonly Dictionary<long, int> _integerIndex = new();
    private readonly Dictionary<double, int> _realIndex = new();

    /// <summary>All interned constants, indexed by slot number.</summary>
    internal IReadOnlyList<Var> Pool => _pool;

    /// <summary>
    /// Get or add a string constant. Returns the slot index.
    /// </summary>
    internal int GetOrAddString(string value)
    {
        if (_stringIndex.TryGetValue(value, out var idx))
            return idx;

        idx = _pool.Count;
        _pool.Add(new StringVar(value));
        _stringIndex[value] = idx;
        return idx;
    }

    /// <summary>
    /// Get or add an integer constant. Returns the slot index.
    /// </summary>
    internal int GetOrAddInteger(long value)
    {
        if (_integerIndex.TryGetValue(value, out var idx))
            return idx;

        idx = _pool.Count;
        _pool.Add(new IntegerVar(value));
        _integerIndex[value] = idx;
        return idx;
    }

    /// <summary>
    /// Get or add a real constant. Returns the slot index.
    /// </summary>
    internal int GetOrAddReal(double value)
    {
        if (_realIndex.TryGetValue(value, out var idx))
            return idx;

        idx = _pool.Count;
        _pool.Add(new RealVar(value));
        _realIndex[value] = idx;
        return idx;
    }

    /// <summary>Total number of interned constants.</summary>
    internal int Count => _pool.Count;
}
