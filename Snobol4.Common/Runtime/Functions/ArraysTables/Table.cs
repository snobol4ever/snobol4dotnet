namespace Snobol4.Common;

//"table argument is not integer" /* 195 UNUSED*/,
//"table argument is out of range" /* 196 UNUSED*/,

public partial class Executive
{
                        public void CreateTable(List<Var> arguments)
    {
        var fill = arguments[2];
        TableVar newTable = new(fill);
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
        var value = tableVar.Data.TryGetValue(key, out var value1) ? value1 : tableVar.Fill;
        value.Key = varIndices[0].GetTableKey();
        value.Collection = tableVar;
        SystemStack.Push(value);
    }
}