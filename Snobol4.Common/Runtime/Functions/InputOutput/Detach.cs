namespace Snobol4.Common;

//"detach argument is not appropriate name" /* 87 */,

public partial class Executive
{
    internal void Detach(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var value, this))
        {
            LogRuntimeException(87);
            return;
        }

        var symbol =(string)value;

        if (symbol == "")
        {
            LogRuntimeException(87);
            return;
        }

        // Only the variable named is detached; another variable on the same channel keeps its association (sbl).
        var variable = IdentifierTable[symbol];
        variable.InputChannel = "";
        variable.OutputChannel = "";

        PredicateSuccess();
    }
}