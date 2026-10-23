namespace Snobol4.Common;

public sealed class ProgramDefinedDataConversionStrategy : IConversionStrategy
{

    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        // A DATA object is no string, as an array or a table is not: OUTPUT prints its datatype name, but a concatenation, SIZE,
        // LPAD and the like refuse it. All conversions fail.
        varOut = StringVar.Null();
        valueOut = "";
        return false;
    }


    public string GetDataType(Var self)
    {
        var dataSelf = (ProgramDefinedDataVar)self;
        // The name as DATA defined it: folded to upper case under case folding, as spelled under
        // -f, as sbl answers (NODE under -b, node under -bf)
        return dataSelf.DataName;
    }


    public object GetTableKey(Var self)
    {
        // User-defined data is keyed by the object itself, its field store, shared by every copy
        return new TableKey(((ProgramDefinedDataVar)self).FieldValues.SequenceId, self);
    }
}