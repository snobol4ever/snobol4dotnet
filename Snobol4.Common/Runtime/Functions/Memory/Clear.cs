namespace Snobol4.Common;

//"clear argument is not a string" /* 71 */,
//"clear argument has null variable name" /* 72 */,

public partial class Executive
{
    internal void ReinitializeVariables(List<Var> arguments)
    {

        // first argument must be a string
        if (!arguments[0].Convert(VarType.STRING, out _, out var data, this))
        {
            LogRuntimeException(71);
            return;
        }

        var valuesArray = ((string)data).Split(',');

        // Skip list cannot have null entries
        if ((string)data != "" && valuesArray.Contains(""))
        {
            LogRuntimeException(72);
            return;
        }

        var skipList = valuesArray.ToList();

        foreach (var kvp in IdentifierTable.Where(kvp =>
                     !skipList.Contains(kvp.Key) && kvp.Key[0] != '&' && !kvp.Value.IsReadOnly))
        {
            IdentifierTable[kvp.Key] = StringVar.Null();
            IdentifierTable[kvp.Key].Symbol = kvp.Key;
        }

        SystemStack.Push(StringVar.Null());
    }
}