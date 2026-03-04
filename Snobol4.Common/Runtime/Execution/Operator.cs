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

        // If any arguments have input channels, get input now
        InputArguments(arguments);

        FunctionTable[functionName]!.Handler(arguments);
    }

    public void Undefined(List<Var> arguments)
    {
        LogRuntimeException(29);
    }


}