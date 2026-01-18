namespace Snobol4.Common;

public partial class Executive
{
    /// <summary>
    /// Executes the negation operation (unary ~). If X is an
    /// expression which fails, ~X succeeds.
    /// if X succeeds, ~X fails, returning the null string.
    /// </summary>
    internal void Negation(List<Var> arguments)
    {
        var v = SystemStack.Peek();
        v.Succeeded = !v.Succeeded;
        Failure = !Failure;
    }
}