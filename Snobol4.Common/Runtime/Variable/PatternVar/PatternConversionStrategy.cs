using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Conversion strategy for pattern variables
/// </summary>
public class PatternConversionStrategy : IConversionStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        var patternSelf = (PatternVar)self;

        if (targetType == Executive.VarType.PATTERN)
        {
            varOut = patternSelf;
            valueOut = patternSelf.Data;
            return true;
        }

        // All other types fail conversion
        varOut = StringVar.Null();
        valueOut = "";
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDataType(Var self)
    {
        return "pattern";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetTableKey(Var self)
    {
        // Patterns use their unique ID as table key
        return self.Uid;
    }
}