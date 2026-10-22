namespace Snobol4.Common;

//"lpad third argument is not a string" /* 144 */,
//"lpad second argument is not integer" /* 145 */,
//"lpad first argument is not a string" /* 146 */,

public partial class Executive
{
    internal void PadLeft(List<Var> arguments)
    {
        // SPITBOL takes the pad character, then the length, then the string: that is the order of its errors
        if (!arguments[2].Convert(VarType.STRING, out _, out var padString, this))
        {
            LogRuntimeException(144);
            return;
        }

        // a length that is negative or too large to be one pads nothing
        if (!SmallInteger(arguments[1], out var totalWidth, out var inRange))
        {
            LogRuntimeException(145);
            return;
        }

        if (!inRange)
            totalWidth = 0;

        if (!arguments[0].Convert(VarType.STRING, out _, out var inputString, this))
        {
            LogRuntimeException(146);
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

        SystemStack.Push(new StringVar(text.PadLeft((int)totalWidth, paddingChar)));
    }
}
