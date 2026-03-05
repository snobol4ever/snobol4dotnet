namespace Snobol4.Common;

public partial class Executive
{
                        internal void Negation(List<Var> arguments)
    {
        var v = SystemStack.Peek();
        // If the top of stack is a shared sentinel, we must not mutate it in-place.
        // Replace it with a fresh StringVar carrying the flipped flag.
        if (v.IsReadOnly)
        {
            SystemStack.Pop();
            SystemStack.Push(new StringVar(!v.Succeeded));
        }
        else
        {
            v.Succeeded = !v.Succeeded;
        }
        Failure = !Failure;
    }
}