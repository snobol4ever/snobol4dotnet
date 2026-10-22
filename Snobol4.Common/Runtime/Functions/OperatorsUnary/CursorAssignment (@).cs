namespace Snobol4.Common;


// TODO Implement for functions

public partial class Executive
{
    public void CreateAtPattern(List<Var> arguments)
    {
        var v0 = arguments[0];

        // a call in the place of the name that handed back a value by RETURN is error 21 as the pattern is built, as in SPITBOL (@H(2))
        if (ReferenceEquals(v0, ValueReturned))
        {
            LogRuntimeException(21);
            return;
        }

        SystemStack.Push(new PatternVar(new AtSign(v0, this)));
    }
}