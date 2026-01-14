using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Comparison strategy for table variables
/// Tables compare by creation time (for same type) or data type name (for different types)
/// Tables are only equal/identical if they reference the same instance
/// </summary>
public sealed class TableComparisonStrategy : IComparisonStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Var self, Var other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var tableSelf = (TableVar)self;

        // Tables of the same type compare by creation time (chronological ordering)
        if (other is TableVar otherTable)
        {
            return DateTime.Compare(tableSelf.CreationDateTime, otherTable.CreationDateTime);
        }

        // Different types compare by data type name (lexicographical ordering)
        return string.Compare(
            tableSelf.DataType(), 
            other.DataType(), 
            ignoreCase: false, 
            CultureInfo.InvariantCulture
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Var self, Var other)
    {
        // Tables use reference equality - only equal if same instance
        return IsIdentical(self, other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsIdentical(Var self, Var other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Tables are identical only if they have the same unique ID (same instance)
        return other.Uid == self.Uid;
    }
}