using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Conversion strategy for program-defined data variables
/// </summary>
public sealed class ProgramDefinedDataConversionStrategy : IConversionStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        if (targetType == Executive.VarType.STRING)
        {
            var dataSelf = (ProgramDefinedDataVar)self;
            var typeName = dataSelf.UserDefinedDataName;
            varOut = new StringVar(typeName);
            valueOut = typeName;
            return true;
        }

        // All other conversions fail
        varOut = StringVar.Null();
        valueOut = "";
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDataType(Var self)
    {
        var dataSelf = (ProgramDefinedDataVar)self;
        // Return the user-defined type name, not "data"
        return dataSelf.UserDefinedDataName;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetTableKey(Var self)
    {
        // User-defined data uses its unique ID as table key
        return self.Uid;
    }
}