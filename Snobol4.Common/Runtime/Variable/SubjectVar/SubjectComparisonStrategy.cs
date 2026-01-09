using System.Globalization;

namespace Snobol4.Common;

/// <summary>
/// Comparison strategy for subject variables
/// Subject variables compare by their string content
/// </summary>
public class SubjectComparisonStrategy : IComparisonStrategy
{
    public int CompareTo(Var self, Var other)
    {
        var subjectSelf = (SubjectVar)self;

        // Optimized: Use pattern matching with when clauses and direct comparison
        return other switch
        {
            SubjectVar subjectOther => string.CompareOrdinal(subjectSelf.Subject, subjectOther.Subject),
            StringVar stringOther => string.CompareOrdinal(subjectSelf.Subject, stringOther.Data),
            _ => string.CompareOrdinal(subjectSelf.DataType(), other.DataType())
        };
    }

    public bool Equals(Var self, Var other)
    {
        var subjectSelf = (SubjectVar)self;

        return other switch
        {
            SubjectVar subjectOther => subjectSelf.Subject == subjectOther.Subject,
            StringVar stringOther => subjectSelf.Subject == stringOther.Data,
            _ => false
        };
    }

    public bool IsIdentical(Var self, Var other)
    {
        // Optimized: Direct comparison without property access
        return ReferenceEquals(self, other) || other.UniqueId == self.UniqueId;
    }
}