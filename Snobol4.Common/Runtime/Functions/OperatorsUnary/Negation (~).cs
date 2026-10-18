namespace Snobol4.Common;

public partial class Executive
{
                        internal void Negation(List<Var> arguments)
    {
        var v = SystemStack.Peek();
        var operand = SystemStack.Count - 1;

        // A failure that began before this operand (F(1) ~F(2) with F failing) fails the statement, as in
        // SPITBOL, where evaluation stops at the first failure; only the operand's own failure is negated
        if (Failure && FailureOrigin < operand)
            return;

        // the value's own flag follows the statement's state: a FRETURN pushes a null value still marked
        // succeeded, which toggling turned into a failed value under a succeeding statement
        Failure = !Failure;
        v.Succeeded = !Failure;
        FailureOrigin = Failure ? operand : int.MaxValue;
    }
}