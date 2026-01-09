namespace Snobol4.Common;

public partial class Executive
{
    // Lock object for thread synchronization
    private readonly Lock _tableCreationLock = new();
    private readonly Lock _tableIndexLock = new();

    /// <summary>
    /// Factory to create a table (Thread-Safe)
    /// </summary>
    /// <param name="arguments">List of arguments
    /// The first argument is used. All others are ignored for tables</param>
    public void CreateTable(List<Var> arguments)
    {
        lock (_tableCreationLock)
        {
            var fill = arguments[2];
            TableVar newTable = new(fill);
            SystemStack.Push(newTable);
        }
    }

    private void IndexTable(TableVar tableVar, List<Var> varIndices)
    {
        lock (_tableIndexLock)
        {
            if (varIndices.Count != 1)
            {
                LogRuntimeException(237);
                return;
            }

            var key = varIndices[0].GetTableKey();
            var value = tableVar.Data.TryGetValue(key, out var value1) ? value1 : tableVar.Fill.Clone();
            value.Key = varIndices[0].GetTableKey();
            value.Collection = tableVar;
            SystemStack.Push(value);
        }
    }
}