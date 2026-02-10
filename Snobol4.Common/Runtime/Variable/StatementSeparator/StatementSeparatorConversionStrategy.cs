namespace Snobol4.Common;

public sealed class StatementSeparatorConversionStrategy : IConversionStrategy
{

    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        varOut = StringVar.Null();
        valueOut = "";

        // Statement separators cannot be converted to any type
        return false;
    }


    public string GetDataType(Var self)
    {
        return "statement-separator";
    }


    public object GetTableKey(Var self)
    {
        // Statement separators use their unique ID as table key (though this should never be used)
        return self.CreationOrder;
    }
}