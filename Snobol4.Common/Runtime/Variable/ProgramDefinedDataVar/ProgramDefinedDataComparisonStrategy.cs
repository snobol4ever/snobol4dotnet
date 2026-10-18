namespace Snobol4.Common;

public sealed class ProgramDefinedDataComparisonStrategy : IComparisonStrategy
{

    public int CompareTo(Var self, Var other)
    {
        var dataSelf = (ProgramDefinedDataVar)self;

        // Compare by data type name first
        if (other is ProgramDefinedDataVar dataOther)
        {
            // Use ordinal comparison for maximum performance
            var typeComparison = string.CompareOrdinal(
                dataSelf.DataName,
                dataOther.DataName);

            // If same type, compare by creation time of the object itself (its field store)
            return typeComparison != 0
                ? typeComparison
                : dataSelf.FieldValues.SequenceId.CompareTo(dataOther.FieldValues.SequenceId);
        }

        // Different base types compare by type name (ordinal for speed)
        return string.CompareOrdinal(dataSelf.DataType(), other.DataType());
    }


    // A DATA object is its field store: assignment, argument passing and storage copy the Var
    // wrapper but share FieldValues, and SPITBOL's IDENT sees one object (Budne's atn: IDENT(X, nil))
    public bool Equals(Var self, Var other) => SameObject(self, other);

    private static bool SameObject(Var self, Var other) =>
        other is ProgramDefinedDataVar dataOther && ReferenceEquals(((ProgramDefinedDataVar)self).FieldValues, dataOther.FieldValues);


    public bool IsIdentical(Var self, Var other) => SameObject(self, other);
}