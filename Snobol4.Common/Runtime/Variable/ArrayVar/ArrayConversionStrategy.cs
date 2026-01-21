using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Conversion strategy for array variables.
/// Supports conversion to table (2-column arrays) and self-conversion.
/// </summary>
public class ArrayConversionStrategy : IConversionStrategy
{
    private const int _requiredTableDimensions = 2;
    private const int _requiredTableColumns = 2;
    private const int _keyColumnIndex = 0;
    private const int _valueColumnIndex = 1;

    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        var arraySelf = (ArrayVar)self;

        return targetType switch
        {
            Executive.VarType.ARRAY => ConvertToSelf(arraySelf, out varOut, out valueOut),
            Executive.VarType.TABLE => TryConvertToTable(arraySelf, out varOut, out valueOut),
            _ => InitializeFailure(out varOut, out valueOut)
        };
    }

    /// <summary>
    /// Convert array to itself (identity conversion)
    /// </summary>

    private static bool ConvertToSelf(ArrayVar array, out Var varOut, out object valueOut)
    {
        varOut = array;
        valueOut = array;
        return true;
    }

    /// <summary>
    /// Attempt to convert a 2-dimensional, 2-column array to a table.
    /// The array must be in format [key1, value1, key2, value2, ...]
    /// </summary>
    private static bool TryConvertToTable(ArrayVar array, out Var varOut, out object valueOut)
    {
        // Fast validation checks first (fail fast)
        if (array.Dimensions != _requiredTableDimensions)
            return InitializeFailure(out varOut, out valueOut);

        // Calculate column count once
        var columnCount = array.UpperBounds[_keyColumnIndex] - array.LowerBounds[_keyColumnIndex] + 1;
        
        if (columnCount != _requiredTableColumns)
            return InitializeFailure(out varOut, out valueOut);

        var dataCount = array.Data.Count;
        
        // Validate sufficient data elements (must be even for key-value pairs)
        if (dataCount < _requiredTableColumns || (dataCount & 1) != 0) // Bitwise check for even/odd
            return InitializeFailure(out varOut, out valueOut);

        // Create new table with appropriate capacity
        var convertedTable = new TableVar(array.Fill);
        var entryCount = dataCount >> 1; // Divide by 2 using bit shift
        
        // Pre-allocate dictionary capacity to avoid resizing
        convertedTable.Data = new Dictionary<object, Var>(entryCount);

        // Convert array pairs to table entries
        // Data is stored as [key1, value1, key2, value2, ...]
        var data = array.Data; // Cache reference to avoid property access
        for (var i = 0; i < dataCount; i += _requiredTableColumns)
        {
            var key = data[i].GetTableKey(); // Direct indexing without offset calculation
            var value = data[i + 1];
            convertedTable.Data[key] = value;
        }

        valueOut = convertedTable.Data;
        varOut = convertedTable;
        return true;
    }


    private static bool InitializeFailure(out Var varOut, out object valueOut)
    {
        varOut = StringVar.Null();
        valueOut = string.Empty;
        return false;
    }

    public string GetDataType(Var self)
    {
        return "array";
    }

    public object GetTableKey(Var self)
    {
        // Arrays use their unique ID as table key (reference equality)
        return self.Uid;
    }
}