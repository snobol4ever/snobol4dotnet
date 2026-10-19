namespace Snobol4.Common;

public sealed class TableConversionStrategy : IConversionStrategy
{

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

            
    private static bool ConvertToArray(TableVar tableSelf, out Var varOut, out object valueOut)
    {
        // Only entries with non-null values become rows, and a table with none cannot convert, as in SPITBOL
        // (a key assigned the null string is no entry: SORT of such a table fails)
        var count = tableSelf.Data.Values.Count(value => value is not StringVar { Data: "" });
        if (count == 0)
        {
            varOut = StringVar.Null();
            valueOut = "";
            return false;
        }

        // Create 2D array: rows = table entry count, columns = 2 (key, value)
        var convertedArray = new ArrayVar { BlockId = Executive.NewBlockId() };
        convertedArray.ConfigurePrototype($"{count},2", tableSelf.Fill);

        var arrayData = convertedArray.Data;
        var arrayIndex = 0;

        // Faster enumeration using foreach on dictionary
        foreach (var kvp in tableSelf.Data)
        {
            if (kvp.Value is StringVar { Data: "" })
                continue;

            // Store key in first column
            arrayData[arrayIndex++] = ConvertKeyToVar(kvp.Key, tableSelf);

            // Store value in second column
            arrayData[arrayIndex++] = kvp.Value;
        }

        varOut = convertedArray;
        valueOut = tableSelf.Data;
        return true;
    }

            
    private static Var ConvertKeyToVar(object key, TableVar tableSelf)
    {
        return key switch
        {
            long longValue => new IntegerVar(longValue),
            double doubleValue => new RealVar(doubleValue),
            string stringValue => new StringVar(stringValue),
            _ => tableSelf.Collection is not null && tableSelf.Key is not null
                ? ((TableVar)tableSelf.Collection).Data[tableSelf.Key]
                : tableSelf
        };
    }


    public string GetDataType(Var self)
    {
        return "TABLE";
    }


    public object GetTableKey(Var self)
    {
        // Tables use their unique ID as table key
        return self.SequenceId;
    }
}