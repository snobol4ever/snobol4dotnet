namespace Snobol4.Common;

public sealed class TableFormattingStrategy : IFormattingStrategy
{
            
    public string ToString(Var self)
    {
        return "table";
    }

                
    public string DumpString(Var self)
    {
        var tableSelf = (TableVar)self;
        return $"table({tableSelf.Count})";
    }

                            public string DebugVar(Var self)
    {
        var tableSelf = (TableVar)self;
        var symbol = string.IsNullOrEmpty(tableSelf.Symbol) ? "<no name>" : tableSelf.Symbol;
        return $"TABLE Symbol: {symbol}, Count: {tableSelf.Count}, Succeeded: {tableSelf.Succeeded}, Fill: {tableSelf.Fill.DataType()}";
    }
}