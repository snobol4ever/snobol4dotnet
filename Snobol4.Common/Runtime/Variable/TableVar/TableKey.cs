namespace Snobol4.Common;

/// <summary>
/// A table's key for a value that is no string or number: an array, a table, a pattern, an expression, a code, a DATA object or a
/// name. It is equal only to the key of the same object (its identity), never to a string or a number, as SPITBOL keys a table by the
/// object itself: T&lt;A&gt; is not T&lt;n&gt; for the number n that happens to be A's identity, T&lt;.X&gt; is not T&lt;'X'&gt;, and the names
/// of two elements are not one key. It gives back the value it was made from, so CONVERT to ARRAY and DUMP show the object.
/// </summary>
internal sealed class TableKey(object identity, Var value) : IEquatable<TableKey>
{
    private readonly object _identity = identity;

    internal Var Value { get; } = value;

    public bool Equals(TableKey? other) => other is not null && _identity.Equals(other._identity);

    public override bool Equals(object? obj) => obj is TableKey other && Equals(other);

    public override int GetHashCode() => _identity.GetHashCode();
}
