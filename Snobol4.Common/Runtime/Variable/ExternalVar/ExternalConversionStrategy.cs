namespace Snobol4.Common;

public class ExternalConversionStrategy : IConversionStrategy
{
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive executive)
    {
        // EXTERNAL can only convert to itself — opaque pointer, no coercion.
        varOut    = self;
        valueOut  = ((ExternalVar)self).Pointer;
        return targetType == Executive.VarType.STRING; // treat as passable, not coercible
    }

    public string GetDataType(Var self) => "EXTERNAL";

    public object GetTableKey(Var self) => new TableKey(((ExternalVar)self).Pointer.ToInt64(), self);
}
