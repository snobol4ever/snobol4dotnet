namespace Snobol4.Common;

//"setexit argument is not label name or null" /* 187 */,

public partial class Executive
{
    public string SetExitLabel = "";
    public int SetExitNumber;

    // A handler that is a system label (SETEXIT(.CONTINUE), SETEXIT('RETURN'), SETEXIT('ABORT')): its exit code, -1 END to -7
    // SCONTINUE, 0 for none. It has no statement to run, so it acts at once: the error is taken as that goto would take it (SystemExit)
    internal int SetExitSystemCode;

    internal void SetExit(List<Var> arguments)
    {
        // the handler it replaces comes back as the name of its label, as in SPITBOL; none is the null string
        Var previous = SetExitLabel.Length == 0 ? new StringVar("") : new NameVar(SetExitLabel, null, null);

        // SETEXIT() cancels the handler, as in SPITBOL: an error after it is no trap (the number an error arms its trap from was left)
        if (arguments[0] is StringVar str && str.Data.Length == 0)
        {
            SystemStack.Push(previous);
            SetExitLabel = "";
            SetExitNumber = 0;
            SetExitSystemCode = 0;
            return;
        }

        if (arguments[0].Convert(VarType.NAME, out var name, out _, this))
        {
            // a label read from a string folds as &CASE says at that moment, as in SPITBOL: with &CASE 0 under -b, SETEXIT('lab1') finds
            // no label LAB1 (187); a name is the name it is (SETEXIT(.lab1) with &CASE 1 under -f is lab1's)
            var label = arguments[0] is NameVar ? ((NameVar)name).Pointer : FoldName(((NameVar)name).Pointer);

            // (a label that is not there leaves the handler as it was, and the error is the old handler's to take)
            if (LabelTable.TryGetValue(label, out var number))
            {
                SetExitNumber = number >= 0 ? number : 0;
                SetExitSystemCode = number >= 0 ? 0 : number;
                SetExitLabel = label;
                SystemStack.Push(previous);
                return;
            }
        }

        LogRuntimeException(187);
    }
}