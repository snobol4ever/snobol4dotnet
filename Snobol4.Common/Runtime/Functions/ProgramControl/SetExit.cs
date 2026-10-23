namespace Snobol4.Common;

//"setexit argument is not label name or null" /* 187 */,

public partial class Executive
{
    public string SetExitLabel = "";
    public int SetExitNumber;

    internal void SetExit(List<Var> arguments)
    {
        var previousSetExitLabel = SetExitLabel;

        // SETEXIT() cancels the handler, as in SPITBOL: an error after it is no trap (the number an error arms its trap from was left)
        if (arguments[0] is StringVar str && str.Data.Length == 0)
        {
            SystemStack.Push(new StringVar(previousSetExitLabel));
            SetExitLabel = "";
            SetExitNumber = 0;
            return;
        }

        if (arguments[0].Convert(VarType.NAME, out var name, out _, this))
        {
            var label = Parent.FoldCase(((NameVar)name).Pointer);

            // (a label that is not there leaves the handler as it was, and the error is the old handler's to take)
            if (LabelTable.TryGetValue(label, out var number))
            {
                SetExitNumber = number;
                SetExitLabel = label;
                SystemStack.Push(new StringVar(previousSetExitLabel));
                return;
            }
        }

        LogRuntimeException(187);
    }
}