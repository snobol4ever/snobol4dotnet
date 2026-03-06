namespace Snobol4.Common;

public class SystemStack : Stack<Var>
{
    public new void Push(Var v)
    {
        base.Push(v);
    }

    public new Var Pop()
    {
        var v = base.Pop();
        return v;
    }

    public Var Peek(int count = 0)
    {
        var v = this.ElementAt(count);
        return v;
    }

    public bool ExtractArguments(int count, List<Var> arguments, Executive x, int start = 0)
    {
        for (var i = 0; i < count; ++i)
            arguments.Insert(0, base.Pop());

        if (arguments.All(arg => arg.Succeeded)) 
            return false;
        
        base.Push(new StringVar(false));
        return true;
    }

    /// <summary>
    /// Array-based variant for the hot path — avoids List allocation.
    /// Fills <paramref name="buf"/> in argument order (index 0 = first arg).
    /// Returns true if any argument failed (Succeeded == false), pushing a
    /// null StringVar sentinel and signalling the caller to abort.
    /// </summary>
    public bool ExtractArgumentsToArray(int count, Var[] buf)
    {
        for (var i = count - 1; i >= 0; --i)
            buf[i] = base.Pop();

        for (var i = 0; i < count; ++i)
            if (!buf[i].Succeeded)
            {
                base.Push(new StringVar(false));
                return true;
            }

        return false;
    }
}