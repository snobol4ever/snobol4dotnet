namespace Snobol4.Common;

//"rpad third argument is not a string" /* 178 */,
//"rpad second argument is not integer" /* 179 */,
//"rpad first argument is not a string" /* 180 */,

public partial class Executive
{
    internal void PadRight(List<Var> arguments)
    {
        // SPITBOL takes the pad character, then the length, then the string: that is the order of its errors
        if (!arguments[2].Convert(VarType.STRING, out _, out var padString, this))
        {
            LogRuntimeException(178);
            return;
        }

        // a length that is negative or too large to be one pads nothing
        if (!SmallInteger(arguments[1], out var totalWidth, out var inRange))
        {
            LogRuntimeException(179);
            return;
        }

        if (!inRange)
            totalWidth = 0;

        if (!arguments[0].Convert(VarType.STRING, out _, out var inputString, this))
        {
            LogRuntimeException(180);
            return;
        }

        var text = (string)inputString;
        if (text.Length >= totalWidth)
        {
            SystemStack.Push(new StringVar(text));
            return;
        }

        if (StringTooLong(totalWidth))
            return;

        var paddingChar = ((string)padString).Length == 0 ? ' ' : ((string)padString)[0];

        SystemStack.Push(new StringVar(text.PadRight((int)totalWidth, paddingChar)));
    }
}
