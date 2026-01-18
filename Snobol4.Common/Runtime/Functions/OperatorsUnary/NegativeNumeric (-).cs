namespace Snobol4.Common;

//"negation operand is not numeric" /* 10 */,
//"negation caused integer overflow" /* 11 */,

public partial class Executive
{
    internal void UnaryMinus(List<Var> arguments)
    {
        if (!Var.ToNumeric(arguments[0], out var isInteger, out var l, out var d, this))
        {
            LogRuntimeException(10);
            return;
        }

        if (isInteger)
        {
            try
            {
                checked
                {
                    SystemStack.Push(new IntegerVar(-l));
                    return;
                }
            }
            catch
            {
                LogRuntimeException(11);
                return;
            }
        }

        SystemStack.Push(new RealVar(-d));
    }
}