namespace Snobol4.Common;

//"size argument is not a string" /* 189 */,

public partial class Executive
{
    public void Size(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var str, this))
        {
            LogRuntimeException(189);
            return;
        }

        SystemStack.Push(new IntegerVar(((string)str).Length));
    }
}