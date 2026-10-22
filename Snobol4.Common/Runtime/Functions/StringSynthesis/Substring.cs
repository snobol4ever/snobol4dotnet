namespace Snobol4.Common;

//"substr third argument is not integer" /* 192 */,
//"substr second argument is not integer" /* 193 */,
//"substr first argument is not a string" /* 194 */,

public partial class Executive
{
    internal void Substring(List<Var> arguments)
    {
        // SPITBOL takes the third argument off the stack first, then the second, then the first: that is the order of
        // its errors, and a third or second argument out of range fails before the first one is looked at
        if (!SmallInteger(arguments[2], out var length, out var lengthInRange))
        {
            LogRuntimeException(192);
            return;
        }

        if (!lengthInRange)
        {
            NonExceptionFailure();
            return;
        }

        if (!SmallInteger(arguments[1], out var start, out var startInRange))
        {
            LogRuntimeException(193);
            return;
        }

        if (!startInRange || start == 0)
        {
            NonExceptionFailure();
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var inputString, this))
        {
            LogRuntimeException(194);
            return;
        }

        var text = (string)inputString;
        var offset = (int)start - 1;
        var count = (int)length;
        if (count == 0)
        {
            // no third argument (or zero): the rest of the string, which is null when the offset is just past its end
            if (offset > text.Length)
            {
                NonExceptionFailure();
                return;
            }

            count = text.Length - offset;
        }

        if (offset + count > text.Length)
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(new StringVar(text.Substring(offset, count)));
    }
}
