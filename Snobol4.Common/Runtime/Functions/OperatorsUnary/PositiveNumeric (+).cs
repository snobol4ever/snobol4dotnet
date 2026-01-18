namespace Snobol4.Common;

//"affirmation operand is not numeric" /* 4 */,

public partial class Executive
{
    internal void UnaryPlus(List<Var> arguments)
    {
        if (!Var.ToNumeric(arguments[0], out var isInteger, out var l, out var d, this))
        {
            LogRuntimeException(4);
            return;
        }

        if (isInteger)
        {
            SystemStack.Push(new IntegerVar(l));
            return;
        }

        SystemStack.Push(new RealVar(-d));
    }
}