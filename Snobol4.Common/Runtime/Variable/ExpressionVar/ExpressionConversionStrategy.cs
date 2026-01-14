using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Conversion strategy for expression variables
/// </summary>
public sealed class ExpressionConversionStrategy : IConversionStrategy
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        if (targetType == Executive.VarType.EXPRESSION)
        {
            var expressionSelf = (ExpressionVar)self;
            varOut = expressionSelf;
            valueOut = expressionSelf.FunctionName;
            return true;
        }

        varOut = StringVar.Null();
        valueOut = "";
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDataType(Var self)
    {
        return "expression";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetTableKey(Var self)
    {
        // Expressions use their unique ID as table key
        return self.Uid;
    }
}