namespace Snobol4.Common;

public sealed class ProgramDefinedDataFormattingStrategy : IFormattingStrategy
{

    public string ToString(Var self)
    {
        var dataSelf = (ProgramDefinedDataVar)self;
        return dataSelf.DataName;
    }

    public string DumpString(Var self)
    {
        var dataSelf = (ProgramDefinedDataVar)self;

        // Show type and field count
        return $"<{dataSelf.DataName}:{dataSelf}>";
    }

    public string DebugVar(Var self)
    {
        var dataSelf = (ProgramDefinedDataVar)self;
        var symbol = dataSelf.Symbol == "" ? "<no name>" : dataSelf.Symbol;

        // Show field names - use string.Join for better performance
        //var fields = string.Join(", ", dataSelf.Data.Keys);

        return $"DATA Symbol: {symbol}  Type: {dataSelf.DataName}  Succeeded: {dataSelf.Succeeded}";
    }
}