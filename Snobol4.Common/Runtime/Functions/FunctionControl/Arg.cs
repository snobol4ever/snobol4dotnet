namespace Snobol4.Common;

public partial class Executive
{
    //"arg second argument is not integer" /* 62 */,
    //"arg first argument is not program function name" /* 63 */,

    public void Arg(List<Var> arguments)
    {
        //Debug.WriteLine("Arg()");
        if (!arguments[0].Convert(VarType.STRING, out _, out var str, this) || (string)str == "")
        {
            LogRuntimeException(60);
            return;
        }

        if (!UserFunctionDefinitions.TryGetValue((string)str, out var entry))
        {
            LogRuntimeException(63);
            return;
        }

        if (!arguments[1].Convert(VarType.INTEGER, out _, out var i, this))
        {
            LogRuntimeException(62);
            return;
        }

        // Fail if index is out of range
        if ((long)i > entry.Parameters.Count || (long)i <= 0)
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(new StringVar(entry.Parameters[(int)(long)i - 1]));
    }
}