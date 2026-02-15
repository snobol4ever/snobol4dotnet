namespace Snobol4.Common;

//"local second argument is not integer" /* 134 */,
//"local first arg is not a program function name" /* 135 */,

public partial class Executive
{
    public void Local(List<Var> arguments)
    {
        //Debug.WriteLine("Local()");
        if (!arguments[0].Convert(VarType.STRING, out _, out var functionName, this) || (string)functionName == "")
        {
            LogRuntimeException(60);
            return;
        }

        functionName = Parent.FoldCase("Local", (string)functionName);

        if (!UserFunctionTable.TryGetValue((string)functionName, out var entry))
        {
            LogRuntimeException(135);
            return;
        }

        if (!arguments[1].Convert(VarType.INTEGER, out _, out var i, this))
        {
            LogRuntimeException(134);
            return;
        }

        // Fail if index is out of range
        var j = (int)(long)i - 1;
        if (j >= entry.Locals.Count || j < 0)
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(new StringVar(entry.Locals[j]));
    }
}