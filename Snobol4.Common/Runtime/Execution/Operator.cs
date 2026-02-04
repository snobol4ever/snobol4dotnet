namespace Snobol4.Common;

public partial class Executive
{
    /// <summary>
    /// Function call for symbolic operators
    /// These are delegates that are called by the matching string reference
    /// stored in the FunctionTable. This method appears unused because
    /// it is called solely by the compiled assembly. 
    /// </summary>
    /// <param name="functionName">string name of operator</param>
    /// <param name="argumentCount">number of operands</param>
    // ReSharper disable once UnusedMember.Global
    public void Operator(string functionName, int argumentCount)
    {
        if (Parent.TraceStatements)
            Console.Error.WriteLine($@"Operator {functionName} {argumentCount}");

        // Get all arguments and check for prior failure
        List<Var> arguments = [];
        if (SystemStack.ExtractArguments(argumentCount, arguments, this))
            return;

        // If any arguments have input channels, get input now
        InputArguments(arguments);

        FunctionTable[functionName].Handler(arguments);
    }

    public void Undefined(List<Var> arguments)
    {
        LogRuntimeException(29);
    }


}