namespace Snobol4.Common;

public interface IConversionStrategy
{
                bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive executive);

                string GetDataType(Var self);

                object GetTableKey(Var self);
}