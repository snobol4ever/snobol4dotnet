namespace Snobol4.Common;


// TODO Implement for functions

public partial class Executive
{
    public void CreateAtPattern(List<Var> arguments)
    {
        var v0 = arguments[0];

        // a call in the place of the name that handed back a value by RETURN is error 21 as the pattern is built, as in SPITBOL (@H(2))
        if (ValueInNamesPlace(v0) is var valueError and not 0)
        {
            LogRuntimeException(valueError);
            return;
        }

        // so is a builtin's value (@SIZE('a')): no name, unless it is a deferred expression, found at match time
        if (v0 is not ExpressionVar && !IsName(v0))
        {
            LogRuntimeException(21);
            return;
        }

        SystemStack.Push(new PatternVar(new AtSign(v0, this)));
    }
}