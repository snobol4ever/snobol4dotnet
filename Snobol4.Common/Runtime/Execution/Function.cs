using System.Diagnostics;

namespace Snobol4.Common;

public partial class Executive
{
    /// <summary>
    /// Function call for named functions (e.g, remdr(), pos(), any())
    /// These are delegates that are called by the matching string reference
    /// stored in the FunctionTable. This method appears unused because
    /// it is called solely by the compiled assembly.
    ///
    /// For these functions, the stack is popped according to the number
    /// of arguments entered by the programmer. If too few arguments were entered,
    /// they are filled with empty strings. If too many arguments were entered,
    /// the extras are truncated. The method pushes arguments back onto the stack
    /// in right to left order.
    /// </summary>
    /// <param name="argumentCount">Number of supplied arguments</param>
    public void Function(int argumentCount)
    {
        Stopwatch timer = Stopwatch.StartNew();

        if (Builder.TraceStatements)
            Console.Error.WriteLine($@"Function {argumentCount}");

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

        // Invoke the function obtained from the Function Table
        functionEntry.Handler(arguments);
    }
}