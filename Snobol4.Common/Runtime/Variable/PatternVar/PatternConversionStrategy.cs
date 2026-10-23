namespace Snobol4.Common;

public class PatternConversionStrategy : IConversionStrategy
{

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


    public string GetDataType(Var self)
    {
        return "PATTERN";
    }


    public object GetTableKey(Var self)
    {
        // A pattern is keyed by its identity, which every copy of it keeps (Q = P, then T<Q> is T<P>), as IDENT compares it
        return new TableKey(((PatternVar)self).Identity, self);
    }
}