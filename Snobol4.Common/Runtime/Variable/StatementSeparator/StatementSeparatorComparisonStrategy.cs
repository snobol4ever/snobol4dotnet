namespace Snobol4.Common;

public sealed class StatementSeparatorComparisonStrategy : IComparisonStrategy
{

    public int CompareTo(Var self, Var other)
    {
        // Statement separators are internal markers - comparison doesn't make sense
        throw new InvalidOperationException("Statement separators cannot be compared");
    }


    public bool Equals(Var self, Var other)
    {
        // Only equal if both are statement separators
        return other is StatementSeparator;
    }


    public bool IsIdentical(Var self, Var other)
    {
        // Statement separators are identical if both are statement separators
        return other is StatementSeparator;
    }
}