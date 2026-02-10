namespace Snobol4.Common;

public partial class Executive
{
                                                        public void Function(int argumentCount)
    {
        using var profiler1 = Profiler.Start3($"Function", this);

        // Get all arguments and check for prior failure
        List<Var> arguments = [];
        if (SystemStack.ExtractArguments(argumentCount, arguments, this))
        {
            return;
        }

        var functionStringVar = (StringVar)SystemStack.Pop();
        // Function name must be in the symbol table
        if (!FunctionTable.TryGetValue(functionStringVar.Data, out var functionEntry))
        {
            // 22 undefined function called
            LogRuntimeException(22);
            return;
        }

        // Fill in missing arguments with empty strings
        for (var i = arguments.Count; i < functionEntry.ArgumentCount; ++i)
            arguments.Add(StringVar.Null());

        // If any arguments have input channels, get input now
        InputArguments(arguments);

        // Add argument for name of function
        arguments.Add(functionStringVar);

        profiler1?.Dispose();
        using var profiler2 = Profiler.Start3($"F_{functionStringVar.Data}", this);

        // Invoke the function obtained from the Function Table
        functionEntry.Handler(arguments);
    }
}