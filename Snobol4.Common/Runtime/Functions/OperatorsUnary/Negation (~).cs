namespace Snobol4.Common;

public partial class Executive
{
                        internal void Negation(List<Var> arguments)
    {
        var v = SystemStack.Peek();
        v.Succeeded = !v.Succeeded;
        Failure = !Failure;
    }
}