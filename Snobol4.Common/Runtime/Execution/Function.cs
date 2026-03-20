namespace Snobol4.Common;

public partial class Executive
{
    public void Function(int argumentCount)
    {
        using var profiler1 = Profiler.Start3($"Function", this);

        // Don't execute function if there was already a failure in this statement
        if (Failure)
        {
            return;
        }

        // Get all arguments and check for prior failure.
        // Reuse the pre-allocated arg list to avoid per-call heap allocation.
        var arguments = _reusableArgList;
        arguments.Clear();
        if (SystemStack.ExtractArguments(argumentCount, arguments, this))
        {
            return;
        }

        var functionStringVar = (StringVar)SystemStack.Pop();
        // Function name must be in the symbol table
        var functionEntry = FunctionTable[functionStringVar.Data];
        //if (!FunctionTable.TryGetValue(functionStringVar.Data, out var functionEntry))
        if (functionEntry == null)
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

    /// <summary>
    /// Indirect function call: <c>$VAR(args)</c>.
    /// The function name is determined at runtime by resolving the value already
    /// pushed by <c>OpIndirection</c>.  Stack layout when called:
    ///   [ resolved-name-Var ] [ arg0 ] … [ argN-1 ]   (top = argN-1)
    /// </summary>
    public void FunctionIndirect(int argumentCount)
    {
        if (Failure)
            return;

        var arguments = _reusableArgList;
        arguments.Clear();
        if (SystemStack.ExtractArguments(argumentCount, arguments, this))
            return;

        // The name was resolved by OpIndirection — pop and convert to string.
        var nameVar = SystemStack.Pop();
        var functionName = Parent.FoldCase(nameVar.ToString() ?? "");

        if (string.IsNullOrEmpty(functionName))
        {
            LogRuntimeException(22);
            return;
        }

        var functionStringVar = new StringVar(functionName);
        var functionEntry = FunctionTable[functionName];
        if (functionEntry == null)
        {
            LogRuntimeException(22);
            return;
        }

        for (var i = arguments.Count; i < functionEntry.ArgumentCount; ++i)
            arguments.Add(StringVar.Null());

        InputArguments(arguments);
        arguments.Add(functionStringVar);

        functionEntry.Handler(arguments);
    }
}