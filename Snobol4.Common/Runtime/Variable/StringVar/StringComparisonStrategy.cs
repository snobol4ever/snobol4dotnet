using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Comparison strategy for string variables
/// </summary>
public class StringComparisonStrategy : IComparisonStrategy
{

    public int CompareTo(Var self, Var other)
    {
        var stringSelf = (StringVar)self;

        if (other is StringVar stringOther)
        {
            return string.Compare(stringSelf.Data, stringOther.Data, false, CultureInfo.CurrentCulture);
        }

        // Strings sort after other types by type name comparison
        return string.Compare(stringSelf.DataType(), other.DataType(), StringComparison.Ordinal);
    }


    public bool Equals(Var self, Var other)
    {
        if (other is not StringVar stringOther)
            return false;

        var stringSelf = (StringVar)self;
        return stringSelf.Data == stringOther.Data;
    }


    public bool IsIdentical(Var self, Var other)
    {
        if (other is not StringVar stringOther)
            return false;

        var stringSelf = (StringVar)self;
        return stringSelf.Data == stringOther.Data;
    }
}