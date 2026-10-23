namespace Snobol4.Common;

public partial class Executive
{
                        internal void Negation(List<Var> arguments)
    {
        ReadInputOperand();
        var operand = SystemStack.Count - 1;

        // A failure that began before this operand (F(1) ~F(2) with F failing) fails the statement, as in
        // SPITBOL, where evaluation stops at the first failure; only the operand's own failure is negated, and not an error's
        // while a SETEXIT handler waits for it ('a' + ~$Q enters the handler with 239) or in an evaluated expression (ErrorEndsEvaluation)
        if (Failure && (FailureOrigin < operand || ErrorEndsEvaluation))
            return;

        // The operand's value is replaced, never changed, as in SPITBOL: a failed operand gives the null string (so ~~X is the null
        // string) and one that succeeded fails. Toggling the value's own flag marked the very value a variable holds as failed, so
        // X read as failed after ~X, and ~~X gave X
        SystemStack.Pop();
        if (Failure)
        {
            Failure = false;
            FailureOrigin = int.MaxValue;
            SystemStack.Push(StringVar.Null());
        }
        else
            NonExceptionFailure();
    }

    // ~ and ? take their operand off the stack themselves, so an input-associated variable there (?INPUT,
    // ~F) is read here, as any operand would be; end of input is the operand's failure
    private void ReadInputOperand()
    {
        if (Failure || SystemStack.Peek().InputChannel == "")
            return;

        var read = InputArgument(SystemStack.Pop());
        if (!Failure)
            SystemStack.Push(read);
    }
}