namespace Snobol4.Common;

//"table argument is not integer" /* 195 */,
//"table argument is out of range" /* 196 */,

public partial class Executive
{
                        public void CreateTable(List<Var> arguments)
    {
        if (!SmallInteger(arguments[0], out var size, out var inRange))
        {
            LogRuntimeException(195);
            return;
        }

        if (!inRange)
        {
            LogRuntimeException(196);
            return;
        }

        var fill = arguments[2];
        TableVar newTable = new(fill) { BlockId = NewBlockId() };
        if (size > 0)
            newTable.Buckets = (int)Math.Min(size, int.MaxValue);
        SystemStack.Push(newTable);
    }

    private void IndexTable(TableVar tableVar, List<Var> varIndices)
    {
        if (varIndices.Count != 1)
        {
            LogRuntimeException(237);
            return;
        }

        var key = varIndices[0].GetTableKey();
        Var value;
        if (tableVar.Data.TryGetValue(key, out var stored))
        {
            // Aggregate types (Array/Table) keep reference semantics so that
            // chained-subscript writes — e.g. mem['s']['w'] = 1 — modify the
            // real inner aggregate, not a transient clone.  Scalars are cloned
            // to avoid aliasing the data slot with a stack/variable copy.
            value = stored is ArrayVar or TableVar ? stored : stored.Clone();
        }
        else
        {
            // Missing key: return a fresh clone of Fill so the lvalue path can
            // safely stamp Key/Collection without mutating the shared default.
            value = tableVar.Fill.Clone();
        }
        value.Key = varIndices[0].GetTableKey();
        value.Collection = tableVar;
        value.LvalueMark = StatementSerial;
        SystemStack.Push(value);
    }
}