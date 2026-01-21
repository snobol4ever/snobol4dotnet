    using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Cloning strategy for table variables
/// Creates a deep copy of the table including:
/// - Fill value (cloned)
/// - All key-value pairs (values cloned, keys copied by reference)
/// - Symbol, input/output channels
/// </summary>
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
            targetData[kvp.Key] = kvp.Value.Clone();
        }

        return clonedTable;
    }
}