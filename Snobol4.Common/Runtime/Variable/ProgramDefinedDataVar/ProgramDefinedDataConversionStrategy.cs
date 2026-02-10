namespace Snobol4.Common;

public sealed class ProgramDefinedDataConversionStrategy : IConversionStrategy
{

    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        if (targetType == Executive.VarType.STRING)
        {
            var dataSelf = (ProgramDefinedDataVar)self;
            var typeName = dataSelf.DataName;
            varOut = new StringVar(typeName);
            valueOut = typeName;
            return true;
        }

        // All other conversions fail
        varOut = StringVar.Null();
        valueOut = "";
        return false;
    }


    public string GetDataType(Var self)
    {
        var dataSelf = (ProgramDefinedDataVar)self;
        // Return the user-defined type name, not "data"
        return dataSelf.DataName;
    }


    public object GetTableKey(Var self)
    {
        // User-defined data uses its unique ID as table key
        return self.CreationOrder;
    }
}