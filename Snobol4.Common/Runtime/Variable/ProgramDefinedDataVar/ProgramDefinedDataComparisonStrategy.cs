using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Comparison strategy for program-defined data variables
/// User-defined data types compare by type name then creation time
/// </summary>
public sealed class ProgramDefinedDataComparisonStrategy : IComparisonStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Var self, Var other)
    {
        var dataSelf = (ProgramDefinedDataVar)self;

        // Compare by data type name first
        if (other is ProgramDefinedDataVar dataOther)
        {
            // Use ordinal comparison for maximum performance
            var typeComparison = string.CompareOrdinal(
                dataSelf.UserDefinedDataName,
                dataOther.UserDefinedDataName);

            // If same type, compare by creation time
            return typeComparison != 0
                ? typeComparison
                : DateTime.Compare(dataSelf.CreationDateTime, other.CreationDateTime);
        }

        // Different base types compare by type name (ordinal for speed)
        return string.CompareOrdinal(dataSelf.DataType(), other.DataType());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Var self, Var other)
    {
        // User-defined data is only equal if it's the same instance
        return self.Uid == other.Uid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsIdentical(Var self, Var other)
    {
        // User-defined data is identical only if they have the same unique ID
        return self.Uid == other.Uid;
    }
}