using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Conversion strategy for table variables
/// </summary>
public sealed class TableConversionStrategy : IConversionStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        var tableSelf = (TableVar)self;

        switch (targetType)
        {
            case Executive.VarType.TABLE:
                varOut = tableSelf;
                valueOut = tableSelf.Data;
                return true;

            case Executive.VarType.ARRAY:
                return ConvertToArray(tableSelf, out varOut, out valueOut);

            default:
                varOut = StringVar.Null();
                valueOut = "";
                return false;
        }
    }

    /// <summary>
    /// Converts a table to a 2D array where first column contains keys and second column contains values
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ConvertToArray(TableVar tableSelf, out Var varOut, out object valueOut)
    {
        // Cannot convert empty table to array
        var count = tableSelf.Count;
        if (count == 0)
        {
            varOut = StringVar.Null();
            valueOut = "";
            return false;
        }

        // Create 2D array: rows = table entry count, columns = 2 (key, value)
        var convertedArray = new ArrayVar();
        convertedArray.ConfigurePrototype($"{count},2", tableSelf.Fill);

        var arrayData = convertedArray.Data;
        var arrayIndex = 0;

        // Faster enumeration using foreach on dictionary
        foreach (var kvp in tableSelf.Data)
        {
            // Store key in first column
            arrayData[arrayIndex++] = ConvertKeyToVar(kvp.Key, tableSelf);

            // Store value in second column
            arrayData[arrayIndex++] = kvp.Value;
        }

        varOut = convertedArray;
        valueOut = tableSelf.Data;
        return true;
    }

    /// <summary>
    /// Converts a table key object to its appropriate Var type
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Var ConvertKeyToVar(object key, TableVar tableSelf)
    {
        return key switch
        {
            long longValue => new IntegerVar(longValue),
            double doubleValue => new RealVar(doubleValue),
            string stringValue => new StringVar(stringValue),
            _ => tableSelf.Collection != null && tableSelf.Key != null
                ? ((TableVar)tableSelf.Collection).Data[tableSelf.Key]
                : tableSelf
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDataType(Var self)
    {
        return "table";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetTableKey(Var self)
    {
        // Tables use their unique ID as table key
        return self.UniqueId;
    }
}