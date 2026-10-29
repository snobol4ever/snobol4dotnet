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

        // an expression is a pattern that evaluates it when it is matched, as *X is in a pattern: CONVERT(*X, 'PATTERN') in SPITBOL
        if (targetType == Executive.VarType.PATTERN)
        {
            varOut = new PatternVar(UnevaluatedPattern.Structure(((ExpressionVar)self).FunctionName));
            valueOut = ((PatternVar)varOut).Data;
            return true;
        }

        varOut = StringVar.Null();
        valueOut = "";
        return false;
    }


    public string GetDataType(Var self)
    {
        return "EXPRESSION";
    }


    public object GetTableKey(Var self)
    {
        // Expressions use their unique ID as table key
        return new TableKey(self.SequenceId, self);
    }
}