namespace Snobol4.Common;

public partial class Executive
{
    // Set while the compiler pre-evaluates an operator or function whose operands are all constants, as SPITBOL does: a run-time
    // error then records its number and ends the call, and the compiler reports it as an error in the statement it is compiling
    internal bool PreEvaluating;
    internal int PreEvaluationCode;

    private sealed class PreEvaluationAbandoned : Exception;

    // Calls a builtin on constants at compile time and returns its value, or null when it raised an error (its number in
    // `code`) or failed. Nothing of the call is left behind: the stack and the failure flag are as they were.
    internal Var? PreEvaluate(FunctionTableEntry entry, List<Var> arguments, out int code)
    {
        var depth = SystemStack.Count;
        var failure = Failure;
        Failure = false;
        PreEvaluating = true;
        PreEvaluationCode = 0;
        Var? result = null;
        try
        {
            entry.Handler(arguments);
            if (PreEvaluationCode == 0 && !Failure && SystemStack.Count == depth + 1)
                result = SystemStack.Peek();
        }
        catch (PreEvaluationAbandoned)
        {
        }
        finally
        {
            PreEvaluating = false;
            while (SystemStack.Count > depth)
                SystemStack.Pop();
            Failure = failure;
        }

        code = PreEvaluationCode;
        return code == 0 ? result : null;
    }
}
