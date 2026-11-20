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

        // the names of deferred expressions are equal only as one name (IDENT(.*A<1>, .*A<1>) fails, as in SPITBOL)
        if (nameSelf.Expression is not null || nameOther.Expression is not null)
            return ReferenceEquals(nameSelf, nameOther);

        // the same collection: an element read out of another (T['a']['b'], A<1><2>) is a handle on the array or table it holds, a
        // new one each read that shares its storage, so the names of one element are identical, as in SPITBOL
        if (!SameCollection(nameSelf.TargetCollection, nameOther.TargetCollection))
            return false;

        // Check key equality (handles null correctly)
        return Equals(nameSelf.TargetKey, nameOther.TargetKey);
    }


    private static bool SameCollection(Var? a, Var? b) =>
        ReferenceEquals(a, b)
        || a is TableVar ta && b is TableVar tb && ReferenceEquals(ta.Data, tb.Data)
        || a is ArrayVar aa && b is ArrayVar ab && ReferenceEquals(aa.Data, ab.Data)
        || a is ProgramDefinedDataVar da && b is ProgramDefinedDataVar db && ReferenceEquals(da.FieldValues, db.FieldValues);

    // Two names are identical when they name the same variable or element (SPITBOL's ident compares the
    // name blocks' base and offset); a name and any other value differ, so IDENT(.X) fails. Each .&KEYWORD
    // builds a new keyword block in SPITBOL, so two keyword names are identical only as one value
    public bool IsIdentical(Var self, Var other) =>
        ReferenceEquals(self, other) || (!((NameVar)self).Pointer.StartsWith('&') && Equals(self, other));
}