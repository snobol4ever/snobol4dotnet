namespace Snobol4.Common;

public partial class Executive
{
                                    // ReSharper disable once UnusedMember.Global
    public void Operator(string functionName, int argumentCount)
    {
        using var profiler = Profiler.Start3($"Op_{functionName}", this);

        if (Parent.BuildOptions.TraceStatements)
        {
            Console.Error.WriteLine($@"Operator {functionName} {argumentCount}");
        }

        // Get all arguments and check for prior failure; an operand that already failed (a FRETURN's
        // null value is still marked succeeded) fails the statement before the operator runs
        List<Var> arguments = [];
        if (Failure && argumentCount > 0)
        {
            for (var i = 0; i < argumentCount; ++i) SystemStack.Pop();
            SystemStack.Push(new StringVar(false) { Succeeded = false });
            return;
        }
        if (SystemStack.ExtractArguments(argumentCount, arguments, this))
        {
            return;
        }

        // If any arguments have input channels, get input now; the name operator takes its operand's
        // name, so .INPUT reads no line, as in SPITBOL, and nor does the keyword operator, &INPUT
        if (functionName is not ("_." or "_&") && InputArguments(arguments, functionName is "__." or "__$"))
            return;

        var handler = FunctionTable[functionName]!.Handler;
        // a program-defined function reads its name from the last argument, which Function() appends on
        // a call; an operator OPSYN'd to one (| or ! in CODE or EVAL text) appends it here, as OperatorFast does
        if (handler == ExecuteProgramDefinedFunction)
        {
            // the function is given the arguments it declares: the operands it lacks are null, the ones it has too many are dropped
            if (UserFunctionTable.TryGetValue(functionName, out var definition))
            {
                while (arguments.Count < definition.Parameters.Count)
                    arguments.Add(StringVar.Null());
                if (arguments.Count > definition.Parameters.Count)
                    arguments.RemoveRange(definition.Parameters.Count, arguments.Count - definition.Parameters.Count);
            }

            arguments.Add(new StringVar(UserFunctionTable.TryGetValue(functionName, out definition)
                ? definition.FunctionName : functionName));
        }

        handler(arguments);
    }

    public void Undefined(List<Var> arguments)
    {
        LogRuntimeException(29);
    }


}