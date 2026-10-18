namespace Snobol4.Common;

//"ord argument is not a string" /* 333 */,

public partial class Executive
{
    // ORD(s): the character code of the first character of s, as SPITBOL's s_ord answers it; the null
    // string fails
    internal void Ord(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this))
        {
            LogRuntimeException(333);
            return;
        }

        var text = (string)s;
        if (text.Length == 0)
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(new IntegerVar(text[0]));
    }
}
