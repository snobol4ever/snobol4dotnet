namespace Snobol4.Common;


// TODO Implement for functions

public partial class Executive
{
    public void CreateAtPattern(List<Var> arguments)
    {
        var v0 = arguments[0];

        SystemStack.Push(new PatternVar(new AtSign(v0, this)));
    }
}