    namespace Snobol4.Common;

public sealed class TableCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var tableSelf = (TableVar)self;

        // Clone the fill value to ensure independence
        var clonedFill = tableSelf.Fill.Clone();
        var sourceCount = tableSelf.Count;
        
        // Pre-allocate dictionary capacity to avoid resizing
        var clonedTable = new TableVar(clonedFill, sourceCount)
        {
            BlockId = Executive.NewBlockId(),
            Buckets = tableSelf.Buckets,
            Symbol = tableSelf.Symbol,
            InputChannel = tableSelf.InputChannel,
            OutputChannel = tableSelf.OutputChannel
        };

        // Deep copy all key-value pairs
        // Keys are copied by reference (can be primitives or composite types)
        // Values are cloned to ensure independence from the original table
        var sourceData = tableSelf.Data;
        var targetData = clonedTable.Data;
        
        foreach (var kvp in sourceData)
        {
            // an array, table or DATA object value is shared, as SPITBOL's COPY copies one level
            targetData[kvp.Key] = kvp.Value is ArrayVar or TableVar or ProgramDefinedDataVar ? kvp.Value : kvp.Value.Clone();
        }

        return clonedTable;
    }
}