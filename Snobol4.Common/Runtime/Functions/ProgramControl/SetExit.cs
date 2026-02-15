namespace Snobol4.Common;

//"setexit argument is not label name or null" /* 187 */,

public partial class Executive
{
    internal string SetExitLabel = "";
    internal bool InSetExit = false;

    internal void SetExit(List<Var> arguments)
    {
        var previousSetExitLabel = SetExitLabel;

        if (arguments[0] is StringVar str && str.Data.Length == 0)
        {
            SystemStack.Push(new StringVar(previousSetExitLabel));
            SetExitLabel = "";
            return;
        }

        if (arguments[0].Convert(VarType.NAME, out var name, out _, this))
        {
            var label = ((NameVar)name).Pointer;

            if (LabelTable.TryGetValue(label, out var setExitLabel))
            {
                SetExitLabel = label;
                SystemStack.Push(new StringVar(previousSetExitLabel));
                return;
            }
        }

        SystemStack.Push(new StringVar(previousSetExitLabel));
        LogRuntimeException(187);
    }
}