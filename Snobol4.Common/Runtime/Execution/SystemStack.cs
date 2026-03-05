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
        
        base.Push(x.FailureSentinel);
        return true;

    }
}