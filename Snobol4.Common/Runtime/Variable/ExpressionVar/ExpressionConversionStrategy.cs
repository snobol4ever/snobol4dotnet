namespace Snobol4.Common;

public sealed class ExpressionConversionStrategy : IConversionStrategy
{

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


    public string GetDataType(Var self)
    {
        return "expression";
    }


    public object GetTableKey(Var self)
    {
        // Expressions use their unique ID as table key
        return self.CreationOrder;
    }
}