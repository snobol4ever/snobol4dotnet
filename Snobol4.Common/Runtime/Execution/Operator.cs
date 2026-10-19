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

        // Get all arguments and check for prior failure
        List<Var> arguments = [];
        if (SystemStack.ExtractArguments(argumentCount, arguments, this))
        {
            return;
        }

        // If any arguments have input channels, get input now; the name operator takes its operand's
        // name, so .INPUT reads no line, as in SPITBOL
        if (functionName != "_.")
            InputArguments(arguments);

        var handler = FunctionTable[functionName]!.Handler;
        // a program-defined function reads its name from the last argument, which Function() appends on
        // a call; an operator OPSYN'd to one (| or ! in CODE or EVAL text) appends it here, as OperatorFast does
        if (handler == ExecuteProgramDefinedFunction)
            arguments.Add(new StringVar(UserFunctionTable.TryGetValue(functionName, out var definition)
                ? definition.FunctionName : functionName));
        handler(arguments);
    }

    public void Undefined(List<Var> arguments)
    {
        LogRuntimeException(29);
    }


}