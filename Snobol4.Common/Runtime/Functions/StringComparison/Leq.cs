namespace Snobol4.Common;

//"leq first argument is not a string" /* 122 */,
//"leq second argument is not a string" /* 123 */,

public partial class Executive
{
    internal void LexicalEqual(List<Var> arguments)
    {
        var v0 = arguments[0];
        var v1 = arguments[1];

        // the second argument is converted first, as SPITBOL takes the arguments off its stack, so when neither is a string the
        // error is the second's
        if (!v1.Convert(VarType.STRING, out _, out var right, this))
        {
            LogRuntimeException(123);
            return;
        }

        if (!v0.Convert(VarType.STRING, out _, out var left, this))
        {
            LogRuntimeException(122);
            return;
        }

        if (string.CompareOrdinal((string)left, (string)right) != 0)
        {
            NonExceptionFailure();
            return;
        }

        PredicateSuccess();
    }
}