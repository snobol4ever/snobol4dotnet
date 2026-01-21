using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Comparison strategy for name variables
/// Names compare by dereferencing to the target variable
/// </summary>
public class NameComparisonStrategy : IComparisonStrategy
{

    public int CompareTo(Var self, Var other)
    {
        var nameSelf = (NameVar)self;

        // Fast path: comparing two names
        if (other is NameVar nameOther)
        {
            return string.CompareOrdinal(nameSelf.Pointer, nameOther.Pointer);
        }

        // Different types compare by type name
        return string.Compare(nameSelf.DataType(), other.DataType(), false, CultureInfo.InvariantCulture);
    }

    public bool Equals(Var self, Var other)
    {
        if (other is not NameVar nameOther)
            return false;

        var nameSelf = (NameVar)self;

        // Fast path: check pointer equality first (most common)
        if (nameSelf.Pointer != nameOther.Pointer)
            return false;

        // Check collection reference equality
        if (nameSelf.Collection != nameOther.Collection)
            return false;

        // Check key equality (handles null correctly)
        return Equals(nameSelf.Key, nameOther.Key);
    }


    public bool IsIdentical(Var self, Var other)
    {
        // From original: always returns true
        // This is intentional - names are considered identical based on what they reference
        return true;
    }
}