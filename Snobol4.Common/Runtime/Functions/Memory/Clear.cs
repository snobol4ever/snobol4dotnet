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

        data = Parent.FoldCase((string)data);
        var valuesArray = ((string)data).Split(',');

        // Skip list cannot have null entries
        if ((string)data != "" && valuesArray.Contains(""))
        {
            LogRuntimeException(72);
            return;
        }

        var skipList = valuesArray.ToList();

        // The null value goes in under any input or output association, as SPITBOL stores it at the end
        // of the variable's trap chain (s_clr): OUTPUT still writes after CLEAR(), and nothing is written
        foreach (var kvp in IdentifierTable.Where(kvp =>
                     !skipList.Contains(kvp.Key) && kvp.Key[0] != '&' && !kvp.Value.IsReadOnly).ToList())
        {
            var cleared = StringVar.Null();
            cleared.InputChannel = kvp.Value.InputChannel;
            cleared.OutputChannel = kvp.Value.OutputChannel;
            IdentifierTable[kvp.Key] = cleared;
        }

        SystemStack.Push(StringVar.Null());
    }
}