using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Conversion strategy for code variables
/// </summary>
public sealed class CodeConversionStrategy : IConversionStrategy
{
    private const string _codeTypeString = "code";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        if (targetType == Executive.VarType.CODE)
        {
            var codeSelf = (CodeVar)self;
            varOut = codeSelf;
            valueOut = codeSelf.Data;
            return true;
        }

        varOut = StringVar.Null();
        valueOut = string.Empty;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDataType(Var self)
    {
        return _codeTypeString;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetTableKey(Var self)
    {
        // Code uses its unique ID as table key
        return self.Uid;
    }
}