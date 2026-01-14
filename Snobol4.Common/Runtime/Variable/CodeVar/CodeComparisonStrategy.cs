using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Comparison strategy for code variables
/// Code compares by creation time and data type
/// </summary>
public sealed class CodeComparisonStrategy : IComparisonStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Var self, Var other)
    {
        var codeSelf = (CodeVar)self;

        // Code of the same type compares by creation time
        if (other is CodeVar)
        {
            return codeSelf.CreationDateTime.CompareTo(other.CreationDateTime);
        }

        // Different types compare by type name
        return string.Compare(codeSelf.DataType(), other.DataType(), StringComparison.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Var self, Var other)
    {
        // Code is only equal if it's the same instance
        return self.Uid == other.Uid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsIdentical(Var self, Var other)
    {
        // Code is identical only if they have the same unique ID
        return self.Uid == other.Uid;
    }
}