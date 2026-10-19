using System.Globalization;

namespace Snobol4.Common;

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


    // Two names are identical when they name the same variable or element (SPITBOL's ident compares the
    // name blocks' base and offset); a name and any other value differ, so IDENT(.X) fails. Each .&KEYWORD
    // builds a new keyword block in SPITBOL, so two keyword names are identical only as one value
    public bool IsIdentical(Var self, Var other) =>
        ReferenceEquals(self, other) || (!((NameVar)self).Pointer.StartsWith('&') && Equals(self, other));
}