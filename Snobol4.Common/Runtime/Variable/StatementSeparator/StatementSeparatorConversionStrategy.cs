using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Conversion strategy for statement separator
/// Statement separators cannot be converted to other types
/// </summary>
public sealed class StatementSeparatorConversionStrategy : IConversionStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        varOut = StringVar.Null();
        valueOut = "";

        // Statement separators cannot be converted to any type
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDataType(Var self)
    {
        return "statement-separator";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetTableKey(Var self)
    {
        // Statement separators use their unique ID as table key (though this should never be used)
        return self.Uid;
    }
}