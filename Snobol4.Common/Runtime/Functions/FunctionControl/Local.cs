namespace Snobol4.Common;

//"local second argument is not integer" /* 134 */,
//"local first arg is not a program function name" /* 135 */,

public partial class Executive
{
    public void Local(List<Var> arguments)
    {
        // SPITBOL takes the second argument first: a number out of range fails before the first one is looked at
        if (!SmallInteger(arguments[1], out var i, out var inRange))
        {
            LogRuntimeException(134);
            return;
        }

        if (!inRange)
        {
            NonExceptionFailure();
            return;
        }

        if (!NaturalVariableName(arguments[0], out var functionName) || !UserFunctionTable.TryGetValue(functionName, out var entry))
        {
            LogRuntimeException(135);
            return;
        }

        // Fail if index is out of range
        if (i > entry.Locals.Count || i <= 0)
        {
            NonExceptionFailure();
            return;
        }

        // the local's NAME, as in SPITBOL (DATATYPE(LOCAL(.F,1)) is NAME)
        SystemStack.Push(new NameVar(entry.Locals[(int)i - 1], null, null));
    }
}
