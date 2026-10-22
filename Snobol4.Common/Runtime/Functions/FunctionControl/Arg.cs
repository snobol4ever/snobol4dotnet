namespace Snobol4.Common;

public partial class Executive
{
    //"arg second argument is not integer" /* 62 */,
    //"arg first argument is not program function name" /* 63 */,

    public void Arg(List<Var> arguments)
    {
        // SPITBOL takes the second argument first: a number out of range fails before the first one is looked at
        if (!SmallInteger(arguments[1], out var i, out var inRange))
        {
            LogRuntimeException(62);
            return;
        }

        if (!inRange)
        {
            NonExceptionFailure();
            return;
        }

        if (!NaturalVariableName(arguments[0], out var functionName) || !UserFunctionTable.TryGetValue(functionName, out var entry))
        {
            LogRuntimeException(63);
            return;
        }

        // Fail if index is out of range
        if (i > entry.Parameters.Count || i <= 0)
        {
            NonExceptionFailure();
            return;
        }

        // the parameter's NAME, as in SPITBOL (DATATYPE(ARG(.F,1)) is NAME)
        SystemStack.Push(new NameVar(entry.Parameters[(int)i - 1], null, null));
    }
}
