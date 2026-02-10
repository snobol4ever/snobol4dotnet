namespace Snobol4.Common;

public sealed class ExpressionComparisonStrategy : IComparisonStrategy
{

    public int CompareTo(Var self, Var other)
    {
        // Different types compare by type name
        if (other is not ExpressionVar)
        {
            return string.Compare(self.DataType(), other.DataType(), StringComparison.OrdinalIgnoreCase);
        }

        // Expressions of the same type compare by creation time
        return self.CreationOrder.CompareTo(other.CreationOrder);;
    }


    public bool Equals(Var self, Var other)
    {
        // Expressions are only equal if they're the same instance
        return other.CreationOrder == self.CreationOrder;
    }


    public bool IsIdentical(Var self, Var other)
    {
        // Expressions are identical only if they have the same unique ID
        return other.CreationOrder == self.CreationOrder;
    }
}