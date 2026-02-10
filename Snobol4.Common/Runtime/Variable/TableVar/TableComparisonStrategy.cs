using System.Globalization;

namespace Snobol4.Common;

public sealed class TableComparisonStrategy : IComparisonStrategy
{

    public int CompareTo(Var self, Var other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var tableSelf = (TableVar)self;

        // Tables of the same type compare by creation time (chronological ordering)
        if (other is TableVar otherTable)
        {
            return tableSelf.CreationOrder.CompareTo(otherTable.CreationOrder);
        }

        // Different types compare by data type name (lexicographical ordering)
        return string.Compare(
            tableSelf.DataType(), 
            other.DataType(), 
            ignoreCase: false, 
            CultureInfo.InvariantCulture
        );
    }


    public bool Equals(Var self, Var other)
    {
        // Tables use reference equality - only equal if same instance
        return IsIdentical(self, other);
    }


    public bool IsIdentical(Var self, Var other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Tables are identical only if they have the same unique ID (same instance)
        return other.CreationOrder == self.CreationOrder;
    }
}